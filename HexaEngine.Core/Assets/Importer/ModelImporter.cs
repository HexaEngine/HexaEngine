namespace HexaEngine.Core.Assets.Importer
{
    using Hexa.NET.Assimp;
    using Hexa.NET.Logging;
    using Hexa.NET.Mathematics;
    using Hexa.NET.Utilities;
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Collections;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Animations;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Core.IO.Binary.Meshes;
    using HexaEngine.Core.IO.Binary.Meshes.Processing;
    using HexaEngine.Core.UI;
    using HexaEngine.Materials;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using System.Text;
    using Node = IO.Binary.Meshes.Node;

    public class ModelImporter : IAssetImporter
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(ModelImporter));
        private ImportContext context = null!;

        public Type? SettingsType { get; } = typeof(ModelImporterSettings);

        public string SettingsKey { get; } = "ModelImportSettings";

        public string? SettingsDisplayName { get; } = "Model Import Settings";

        static unsafe ModelImporter()
        {
#if DEBUG
            Assimp.EnableVerboseLogging(Assimp.AI_TRUE);
#endif
        }

        public unsafe ModelImporter()
        {
            //AiLogStream stream = new(Log);
            //Assimp.AttachLogStream(&stream);
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
                stream = File.OpenRead(artifact.Path);

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
            this.context = context;
            ModelImporterSettings settings = context.GetOrCreateAdditionalMetadata<ModelImporterSettings>(SettingsKey);

            Logger.Info($"Importing model '{Path.GetFileName(context.SourcePath)}'.");

            AiScene* scene = null;
            if (!Load(targetPlatform, context, settings, &scene))
            {
                Logger.Error($"Failed to import {context.SourcePath}");
                if (scene != null)
                {
                    Assimp.ReleaseImport(scene);
                    return;
                }
            }

            if (scene != null)
            {
                Assimp.ReleaseImport(scene);
            }

            Logger.Info($"Imported model '{Path.GetFileName(context.SourcePath)}'.");
        }

        public unsafe bool Load(TargetPlatform targetPlatform, ImportContext context, ModelImporterSettings settings, AiScene** outScene)
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

                if (File.Exists(path))
                {
                    SourceAssetsDatabase.ImportFile(path, new DefaultGuidProvider(context.AssetMetadata.Guid));
                }
            }

            context.SetSteps(7);

            context.BeginStep(1, "Load File");
            AiScene* scene = default;
            try
            {
                scene = Assimp.ImportFile(context.SourcePath, (uint)(AiImporterFlags.SupportBinaryFlavour | AiImporterFlags.SupportCompressedFlavour | AiImporterFlags.SupportTextFlavour));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
            context.EndStep();

            *outScene = scene;

            if (scene == null)
            {
                Logger.Error($"Failed to load {context.SourcePath}");
                MessageBox.Show($"Failed to load scene", $"Failed to load {context.SourcePath}");
                return false;
            }

            context.BeginStep(1, "Post Processing");
            scene = Assimp.ApplyPostProcessing(scene, (uint)settings.PostProcessSteps);
            context.EndStep();

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
                        if (value.MType == AiMetadataType.Float)
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

            context.BeginStep(1, "Importing Graph");
            if (!LoadSceneGraph(scene, out var root, out var nameToNode, out var pToNode))
            {
                return false;
            }
            context.EndStep();

            Guid[]? materialIds = null;
            MaterialFile[]? materials = null;
            List<string>? texturePaths = [];
            Dictionary<string, Guid>? texturePathToGuid = [];

            if (settings.ImportMaterials)
            {
                context.BeginStep((int)scene->MNumMaterials, "Importing Materials");
                if (!LoadMaterials(modelName, importDir, scene, context, out materialIds, out materials, texturePaths, texturePathToGuid))
                {
                    return false;
                }
            }
            context.EndStep();

            context.BeginStep((int)scene->MNumMeshes, "Importing Meshes");
            if (!LoadMeshes(modelName, scene, context, root, pToNode, materialIds, out var meshes, out var nameToMesh, out var pToMesh))
            {
                return false;
            }
            context.EndStep();

            if (CheckForProblems(meshes))
            {
                return false;
            }

            context.BeginStep((int)scene->MNumAnimations, "Importing Animations");
            if (settings.ImportAnimationClips)
            {
                if (!LoadAnimations(modelName, importDir, scene, context, pToMesh))
                {
                    return false;
                }
            }
            context.EndStep();

            if (settings.ImportTextures)
            {
                context.BeginStep(texturePaths.Count, "Importing Textures");
                if (!LoadTextures(targetPlatform, settings, sourceDir, importDir, scene, context, texturePaths, texturePathToGuid))
                {
                    return false;
                }
            }
            context.EndStep();

            return true;
        }

        private unsafe bool LoadTextures(TargetPlatform targetPlatform, ModelImporterSettings settings, string sourceDir, string outDir, AiScene* scene, ImportContext context, List<string> texturePaths, Dictionary<string, Guid> texturePathToGuid)
        {
            Logger.Info("Importing Textures");
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
                    AiTexture* tex = scene->MTextures[index];

                    string fileName = tex->MFilename;

                    if (tex->MHeight != 0)
                    {
                        Logger.Warn($"Failed to import texture {tex->MFilename}, importer doesn't support raw pixel formats.");
                        continue;
                    }

                    var sHint = Encoding.UTF8.GetString(new Span<byte>(&tex->AchFormatHint_0, 3));

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

                    if (string.IsNullOrEmpty(fileName))
                    {
                        fileName = $"{texturePath[1..]}.{sHint}";
                    }

                    string targetPath = Path.Combine(outDir, fileName);
                    context.ImportChild(targetPath, guid, out string path);
                    image.SaveToFile(path, TexFileFormat.Auto, 0);
                    context.AddProgress($"Imported: {targetPath}");
                }
                else
                {
                    string filePath = Path.Combine(sourceDir, texturePath);

                    if (!File.Exists(filePath))
                    {
                        Logger.Warn($"Failed to import texture {filePath}, importer couldn't locate file.");
                        continue;
                    }

                    DictionaryGuidProvider provider = new(context.AssetMetadata.Guid, texturePathToGuid, GuidNotFoundBehavior.Throw);

                    string targetPath = Path.Combine(outDir, texturePath);
                    context.ImportChild(targetPath, provider, out var path);
                    File.Copy(filePath, path, true);
                    context.AddProgress($"Imported: {targetPath}");
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

        private unsafe bool LoadMaterials(string modelName, string outDir, AiScene* scene, ImportContext context, [MaybeNullWhen(false)] out Guid[] materialIds, [MaybeNullWhen(false)] out MaterialFile[] materials, List<string> texturePaths, Dictionary<string, Guid> texturePathToGuid)
        {
            //try
            {
                materials = new MaterialFile[scene->MNumMaterials];
                materialIds = new Guid[scene->MNumMaterials];
                for (int i = 0; i < scene->MNumMaterials; i++)
                {
                    AiMaterial* mat = scene->MMaterials[i];

                    var material = materials[i] = new MaterialFile();
                    material.Guid = Guid.NewGuid();

                    List<MaterialProperty> properties = [];
                    AccessibleList<MaterialTexture> textures = [];
                    List<MaterialShader> shaders = [];

                    for (int j = 0; j < mat->MNumProperties; j++)
                    {
                        AiMaterialProperty* prop = mat->MProperties[j];
                        if (prop == null)
                        {
                            continue;
                        }

                        Span<byte> buffer = new(prop->MData, (int)prop->MDataLength);
                        string key = prop->MKey;
                        int semantic = (int)prop->MSemantic;

                        static ref MaterialTexture FindOrCreate(AccessibleList<MaterialTexture> textures, AiTextureType type)
                        {
                            var t = Convert(type);
                            for (int i = 0; i < textures.Count; i++)
                            {
                                var tex = textures[i];
                                if (tex.Type == t)
                                {
                                    return ref textures.Values[i];
                                }
                            }
                            var index = textures.Count;
                            textures.Add(new MaterialTexture() { Type = t, Filter = TextureMapFilter.Anisotropic, MaxAnisotropy = MaterialTexture.MaxMaxAnisotropy, W = TextureMapMode.Clamp, MinLOD = float.MinValue, MaxLOD = float.MaxValue });
                            return ref textures.Values[index];
                        }

                        static int FindOrCreateIdx(AccessibleList<MaterialTexture> textures, Guid guid, AiTextureType type)
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
                            textures.Add(new MaterialTexture() { Type = t, File = guid, Filter = TextureMapFilter.Anisotropic, MaxAnisotropy = MaterialTexture.MaxMaxAnisotropy, W = TextureMapMode.Clamp, MinLOD = float.MinValue, MaxLOD = float.MaxValue });
                            return index;
                        }

                        switch (key)
                        {
                            case Assimp.AI_MATKEY_NAME:
                                material.Name = Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1));
                                break;

                            case Assimp.AI_MATKEY_TWOSIDED:
                                properties.Add(new("TwoSided", MaterialPropertyType.TwoSided, MaterialValueType.Bool, default, sizeof(bool), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_SHADING_MODEL:
                                properties.Add(new("ShadingMode", MaterialPropertyType.ShadingMode, MaterialValueType.Int32, default, sizeof(int), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_ENABLE_WIREFRAME:
                                properties.Add(new("EnableWireframe", MaterialPropertyType.EnableWireframe, MaterialValueType.Bool, default, sizeof(bool), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_BLEND_FUNC:
                                properties.Add(new("BlendFunc", MaterialPropertyType.BlendFunc, MaterialValueType.Bool, default, sizeof(bool), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_OPACITY:
                                properties.Add(new("Opacity", MaterialPropertyType.Opacity, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_TRANSPARENCYFACTOR:
                                properties.Add(new("Transparency", MaterialPropertyType.Transparency, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_BUMPSCALING:
                                properties.Add(new("BumpScaling", MaterialPropertyType.BumpScaling, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_SHININESS:
                                properties.Add(new("Shininess", MaterialPropertyType.Shininess, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_REFLECTIVITY:
                                properties.Add(new("Reflectivity", MaterialPropertyType.Reflectance, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_SHININESS_STRENGTH:
                                properties.Add(new("ShininessStrength", MaterialPropertyType.ShininessStrength, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_REFRACTI:
                                properties.Add(new("IOR", MaterialPropertyType.IOR, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_COLOR_DIFFUSE:
                                properties.Add(new("ColorDiffuse", MaterialPropertyType.ColorDiffuse, MaterialValueType.Float4, default, sizeof(Vector4), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_COLOR_AMBIENT:
                                properties.Add(new("ColorAmbient", MaterialPropertyType.ColorAmbient, MaterialValueType.Float4, default, sizeof(Vector4), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_COLOR_SPECULAR:
                                properties.Add(new("ColorSpecular", MaterialPropertyType.ColorSpecular, MaterialValueType.Float4, default, sizeof(Vector4), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_COLOR_EMISSIVE:
                                properties.Add(new("Emissive", MaterialPropertyType.Emissive, MaterialValueType.Float4, default, sizeof(Vector4), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_COLOR_TRANSPARENT:
                                properties.Add(new("ColorTransparent", MaterialPropertyType.ColorTransparent, MaterialValueType.Float4, default, sizeof(Vector4), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_COLOR_REFLECTIVE:
                                properties.Add(new("ColorReflective", MaterialPropertyType.ColorReflective, MaterialValueType.Float4, default, sizeof(Vector4), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_BASE_COLOR:
                                properties.Add(new("BaseColor", MaterialPropertyType.BaseColor, MaterialValueType.Float4, default, sizeof(Vector4), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_METALLIC_FACTOR:
                                properties.Add(new("Metallic", MaterialPropertyType.Metallic, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_ROUGHNESS_FACTOR:
                                properties.Add(new("Roughness", MaterialPropertyType.Roughness, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_ANISOTROPY_FACTOR:
                                properties.Add(new("Anisotropy", MaterialPropertyType.Anisotropy, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_SPECULAR_FACTOR:
                                properties.Add(new("Specular", MaterialPropertyType.Specular, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_GLOSSINESS_FACTOR:
                                properties.Add(new("Glossiness", MaterialPropertyType.Glossiness, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_SHEEN_COLOR_FACTOR:
                                properties.Add(new("SheenTint", MaterialPropertyType.SheenTint, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_SHEEN_ROUGHNESS_FACTOR:
                                properties.Add(new("Sheen", MaterialPropertyType.Sheen, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_CLEARCOAT_FACTOR:
                                properties.Add(new("Cleancoat", MaterialPropertyType.Cleancoat, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_CLEARCOAT_ROUGHNESS_FACTOR:
                                properties.Add(new("CleancoatGloss", MaterialPropertyType.CleancoatGloss, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_TRANSMISSION_FACTOR:
                                properties.Add(new("Transmission", MaterialPropertyType.Transmission, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_VOLUME_THICKNESS_FACTOR:
                                properties.Add(new("VolumeThickness", MaterialPropertyType.VolumeThickness, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_VOLUME_ATTENUATION_DISTANCE:
                                properties.Add(new("VolumeAttenuationDistance", MaterialPropertyType.VolumeAttenuationDistance, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_VOLUME_ATTENUATION_COLOR:
                                properties.Add(new("VolumeAttenuationColor", MaterialPropertyType.VolumeAttenuationColor, MaterialValueType.Float4, default, sizeof(Vector4), buffer.ToArray()));

                                break;

                            case Assimp.AI_MATKEY_EMISSIVE_INTENSITY:
                                properties.Add(new("EmissiveIntensity", MaterialPropertyType.EmissiveIntensity, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
                                break;

                            case Assimp.AI_MATKEY_USE_COLOR_MAP:
                                //material.UseColorMap = buffer[0] == 1;
                                break;

                            case Assimp.AI_MATKEY_USE_METALLIC_MAP:
                                //material.UseMetallicMap = buffer[0] == 1;
                                break;

                            case Assimp.AI_MATKEY_USE_ROUGHNESS_MAP:
                                //material.UseRoughnessMap = buffer[0] == 1;
                                break;

                            case Assimp.AI_MATKEY_USE_EMISSIVE_MAP:
                                //material.UseEmissiveMap = buffer[0] == 1;
                                break;

                            case Assimp.AI_MATKEY_USE_AO_MAP:
                                //material.UseAOMap = buffer[0] == 1;
                                break;

                            case Assimp._AI_MATKEY_TEXTURE_BASE:
                                var filePath = Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1));

                                if (filePath.StartsWith("./") || filePath.StartsWith(".\\"))
                                {
                                    filePath = filePath[2..];
                                }

                                if (!texturePathToGuid.TryGetValue(filePath, out var guid))
                                {
                                    guid = Guid.NewGuid();
                                    texturePathToGuid.Add(filePath, guid);
                                }

                                var index = FindOrCreateIdx(textures, guid, (AiTextureType)semantic);

                                if (!texturePaths.Contains(filePath))
                                {
                                    texturePaths.Add(filePath);
                                }

                                break;

                            case Assimp._AI_MATKEY_UVWSRC_BASE:
                                FindOrCreate(textures, (AiTextureType)semantic).UVWSrc = MemoryMarshal.Cast<byte, int>(buffer)[0];
                                break;

                            case Assimp._AI_MATKEY_TEXOP_BASE:
                                //FindOrCreate(textures, (TextureType)semantic).Op = Convert((TextureOp)MemoryMarshal.Cast<byte, int>(buffer)[0]);
                                break;

                            case Assimp._AI_MATKEY_MAPPING_BASE:
                                //FindOrCreate(textures, (TextureType)semantic).Mapping = MemoryMarshal.Cast<byte, int>(buffer)[0];
                                break;

                            case Assimp._AI_MATKEY_TEXBLEND_BASE:
                                //FindOrCreate(textures, (TextureType)semantic).Blend = Convert((BlendMode)MemoryMarshal.Cast<byte, int>(buffer)[0]);
                                break;

                            case Assimp._AI_MATKEY_MAPPINGMODE_U_BASE:
                                FindOrCreate(textures, (AiTextureType)semantic).U = Convert((AiTextureMapMode)MemoryMarshal.Cast<byte, int>(buffer)[0]);
                                break;

                            case Assimp._AI_MATKEY_MAPPINGMODE_V_BASE:
                                FindOrCreate(textures, (AiTextureType)semantic).V = Convert((AiTextureMapMode)MemoryMarshal.Cast<byte, int>(buffer)[0]);
                                break;

                            case Assimp._AI_MATKEY_TEXMAP_AXIS_BASE:
                                break;

                            case Assimp._AI_MATKEY_UVTRANSFORM_BASE:
                                break;

                            case Assimp._AI_MATKEY_TEXFLAGS_BASE:
                                var flags = FindOrCreate(textures, (AiTextureType)semantic).Flags = Convert((AiTextureFlags)MemoryMarshal.Cast<byte, int>(buffer)[0]);
                                if (flags == TextureFlags.UseAlpha)
                                {
                                    material.Flags |= MaterialFlags.AlphaTest;
                                }
                                break;

                            case Assimp.AI_MATKEY_SHADER_VERTEX:
                                shaders.Add(new(MaterialShaderType.VertexShaderFile, Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1))));
                                break;

                            case Assimp.AI_MATKEY_SHADER_TESSELATION:
                                shaders.Add(new(MaterialShaderType.HullShaderFile, Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1))));
                                break;

                            case Assimp.AI_MATKEY_SHADER_PRIMITIVE:
                                shaders.Add(new(MaterialShaderType.DomainShaderFile, Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1))));
                                break;

                            case Assimp.AI_MATKEY_SHADER_GEO:
                                shaders.Add(new(MaterialShaderType.GeometryShaderFile, Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1))));
                                break;

                            case Assimp.AI_MATKEY_SHADER_FRAGMENT:
                                shaders.Add(new(MaterialShaderType.PixelShaderFile, Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1))));
                                break;

                            case Assimp.AI_MATKEY_SHADER_COMPUTE:
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

                    for (int j = 0; j < i; j++)
                    {
                        if (j != i && materials[j]?.Name == material.Name)
                        {
                            material.Name += i.ToString();
                        }
                    }

                    material.Properties = properties;
                    material.Textures = textures.ToList();

                    MaterialNodeConverter.Convert(material, Logger);

                    // try
                    {
                        var guid = materialIds[i] = Guid.NewGuid();

                        string targetPath = Path.Combine(outDir, $"{modelName}-{material.Name}.material");
                        context.ImportChild(targetPath, guid, out var path);
                        material.Save(path, Encoding.UTF8);
                        context.AddProgress($"Imported: {targetPath}");
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

        private unsafe bool LoadAnimations(string modelName, string outDir, AiScene* scene, ImportContext context, Dictionary<Pointer<AiMesh>, MeshData> pToMesh)
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
                        context.AddProgress($"Imported: {targetPath}");
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

        private unsafe bool LoadMeshes(string modelName, AiScene* scene, ImportContext context, Node root, Dictionary<Pointer<AiNode>, Node> pToNode, Guid[]? materialIds, [MaybeNullWhen(false)] out MeshData[] meshes, [MaybeNullWhen(false)] out Dictionary<string, MeshData> nameToMesh, [MaybeNullWhen(false)] out Dictionary<Pointer<AiMesh>, MeshData> pToMesh)
        {
            //try
            {
                meshes = new MeshData[scene->MNumMeshes];
                nameToMesh = [];
                pToMesh = [];
                for (int i = 0; i < scene->MNumMeshes; i++)
                {
                    AiMesh* msh = scene->MMeshes[i];

                    uint[] indices = new uint[msh->MNumFaces * 3];
                    fixed (uint* pFixedIndices = indices)
                    {
                        var pIndices = pFixedIndices;
                        for (int j = 0; j < msh->MNumFaces; j++)
                        {
                            AiFace* face = &msh->MFaces[j];  // Pointer to the current face
                            *pIndices++ = face->MIndices[0];
                            *pIndices++ = face->MIndices[1];
                            *pIndices++ = face->MIndices[2];
                        }
                    }

                    Vector4[]? colors = ToManaged(msh->MColors_0, msh->MNumVertices);
                    Vector3[]? positions = ToManaged(msh->MVertices, msh->MNumVertices);

                    UVChannelInfo channelInfo;
                    UVChannel[] channels = new UVChannel[UVChannelInfo.MaxChannels];
                    UVType* pType = (UVType*)&channelInfo;

                    for (int j = 0; j < UVChannelInfo.MaxChannels; j++)
                    {
                        Vector3* pUVs = msh->MTextureCoords[j];
                        if (pUVs == null)
                        {
                            pType[j] = UVType.Empty;
                            continue;
                        }

                        var componentCount = *(&msh->MNumUVComponents_0 + j);

                        UVChannel channel = default;
                        switch (componentCount)
                        {
                            case 2:
                                {
                                    var type = pType[j] = UVType.UV2D;
                                    channel = new(type, msh->MNumVertices);
                                    Vector2[] channelData = channel.GetUV2D();
                                    for (int k = 0; k < msh->MNumVertices; k++)
                                    {
                                        Vector3 uvw = pUVs[k];
                                        channelData[k] = new(uvw.X, uvw.Y);
                                    }
                                }
                                break;

                            case 3:
                                {
                                    var type = pType[j] = UVType.UV3D;
                                    channel = new(type, msh->MNumVertices);
                                    Vector3[] channelData = channel.GetUV3D();
                                    for (int k = 0; k < msh->MNumVertices; k++)
                                    {
                                        channelData[k] = pUVs[k];
                                    }
                                }
                                break;

                            case 4: // assimp doesnt support 4D UVs
                            default:
                                pType[j] = UVType.Empty; // do not attempt to copy if invalid
                                break;
                        }

                        channels[j] = channel;
                    }

                    Vector3[]? uvs = ToManaged(msh->MTextureCoords_0, msh->MNumVertices);
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
                            AiBone* bn = msh->MBones[j];

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

                            VertexWeight[] w = new VertexWeight[bn->MNumWeights];
                            for (int x = 0; x < w.Length; x++)
                            {
                                w[x] = new(bn->MWeights[x].MVertexId, (float)bn->MWeights[x].MWeight);
                            }

                            bones[j] = new BoneData(bn->MName, w, Matrix4x4.Transpose(bn->MOffsetMatrix));
                        }
                    }

                    if (msh->MNumAnimMeshes > 0)
                    {
                        for (int j = 0; j < msh->MNumAnimMeshes; j++)
                        {
                            AiAnimMesh* animMesh = msh->MAnimMeshes[j];
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

                    MeshData mesh = meshes[i] = new(name, Guid.NewGuid(), materialId, flags, channelInfo, bones);

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
                    MeshLODData data = new(0, msh->MNumVertices, msh->MNumFaces * 3, box, sphere, indices, colors, positions, channels, normals, tangents, weights);
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
                        modelFile.Save(path, Encoding.UTF8, Endianness.LittleEndian, Compression.None);
                        context.AddProgress($"Imported: {modelName}");
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

        private unsafe bool LoadSceneGraph(AiScene* scene, [MaybeNullWhen(false)] out Node root, [MaybeNullWhen(false)] out Dictionary<string, Node> nameToNode, [MaybeNullWhen(false)] out Dictionary<Pointer<AiNode>, Node> pToNode)
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

        private unsafe Node WalkNode(Dictionary<string, Node> nameToNode, Dictionary<Pointer<AiNode>, Node> pToNode, AiNode* node, Node? parent)
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

        private unsafe void Log(byte* message, byte* userdata)
        {
            string msg = Encoding.UTF8.GetString(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(message));
            Logger.Log(msg);

            context.LogMessage(msg);
        }

        public static MaterialTextureType Convert(AiTextureType type)
        {
            return type switch
            {
                AiTextureType.None => MaterialTextureType.None,
                AiTextureType.Diffuse => MaterialTextureType.Diffuse,
                AiTextureType.Specular => MaterialTextureType.Specular,
                AiTextureType.Ambient => MaterialTextureType.Ambient,
                AiTextureType.Emissive => MaterialTextureType.Emissive,
                AiTextureType.Height => MaterialTextureType.Height,
                AiTextureType.Normals => MaterialTextureType.Normal,
                AiTextureType.Shininess => MaterialTextureType.Shininess,
                AiTextureType.Opacity => MaterialTextureType.Opacity,
                AiTextureType.Displacement => MaterialTextureType.Displacement,
                AiTextureType.Lightmap => MaterialTextureType.AmbientOcclusionRoughnessMetallic,
                AiTextureType.Reflection => MaterialTextureType.Reflection,
                AiTextureType.BaseColor => MaterialTextureType.BaseColor,
                AiTextureType.NormalCamera => MaterialTextureType.NormalCamera,
                AiTextureType.EmissionColor => MaterialTextureType.EmissionColor,
                AiTextureType.Metalness => MaterialTextureType.Metallic,
                AiTextureType.DiffuseRoughness => MaterialTextureType.Roughness,
                AiTextureType.AmbientOcclusion => MaterialTextureType.AmbientOcclusion,
                AiTextureType.Sheen => MaterialTextureType.Sheen,
                AiTextureType.Clearcoat => MaterialTextureType.Clearcoat,
                AiTextureType.Transmission => MaterialTextureType.Transmission,
                AiTextureType.Unknown => MaterialTextureType.RoughnessMetallic,
                _ => throw new NotSupportedException($"{nameof(AiTextureType)} '{type}' not supported."),
            };
        }

        public static BlendMode Convert(AiBlendMode mode)
        {
            return mode switch
            {
                AiBlendMode.Default => BlendMode.Default,
                AiBlendMode.Additive => BlendMode.Additive,
                _ => throw new NotSupportedException($"{nameof(BlendMode)} '{mode}' not supported."),
            };
        }

        public static TextureOp Convert(AiTextureOp op)
        {
            return op switch
            {
                AiTextureOp.Multiply => TextureOp.Multiply,
                AiTextureOp.Add => TextureOp.Add,
                AiTextureOp.Subtract => TextureOp.Subtract,
                AiTextureOp.Divide => TextureOp.Divide,
                AiTextureOp.SmoothAdd => TextureOp.SmoothAdd,
                AiTextureOp.SignedAdd => TextureOp.SignedAdd,
                _ => throw new NotSupportedException($"{nameof(TextureOp)} '{op}' not supported."),
            };
        }

        public static TextureMapMode Convert(AiTextureMapMode mode)
        {
            return mode switch
            {
                AiTextureMapMode.Wrap => TextureMapMode.Wrap,
                AiTextureMapMode.Clamp => TextureMapMode.Clamp,
                AiTextureMapMode.Mirror => TextureMapMode.Mirror,
                _ => throw new NotSupportedException($"{nameof(TextureMapMode)} '{mode}' not supported."),
            };
        }

        public static TextureFlags Convert(AiTextureFlags flags)
        {
            TextureFlags result = 0;
            if ((flags & AiTextureFlags.Invert) != 0)
            {
                result |= TextureFlags.Invert;
            }
            if ((flags & AiTextureFlags.UseAlpha) != 0)
            {
                result |= TextureFlags.UseAlpha;
            }
            if ((flags & AiTextureFlags.IgnoreAlpha) != 0)
            {
                result |= TextureFlags.IgnoreAlpha;
            }
            return result;
        }
    }
}