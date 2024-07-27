namespace HexaEngine.Core.Assets.Importer
{
    using Hexa.NET.Logging;
    using Hexa.NET.Mathematics;
    using Hexa.NET.Utilities;
    using Hexa.NET.Utilities.Collections;
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Animations;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Core.IO.Binary.Meshes;
    using HexaEngine.Core.IO.Binary.Meshes.Processing;
    using HexaEngine.Core.UI;
    using Silk.NET.Assimp;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using System.Text;
    using AssimpMaterialProperty = Silk.NET.Assimp.MaterialProperty;
    using AssimpNode = Silk.NET.Assimp.Node;
    using AssimpScene = Silk.NET.Assimp.Scene;
    using BlendMode = Silk.NET.Assimp.BlendMode;
    using MaterialProperty = IO.Binary.Materials.MaterialProperty;
    using Node = IO.Binary.Meshes.Node;
    using TextureFlags = Silk.NET.Assimp.TextureFlags;
    using TextureMapMode = Silk.NET.Assimp.TextureMapMode;
    using TextureOp = Silk.NET.Assimp.TextureOp;
    using TextureType = Silk.NET.Assimp.TextureType;

    public class ModelImporter : IAssetImporter
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(ModelImporter));
        private static readonly Assimp assimp = Assimp.GetApi();

        public Type? SettingsType { get; } = typeof(ModelImporterSettings);

        public string SettingsKey { get; } = "ModelImportSettings";

        public string? SettingsDisplayName { get; } = "Model Import Settings";

        static unsafe ModelImporter()
        {
            LogStream stream = new(new(Log));
            assimp.AttachLogStream(&stream);
#if DEBUG
            assimp.EnableVerboseLogging(Assimp.True);
#endif
        }

        public bool CanImport(ReadOnlySpan<char> fileExtension)
        {
            return fileExtension switch
            {
                ".3d" => true,
                ".3ds" => true,
                ".3mf" => true,
                ".ac" => true,
                ".ac3d" => true,
                ".acc" => true,
                ".amj" => true,
                ".ase" => true,
                ".ask" => true,
                ".b3d" => true,
                ".bvh" => true,
                ".csm" => true,
                ".cob" => true,
                ".dae" => true,
                ".dxf" => true,
                ".enff" => true,
                ".fbx" => true,
                ".glb" => true,
                ".gltf" => true,
                ".hmb" => true,
                ".ifc" => true,
                ".irr" => true,
                ".lwo" => true,
                ".lws" => true,
                ".lxo" => true,
                ".m3d" => true,
                ".md2" => true,
                ".md3" => true,
                ".md5" => true,
                ".mdc" => true,
                ".mdl" => true,
                ".mesh" => true,
                ".mot" => true,
                ".ms3d" => true,
                ".ndo" => true,
                ".nff" => true,
                ".obj" => true,
                ".off" => true,
                ".ogex" => true,
                ".ply" => true,
                ".pmx" => true,
                ".prj" => true,
                ".q3o" => true,
                ".q3s" => true,
                ".raw" => true,
                ".scn" => true,
                ".sib" => true,
                ".smd" => true,
                ".stp" => true,
                ".stl" => true,
                ".ter" => true,
                ".uc" => true,
                ".vta" => true,
                ".x" => true,
                ".x3d" => true,
                ".xgl" => true,
                ".zgl" => true,
                _ => false
            };
        }

        /// <summary>
        /// Checks the content of a file, used for updating file formats.
        /// </summary>
        /// <param name="artifact">The path to the asset.</param>
        /// <returns><c>true</c> if content needs to be reimported/updated, otherwise <c>false</c>.</returns>
        public bool RefreshContent(Artifact artifact)
        {
            Stream? stream = null;

            if (artifact.Type != AssetType.Model)
            {
                return false;
            }

            try
            {
                stream = System.IO.File.OpenRead(artifact.Path);

                ModelHeader header = default;
                if (!header.TryRead(stream))
                {
                    return true; // indicates corruption needs reimport.
                }

                return header.FileVersion != ModelHeader.Version; // if the file format is not up-to-date a update is needed.
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            finally
            {
                stream?.Close();
            }

            return false;
        }

        public unsafe void Import(TargetPlatform targetPlatform, ImportContext context)
        {
            ModelImporterSettings settings = context.GetOrCreateAdditionalMetadata<ModelImporterSettings>(SettingsKey);

            Logger.Info($"Importing model '{Path.GetFileName(context.SourcePath)}'.");

            AssimpScene* scene = null;
            if (!Load(targetPlatform, context, settings, &scene))
            {
                Logger.Error($"Failed to import {context.SourcePath}");
                if (scene != null)
                {
                    assimp.ReleaseImport(scene);
                    return;
                }
            }

            if (scene != null)
            {
                assimp.ReleaseImport(scene);
            }

            Logger.Info($"Imported model '{Path.GetFileName(context.SourcePath)}'.");
        }

        public unsafe bool Load(TargetPlatform targetPlatform, ImportContext context, ModelImporterSettings settings, AssimpScene** outScene)
        {
            var modelName = Path.GetFileNameWithoutExtension(context.SourcePath);
            var importDir = Path.GetDirectoryName(context.SourcePath);
            var sourceDir = context.ImportSourcePath != null ? Path.GetDirectoryName(context.ImportSourcePath) : Path.GetDirectoryName(context.SourcePath);

            if (importDir == null)
            {
                throw new InvalidOperationException($"Cannot continue '{nameof(importDir)}' is null.");
            }

            if (sourceDir == null)
            {
                throw new InvalidOperationException($"Cannot continue '{nameof(sourceDir)}' is null.");
            }

            if (Path.GetExtension(context.SourcePath) == ".gltf")
            {
                string path = Directory.EnumerateFiles(sourceDir, $"{modelName}*.bin").First();

                if (System.IO.File.Exists(path))
                {
                    SourceAssetsDatabase.ImportFile(path, new DefaultGuidProvider(context.AssetMetadata.Guid));
                }
            }

            var scene = assimp.ImportFile(context.SourcePath, (uint)(ImporterFlags.SupportBinaryFlavour | ImporterFlags.SupportCompressedFlavour | ImporterFlags.SupportTextFlavour));
            *outScene = scene;

            if (scene == null)
            {
                Logger.Error($"Failed to load {context.SourcePath}");
                MessageBox.Show($"Failed to load scene", $"Failed to load {context.SourcePath}");
                return false;
            }

            scene = assimp.ApplyPostProcessing(scene, (uint)settings.PostProcessSteps);

            float unitScaleFactor = 1;

            if (scene->MMetaData != null)
            {
                var metadata = scene->MMetaData;
                for (int i = 0; i < metadata->MNumProperties; i++)
                {
                    string key = metadata->MKeys[i];
                    if (key == "UnitScaleFactor")
                    {
                        var value = metadata->MValues[i];
                        if (value.MType == MetadataType.Float)
                        {
                            unitScaleFactor = MemoryMarshal.Cast<byte, float>(new ReadOnlySpan<byte>(value.MData, 4))[0];
                        }
                    }
                }
            }

            if (scene == null)
            {
                Logger.Error($"Failed to apply post processing {context.SourcePath}");
                MessageBox.Show($"Failed to apply post processing", $"Failed to apply post processing {context.SourcePath}");
                return false;
            }

            if (!LoadSceneGraph(scene, out var root, out var nameToNode, out var pToNode))
            {
                return false;
            }

            Guid[]? materialIds = null;
            MaterialFile[]? materials = null;
            List<string>? texturePaths = [];
            Dictionary<string, Guid>? texturePathToGuid = [];

            ProgressContext progressContext = new(context, 4);

            if (settings.ImportMaterials)
            {
                progressContext.BeginSubStep((int)scene->MNumMaterials);
                if (!LoadMaterials(modelName, importDir, scene, context, progressContext, out materialIds, out materials, texturePaths, texturePathToGuid))
                {
                    return false;
                }
            }
            progressContext.EndSubStep();

            progressContext.BeginSubStep((int)scene->MNumMeshes);
            if (!LoadMeshes(modelName, scene, context, progressContext, root, pToNode, materialIds, out var meshes, out var nameToMesh, out var pToMesh))
            {
                return false;
            }
            progressContext.EndSubStep();

            if (CheckForProblems(meshes))
            {
                return false;
            }

            progressContext.BeginSubStep((int)scene->MNumAnimations);
            if (settings.ImportAnimationClips)
            {
                if (!LoadAnimations(modelName, importDir, scene, context, progressContext, pToMesh))
                {
                    return false;
                }
            }
            progressContext.EndSubStep();

            if (settings.ImportTextures)
            {
                progressContext.BeginSubStep(texturePaths.Count);
                if (!LoadTextures(targetPlatform, settings, sourceDir, importDir, scene, context, progressContext, texturePaths, texturePathToGuid))
                {
                    return false;
                }
            }
            progressContext.EndSubStep();

            return true;
        }

        private unsafe bool LoadTextures(TargetPlatform targetPlatform, ModelImporterSettings settings, string sourceDir, string outDir, AssimpScene* scene, ImportContext context, ProgressContext progressContext, List<string> texturePaths, Dictionary<string, Guid> texturePathToGuid)
        {
            var device = Application.GraphicsDevice;
            var loader = device.TextureLoader;

            for (int i = 0; i < texturePaths.Count; i++)
            {
                var texturePath = texturePaths[i];

                if (!texturePathToGuid.TryGetValue(texturePath, out var guid))
                {
                    continue;
                }

                if (texturePath.StartsWith('*'))
                {
                    var index = int.Parse(texturePath[1..]);
                    Texture* tex = scene->MTextures[index];

                    string fileName = tex->MFilename;

                    if (tex->MHeight != 0)
                    {
                        Logger.Warn($"Failed to import texture {tex->MFilename}, importer doesn't support raw pixel formats.");
                        continue;
                    }

                    var sHint = Encoding.UTF8.GetString(new Span<byte>(tex->AchFormatHint, 3));

                    IScratchImage? image = null;
                    switch (sHint)
                    {
                        case "jpg":
                            image = loader.LoadFromMemory(TexFileFormat.JPEG, (byte*)tex->PcData, tex->MWidth);
                            break;

                        case "png":
                            image = loader.LoadFromMemory(TexFileFormat.PNG, (byte*)tex->PcData, tex->MWidth);
                            break;

                        default:
                            Logger.Warn($"Failed to import texture {tex->MFilename}, importer doesn't support {sHint} formats.");
                            continue;
                    }

                    string targetPath = Path.Combine(outDir, fileName);
                    context.ImportChild(targetPath, guid, out string path);
                    image.SaveToFile(path, TexFileFormat.Auto, 0);
                    progressContext.AddProgress();
                }
                else
                {
                    string filePath = Path.Combine(sourceDir, texturePath);

                    if (!System.IO.File.Exists(filePath))
                    {
                        Logger.Warn($"Failed to import texture {filePath}, importer couldn't locate file.");
                        continue;
                    }

                    DictionaryGuidProvider provider = new(context.AssetMetadata.Guid, texturePathToGuid, GuidNotFoundBehavior.Throw);

                    string targetPath = Path.Combine(outDir, texturePath);
                    context.ImportChild(targetPath, provider, out var path);
                    System.IO.File.Copy(filePath, path, true);
                    progressContext.AddProgress();
                }
            }

            return true;
        }

        public static bool CheckForProblems(MeshData[] meshes)
        {
            for (int i = 0; i < meshes.Length; i++)
            {
                MeshData model = meshes[i];
                if (model.Name.Length > 255)
                {
                    return true;
                }
            }
            return false;
        }

        private unsafe bool LoadMaterials(string modelName, string outDir, AssimpScene* scene, ImportContext context, ProgressContext progressContext, [MaybeNullWhen(false)] out Guid[] materialIds, [MaybeNullWhen(false)] out MaterialFile[] materials, List<string> texturePaths, Dictionary<string, Guid> texturePathToGuid)
        {
            //try
            {
                materials = new MaterialFile[scene->MNumMaterials];
                materialIds = new Guid[scene->MNumMaterials];
                for (int i = 0; i < scene->MNumMaterials; i++)
                {
                    Material* mat = scene->MMaterials[i];

                    var material = materials[i] = new MaterialFile();
                    material.Guid = Guid.NewGuid();

                    List<MaterialProperty> properties = [];
                    List<MaterialTexture> textures = [];
                    List<MaterialShader> shaders = [];

                    for (int j = 0; j < mat->MNumProperties; j++)
                    {
                        AssimpMaterialProperty* prop = mat->MProperties[j];
                        if (prop == null)
                        {
                            continue;
                        }

                        Span<byte> buffer = new(prop->MData, (int)prop->MDataLength);
                        string key = prop->MKey;
                        int semantic = (int)prop->MSemantic;

                        static ref MaterialTexture FindOrCreate(List<MaterialTexture> textures, TextureType type)
                        {
                            var t = Convert(type);
                            for (int i = 0; i < textures.Count; i++)
                            {
                                var tex = textures[i];
                                if (tex.Type == t)
                                {
                                    return ref textures.GetInternalArray()[i];
                                }
                            }
                            var index = textures.Count;
                            textures.Add(new MaterialTexture() { Type = t });
                            return ref textures.GetInternalArray()[index];
                        }

                        static int FindOrCreateIdx(List<MaterialTexture> textures, Guid guid, TextureType type)
                        {
                            var t = Convert(type);
                            for (int i = 0; i < textures.Count; i++)
                            {
                                var tex = textures[i];
                                if (tex.Type == t)
                                {
                                    return i;
                                }
                            }
                            var index = textures.Count;
                            textures.Add(new MaterialTexture() { Type = t, File = guid });
                            return index;
                        }

                        switch (key)
                        {
                            case Assimp.MatkeyName:
                                material.Name = Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1));
                                break;

                            case Assimp.MatkeyTwosided:
                                properties.Add(new("TwoSided", MaterialPropertyType.TwoSided, MaterialValueType.Bool, default, sizeof(bool), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyShadingModel:
                                properties.Add(new("ShadingMode", MaterialPropertyType.ShadingMode, MaterialValueType.Int32, default, sizeof(int), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyEnableWireframe:
                                properties.Add(new("EnableWireframe", MaterialPropertyType.EnableWireframe, MaterialValueType.Bool, default, sizeof(bool), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyBlendFunc:
                                properties.Add(new("BlendFunc", MaterialPropertyType.BlendFunc, MaterialValueType.Bool, default, sizeof(bool), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyOpacity:
                                properties.Add(new("Opacity", MaterialPropertyType.Opacity, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyTransparencyfactor:
                                properties.Add(new("Transparency", MaterialPropertyType.Transparency, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyBumpscaling:
                                properties.Add(new("BumpScaling", MaterialPropertyType.BumpScaling, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyShininess:
                                properties.Add(new("Shininess", MaterialPropertyType.Shininess, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyReflectivity:
                                properties.Add(new("Reflectivity", MaterialPropertyType.Reflectance, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyShininessStrength:
                                properties.Add(new("ShininessStrength", MaterialPropertyType.ShininessStrength, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyRefracti:
                                properties.Add(new("IOR", MaterialPropertyType.IOR, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyColorDiffuse:
                                properties.Add(new("ColorDiffuse", MaterialPropertyType.ColorDiffuse, MaterialValueType.Float4, default, sizeof(Vector4), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyColorAmbient:
                                properties.Add(new("ColorAmbient", MaterialPropertyType.ColorAmbient, MaterialValueType.Float4, default, sizeof(Vector4), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyColorSpecular:
                                properties.Add(new("ColorSpecular", MaterialPropertyType.ColorSpecular, MaterialValueType.Float4, default, sizeof(Vector4), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyColorEmissive:
                                properties.Add(new("Emissive", MaterialPropertyType.Emissive, MaterialValueType.Float4, default, sizeof(Vector4), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyColorTransparent:
                                properties.Add(new("ColorTransparent", MaterialPropertyType.ColorTransparent, MaterialValueType.Float4, default, sizeof(Vector4), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyColorReflective:
                                properties.Add(new("ColorReflective", MaterialPropertyType.ColorReflective, MaterialValueType.Float4, default, sizeof(Vector4), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyBaseColor:
                                properties.Add(new("BaseColor", MaterialPropertyType.BaseColor, MaterialValueType.Float4, default, sizeof(Vector4), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyMetallicFactor:
                                properties.Add(new("Metallic", MaterialPropertyType.Metallic, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyRoughnessFactor:
                                properties.Add(new("Roughness", MaterialPropertyType.Roughness, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyAnisotropyFactor:
                                properties.Add(new("Anisotropy", MaterialPropertyType.Anisotropy, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.MatkeySpecularFactor:
                                properties.Add(new("Specular", MaterialPropertyType.Specular, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyGlossinessFactor:
                                properties.Add(new("Glossiness", MaterialPropertyType.Glossiness, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.MatkeySheenColorFactor:
                                properties.Add(new("SheenTint", MaterialPropertyType.SheenTint, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.MatkeySheenRoughnessFactor:
                                properties.Add(new("Sheen", MaterialPropertyType.Sheen, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyClearcoatFactor:
                                properties.Add(new("Cleancoat", MaterialPropertyType.Cleancoat, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyClearcoatRoughnessFactor:
                                properties.Add(new("CleancoatGloss", MaterialPropertyType.CleancoatGloss, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyTransmissionFactor:
                                properties.Add(new("Transmission", MaterialPropertyType.Transmission, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyVolumeThicknessFactor:
                                properties.Add(new("VolumeThickness", MaterialPropertyType.VolumeThickness, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyVolumeAttenuationDistance:
                                properties.Add(new("VolumeAttenuationDistance", MaterialPropertyType.VolumeAttenuationDistance, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyVolumeAttenuationColor:
                                properties.Add(new("VolumeAttenuationColor", MaterialPropertyType.VolumeAttenuationColor, MaterialValueType.Float4, default, sizeof(Vector4), buffer.ToArray()));

                                break;

                            case Assimp.MatkeyEmissiveIntensity:
                                properties.Add(new("EmissiveIntensity", MaterialPropertyType.EmissiveIntensity, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.MatkeyUseColorMap:
                                //material.UseColorMap = buffer[0] == 1;
                                break;

                            case Assimp.MatkeyUseMetallicMap:
                                //material.UseMetallicMap = buffer[0] == 1;
                                break;

                            case Assimp.MatkeyUseRoughnessMap:
                                //material.UseRoughnessMap = buffer[0] == 1;
                                break;

                            case Assimp.MatkeyUseEmissiveMap:
                                //material.UseEmissiveMap = buffer[0] == 1;
                                break;

                            case Assimp.MatkeyUseAOMap:
                                //material.UseAOMap = buffer[0] == 1;
                                break;

                            case Assimp.MatkeyTextureBase:
                                var filePath = Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1));

                                if (!texturePathToGuid.TryGetValue(filePath, out var guid))
                                {
                                    guid = Guid.NewGuid();
                                    texturePathToGuid.Add(filePath, guid);
                                }

                                var index = FindOrCreateIdx(textures, guid, (TextureType)semantic);

                                if (!texturePaths.Contains(filePath))
                                {
                                    texturePaths.Add(filePath);
                                }

                                break;

                            case Assimp.MatkeyUvwsrcBase:
                                FindOrCreate(textures, (TextureType)semantic).UVWSrc = MemoryMarshal.Cast<byte, int>(buffer)[0];
                                break;

                            case Assimp.MatkeyTexopBase:
                                FindOrCreate(textures, (TextureType)semantic).Op = Convert((TextureOp)MemoryMarshal.Cast<byte, int>(buffer)[0]);
                                break;

                            case Assimp.MatkeyMappingBase:
                                FindOrCreate(textures, (TextureType)semantic).Mapping = MemoryMarshal.Cast<byte, int>(buffer)[0];
                                break;

                            case Assimp.MatkeyTexblendBase:
                                FindOrCreate(textures, (TextureType)semantic).Blend = Convert((BlendMode)MemoryMarshal.Cast<byte, int>(buffer)[0]);
                                break;

                            case Assimp.MatkeyMappingmodeUBase:
                                FindOrCreate(textures, (TextureType)semantic).U = Convert((TextureMapMode)MemoryMarshal.Cast<byte, int>(buffer)[0]);
                                break;

                            case Assimp.MatkeyMappingmodeVBase:
                                FindOrCreate(textures, (TextureType)semantic).V = Convert((TextureMapMode)MemoryMarshal.Cast<byte, int>(buffer)[0]);
                                break;

                            case Assimp.MatkeyTexmapAxisBase:
                                break;

                            case Assimp.MatkeyUvtransformBase:
                                break;

                            case Assimp.MatkeyTexflagsBase:
                                var flags = FindOrCreate(textures, (TextureType)semantic).Flags = Convert((TextureFlags)MemoryMarshal.Cast<byte, int>(buffer)[0]);
                                if (flags == IO.Binary.Materials.TextureFlags.UseAlpha)
                                {
                                    material.Flags |= MaterialFlags.AlphaTest;
                                }
                                break;

                            case Assimp.MatkeyShaderVertex:
                                shaders.Add(new(MaterialShaderType.VertexShaderFile, Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1))));
                                break;

                            case Assimp.MatkeyShaderTesselation:
                                shaders.Add(new(MaterialShaderType.HullShaderFile, Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1))));
                                break;

                            case Assimp.MatkeyShaderPrimitive:
                                shaders.Add(new(MaterialShaderType.DomainShaderFile, Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1))));
                                break;

                            case Assimp.MatkeyShaderGeo:
                                shaders.Add(new(MaterialShaderType.GeometryShaderFile, Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1))));
                                break;

                            case Assimp.MatkeyShaderFragment:
                                shaders.Add(new(MaterialShaderType.PixelShaderFile, Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1))));
                                break;

                            case Assimp.MatkeyShaderCompute:
                                shaders.Add(new(MaterialShaderType.ComputeShaderFile, Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1))));
                                break;

                            default:
                                continue;
                        }
                    }

                    if (material.Name == string.Empty)
                    {
                        material.Name = i.ToString();
                    }

                    material.Properties = properties;
                    material.Textures = textures;

                    // try
                    {
                        var guid = materialIds[i] = Guid.NewGuid();

                        string targetPath = Path.Combine(outDir, $"{modelName}-{material.Name}.material");
                        context.ImportChild(targetPath, guid, out var path);
                        material.Save(path, Encoding.UTF8);
                        progressContext.AddProgress();
                    }
                    //catch (Exception ex)
                    //{
                    //    Logger.Log(ex);
                    //    MessageBox.Show("Failed to save material file", ex.Message);
                    //    return false;
                    //}
                }
            }
            //catch (Exception ex)
            /*{
                materials = null;
                materialIds = null;
                Logger.Log(ex);
                MessageBox.Show("Failed to load materials", ex.Message);
                return false;
            }*/

            return true;
        }

        private unsafe bool LoadAnimations(string modelName, string outDir, AssimpScene* scene, ImportContext context, ProgressContext progressContext, Dictionary<Pointer<Mesh>, MeshData> pToMesh)
        {
            try
            {
                AnimationFile[] animations = new AnimationFile[scene->MNumAnimations];
                Guid[] animationsIds = new Guid[scene->MNumAnimations];
                for (int i = 0; i < scene->MNumAnimations; i++)
                {
                    var anim = scene->MAnimations[i];
                    AnimationFile animation = animations[i] = new AnimationFile(Guid.NewGuid(), anim->MName, anim->MDuration, anim->MTicksPerSecond, null);
                    for (int j = 0; j < anim->MNumChannels; j++)
                    {
                        var chan = anim->MChannels[j];
                        NodeChannel channel = new(chan->MNodeName)
                        {
                            PreState = (AnimationBehavior)chan->MPreState,
                            PostState = (AnimationBehavior)chan->MPostState
                        };
                        for (int x = 0; x < chan->MNumPositionKeys; x++)
                        {
                            var key = chan->MPositionKeys[x];
                            channel.PositionKeyframes.Add(new() { Time = key.MTime, Value = key.MValue });
                        }
                        for (int x = 0; x < chan->MNumRotationKeys; x++)
                        {
                            var key = chan->MRotationKeys[x];
                            channel.RotationKeyframes.Add(new() { Time = key.MTime, Value = key.MValue });
                        }
                        for (int x = 0; x < chan->MNumScalingKeys; x++)
                        {
                            var key = chan->MScalingKeys[x];
                            channel.ScaleKeyframes.Add(new() { Time = key.MTime, Value = key.MValue });
                        }
                        animation.NodeChannels.Add(channel);
                    }

                    for (int j = 0; j < anim->MNumMeshChannels; j++)
                    {
                        var chan = anim->MMeshChannels[j];
                        MeshChannel channel = new(chan->MName);
                        for (int x = 0; x < chan->MNumKeys; x++)
                        {
                            var key = chan->MKeys[x];
                            channel.Keyframes.Add(new() { Time = key.MTime, MeshName = pToMesh[scene->MMeshes[key.MValue]].Name });
                        }

                        animation.MeshChannels.Add(channel);
                    }

                    for (int j = 0; j < anim->MNumMorphMeshChannels; j++)
                    {
                        var chan = anim->MMorphMeshChannels[j];
                        MorphMeshChannel channel = new(chan->MName);
                        for (int x = 0; x < chan->MNumKeys; x++)
                        {
                            var key = chan->MKeys[x];
                            MeshMorphKeyframe keyframe = new();
                            keyframe.Values = new uint[key.MNumValuesAndWeights];
                            keyframe.Weights = new double[key.MNumValuesAndWeights];
                            for (int y = 0; y < key.MNumValuesAndWeights; y++)
                            {
                                keyframe.Values[y] = key.MValues[y];
                                keyframe.Weights[y] = key.MWeights[y];
                            }

                            channel.Keyframes.Add(keyframe);
                        }

                        animation.MorphMeshChannels.Add(channel);
                    }

                    try
                    {
                        var guid = animationsIds[i] = Guid.NewGuid();

                        string targetPath = Path.Combine(outDir, $"{modelName}-{animation.Name}.animation");
                        context.ImportChild(targetPath, guid, out var path);
                        animation.Save(path, Encoding.UTF8);
                        progressContext.AddProgress();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        MessageBox.Show("Failed to save animation file", ex.Message);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                MessageBox.Show("Failed to load animations", ex.Message);
                return false;
            }
            return true;
        }

        private unsafe bool LoadMeshes(string modelName, AssimpScene* scene, ImportContext context, ProgressContext progressContext, Node root, Dictionary<Pointer<AssimpNode>, Node> pToNode, Guid[]? materialIds, [MaybeNullWhen(false)] out MeshData[] meshes, [MaybeNullWhen(false)] out Dictionary<string, MeshData> nameToMesh, [MaybeNullWhen(false)] out Dictionary<Pointer<Mesh>, MeshData> pToMesh)
        {
            //try
            {
                meshes = new MeshData[scene->MNumMeshes];
                nameToMesh = [];
                pToMesh = [];
                for (int i = 0; i < scene->MNumMeshes; i++)
                {
                    Mesh* msh = scene->MMeshes[i];

                    uint[] indices = new uint[msh->MNumFaces * 3];
                    for (int j = 0; j < msh->MNumFaces; j++)
                    {
                        var face = msh->MFaces[j];
                        for (int k = 0; k < 3; k++)
                        {
                            indices[j * 3 + k] = face.MIndices[k];
                        }
                    }

                    Vector4[]? colors = ToManaged(msh->MColors.Element0, msh->MNumVertices);
                    Vector3[]? positions = ToManaged(msh->MVertices, msh->MNumVertices);
                    Vector3[]? uvs = ToManaged(msh->MTextureCoords[0], msh->MNumVertices);
                    Vector3[]? normals = ToManaged(msh->MNormals, msh->MNumVertices);
                    Vector3[]? tangents = ToManaged(msh->MTangents, msh->MNumVertices);

                    BoundingBox box = default;
                    BoundingSphere sphere = default;
                    if (positions != null)
                    {
                        box = BoundingBoxHelper.Compute(positions);
                        sphere = BoundingSphere.CreateFromBoundingBox(box);
                    }
                    BoneData[]? bones = null;
                    if (msh->MNumBones > 0)
                    {
                        bones = new BoneData[msh->MNumBones];
                        for (int j = 0; j < bones.Length; j++)
                        {
                            Bone* bn = msh->MBones[j];

                            if (bn->MNode == null)
                            {
                                var node = root.FindNode(bn->MName);
                                if (node != null)
                                {
                                    node.Flags |= NodeFlags.Bone;
                                }
                                else
                                {
                                    Logger.Warn($"Failed to find node for bone '{bn->MName}'");
                                }
                            }
                            else
                            {
                                pToNode[bn->MNode].Flags |= NodeFlags.Bone;
                            }

                            IO.Binary.Meshes.VertexWeight[] w = new IO.Binary.Meshes.VertexWeight[bn->MNumWeights];
                            for (int x = 0; x < w.Length; x++)
                            {
                                w[x] = new(bn->MWeights[x].MVertexId, bn->MWeights[x].MWeight);
                            }

                            bones[j] = new BoneData(bn->MName, w, Matrix4x4.Transpose(bn->MOffsetMatrix));
                        }
                    }

                    Guid materialId = Guid.Empty;
                    if (materialIds != null)
                    {
                        materialId = materialIds[(int)msh->MMaterialIndex];
                    }

                    string name = msh->MName;

                    string orgName = name;
                    int xName = 1;
                    while (nameToMesh.ContainsKey(name))
                    {
                        name = $"{orgName}.{xName}";
                        xName++;
                    }

                    VertexFlags flags = VertexFlags.Positions;
                    if (colors != null)
                    {
                        flags |= VertexFlags.Colors;
                    }

                    if (uvs != null)
                    {
                        flags |= VertexFlags.UVs;
                    }

                    if (normals != null)
                    {
                        flags |= VertexFlags.Normals;
                    }

                    if (tangents != null)
                    {
                        flags |= VertexFlags.Tangents;
                    }

                    if (bones != null)
                    {
                        flags |= VertexFlags.Skinned;
                    }

                    MeshData mesh = meshes[i] = new(name, Guid.NewGuid(), materialId, flags, bones);

                    BoneWeight[]? weights = null;
                    if (bones != null)
                    {
                        weights = new BoneWeight[msh->MNumVertices];
                        for (int j = 0; j < msh->MNumVertices; j++)
                        {
                            weights[j] = new(mesh.GatherBoneData(j));
                        }
                    }
#nullable disable
                    MeshLODData data = new(0, msh->MNumVertices, msh->MNumFaces * 3, box, sphere, indices, colors, positions, uvs, normals, tangents, weights);
#nullable restore
                    mesh.LODs.Add(data);

                    Parallel.For(0, 4, j =>
                    {
                        int lod = (int)Math.Pow(2, j);
                        Logger.Info($"Generating LOD: {lod}, {mesh.Name}");
                        try
                        {
                            var lodLower = SimplifyProcess.Simplify(data, j, Logger);
                            lodLower.LODLevel = (uint)lod;
                            lock (mesh.LODs)
                            {
                                mesh.LODs.Add(lodLower);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"Failed to generate LOD Level {lod} for '{mesh.Name}'");
                            Logger.Log(ex);
                        }
                    });

                    mesh.LODs.Sort(MeshLODLevelComparer.Instance);

                    nameToMesh.Add(name, meshes[i]);
                    pToMesh.Add(msh, meshes[i]);
                }

                if (meshes.Length > 0 || root.Children.Count > 0)
                {
                    try
                    {
                        ModelFile modelFile = new(meshes, root);
                        context.EmitArtifact(modelName, AssetType.Model, out string path);
                        modelFile.Save(path, Encoding.UTF8, Endianness.LittleEndian, Compression.LZ4);
                        progressContext.AddProgress();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        MessageBox.Show("Failed to save model file", ex.Message);
                        return false;
                    }
                }
            }
            //catch (Exception ex)
            /*
            {
                meshes = null;
                nameToMesh = null;
                pToMesh = null;
                Logger.Log(ex);
                MessageBox.Show("Failed to load meshes", ex.Message);
                return false;
            }
            */
            return true;
        }

        private unsafe bool LoadSceneGraph(AssimpScene* scene, [MaybeNullWhen(false)] out Node root, [MaybeNullWhen(false)] out Dictionary<string, Node> nameToNode, [MaybeNullWhen(false)] out Dictionary<Pointer<AssimpNode>, Node> pToNode)
        {
            try
            {
                List<Node> nodes = [];
                pToNode = [];
                nameToNode = [];
                root = WalkNode(nameToNode, pToNode, scene->MRootNode, null);
            }
            catch (Exception ex)
            {
                pToNode = null;
                nameToNode = null;
                root = null;
                Logger.Log(ex);
                MessageBox.Show("Failed to load scene graph", ex.Message);
                return false;
            }
            return true;
        }

        private unsafe Node WalkNode(Dictionary<string, Node> nameToNode, Dictionary<Pointer<AssimpNode>, Node> pToNode, AssimpNode* node, Node? parent)
        {
            string name = node->MName;

            Matrix4x4 transform = Matrix4x4.Transpose(node->MTransformation);

            Node sceneNode = new(name, transform, node->MNumMeshes == 0 ? NodeFlags.None : NodeFlags.Drawable, parent, new([]));

            for (int i = 0; i < node->MNumMeshes; i++)
            {
                sceneNode.Meshes.Add(node->MMeshes[i]);
            }

            for (int i = 0; i < node->MNumChildren; i++)
            {
                var child = WalkNode(nameToNode, pToNode, node->MChildren[i], sceneNode);
                sceneNode.Children.Add(child);
            }

            string orgName = name;
            int x = 1;
            while (nameToNode.ContainsKey(name))
            {
                name = $"{orgName}.{x}";
                x++;
            }

            sceneNode.Name = name;

            nameToNode.Add(name, sceneNode);
            pToNode.Add(node, sceneNode);

            return sceneNode;
        }

        private static void SwapImage(ref IScratchImage before, IScratchImage after)
        {
            before.Dispose();
            before = after;
        }

        private static unsafe void Log(byte* message, byte* userdata)
        {
            string msg = Encoding.UTF8.GetString(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(message));
            Logger.Log(msg);
        }

        public static MaterialTextureType Convert(TextureType type)
        {
            return type switch
            {
                TextureType.None => MaterialTextureType.None,
                TextureType.Diffuse => MaterialTextureType.Diffuse,
                TextureType.Specular => MaterialTextureType.Specular,
                TextureType.Ambient => MaterialTextureType.Ambient,
                TextureType.Emissive => MaterialTextureType.Emissive,
                TextureType.Height => MaterialTextureType.Height,
                TextureType.Normals => MaterialTextureType.Normal,
                TextureType.Shininess => MaterialTextureType.Shininess,
                TextureType.Opacity => MaterialTextureType.Opacity,
                TextureType.Displacement => MaterialTextureType.Displacement,
                TextureType.Lightmap => MaterialTextureType.AmbientOcclusionRoughnessMetallic,
                TextureType.Reflection => MaterialTextureType.Reflection,
                TextureType.BaseColor => MaterialTextureType.BaseColor,
                TextureType.NormalCamera => MaterialTextureType.NormalCamera,
                TextureType.EmissionColor => MaterialTextureType.EmissionColor,
                TextureType.Metalness => MaterialTextureType.Metallic,
                TextureType.DiffuseRoughness => MaterialTextureType.Roughness,
                TextureType.AmbientOcclusion => MaterialTextureType.AmbientOcclusion,
                TextureType.Sheen => MaterialTextureType.Sheen,
                TextureType.Clearcoat => MaterialTextureType.Clearcoat,
                TextureType.Transmission => MaterialTextureType.Transmission,
                TextureType.Unknown => MaterialTextureType.RoughnessMetallic,
                _ => throw new NotImplementedException(),
            };
        }

        public static IO.Binary.Materials.BlendMode Convert(BlendMode mode)
        {
            return mode switch
            {
                BlendMode.Default => IO.Binary.Materials.BlendMode.Default,
                BlendMode.Additive => IO.Binary.Materials.BlendMode.Additive,
                _ => throw new NotImplementedException(),
            };
        }

        public static IO.Binary.Materials.TextureOp Convert(TextureOp op)
        {
            return op switch
            {
                TextureOp.Multiply => IO.Binary.Materials.TextureOp.Multiply,
                TextureOp.Add => IO.Binary.Materials.TextureOp.Add,
                TextureOp.Subtract => IO.Binary.Materials.TextureOp.Subtract,
                TextureOp.Divide => IO.Binary.Materials.TextureOp.Divide,
                TextureOp.SmoothAdd => IO.Binary.Materials.TextureOp.SmoothAdd,
                TextureOp.SignedAdd => IO.Binary.Materials.TextureOp.SignedAdd,
                _ => throw new NotImplementedException(),
            };
        }

        public static IO.Binary.Materials.TextureMapMode Convert(TextureMapMode mode)
        {
            return mode switch
            {
                TextureMapMode.Wrap => IO.Binary.Materials.TextureMapMode.Wrap,
                TextureMapMode.Clamp => IO.Binary.Materials.TextureMapMode.Clamp,
                TextureMapMode.Decal => IO.Binary.Materials.TextureMapMode.Decal,
                TextureMapMode.Mirror => IO.Binary.Materials.TextureMapMode.Mirror,
                _ => throw new NotImplementedException(),
            };
        }

        public static IO.Binary.Materials.TextureFlags Convert(TextureFlags flags)
        {
            return flags switch
            {
                0 => IO.Binary.Materials.TextureFlags.None,
                TextureFlags.Invert => IO.Binary.Materials.TextureFlags.Invert,
                TextureFlags.UseAlpha => IO.Binary.Materials.TextureFlags.UseAlpha,
                TextureFlags.IgnoreAlpha => IO.Binary.Materials.TextureFlags.IgnoreAlpha,
                _ => throw new NotImplementedException(),
            };
        }
    }
}