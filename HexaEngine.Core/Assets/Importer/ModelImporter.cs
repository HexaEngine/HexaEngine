using HexaEngine.Core.IO.Binary.Animations;
using HexaEngine.Core.IO.Binary.Materials;
using HexaEngine.Core.IO.Binary.Meshes;

namespace HexaEngine.Core.Assets.Importer
{
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Extensions;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;
    using HexaEngine.Core.IO.Binary.Animations;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Core.IO.Binary.Meshes;
    using HexaEngine.Core.UI;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;
    using Silk.NET.Assimp;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using System.Text;
    using AnimationData = AnimationClip;
    using AssimpMaterialProperty = Silk.NET.Assimp.MaterialProperty;
    using AssimpNode = Silk.NET.Assimp.Node;
    using AssimpScene = Silk.NET.Assimp.Scene;
    using BlendMode = Silk.NET.Assimp.BlendMode;
    using MaterialProperty = MaterialProperty;
    using Node = Node;
    using TextureFlags = Silk.NET.Assimp.TextureFlags;
    using TextureMapMode = Silk.NET.Assimp.TextureMapMode;
    using TextureOp = Silk.NET.Assimp.TextureOp;
    using TextureType = Silk.NET.Assimp.TextureType;

    public class ModelImporterSettings
    {
        public PostProcessSteps PostProcessSteps = PostProcessSteps.CalculateTangentSpace | PostProcessSteps.MakeLeftHanded | PostProcessSteps.FindInvalidData | PostProcessSteps.FindDegenerates | PostProcessSteps.FindInstances | PostProcessSteps.ImproveCacheLocality;

        public TexPostProcessSteps TexPostProcessSteps = TexPostProcessSteps.None;

        public TexFileFormat TexFileFormat;

        public Format TexFormat;

        public int MaxWidth;

        public int MaxHeight;

        public bool ImportMaterials;

        public bool ImportTextures;

        public TexCompressFlags TexCompressFlags = TexCompressFlags.Parallel;
    }

    public class ModelImporter : IAssetImporter
    {
        private static readonly Assimp assimp = Assimp.GetApi();

        static unsafe ModelImporter()
        {
            LogStream stream = new(new(Log));
            assimp.AttachLogStream(&stream);
#if DEBUG
            assimp.EnableVerboseLogging(Assimp.True);
#endif
        }

        public bool CanImport(string fileExtension)
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

        public unsafe void Import(TargetPlatform targetPlatform, ImportContext context)
        {
            ModelImporterSettings settings = context.GetOrCreateAdditionalMetadata<ModelImporterSettings>("ModelImportSettings");

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
        }

        public unsafe bool Load(TargetPlatform targetPlatform, ImportContext context, ModelImporterSettings settings, AssimpScene** outScene)
        {
            var modelName = Path.GetFileNameWithoutExtension(context.SourcePath);
            var sourceDir = Path.GetDirectoryName(context.SourcePath);

            var scene = assimp.ImportFile(context.SourcePath, (uint)(ImporterFlags.SupportBinaryFlavour | ImporterFlags.SupportCompressedFlavour));
            *outScene = scene;

            if (scene == null)
            {
                Logger.Error($"Failed to load {context.SourcePath}");
                MessageBox.Show($"Failed to load scene", $"Failed to load {context.SourcePath}");
                return false;
            }

            scene = assimp.ApplyPostProcessing(scene, (uint)settings.PostProcessSteps);

            if (scene == null)
            {
                Logger.Error($"Failed to apply post processing {context.SourcePath}");
                MessageBox.Show($"Failed to apply post processing", $"Failed to apply post processing {context.SourcePath}");
                return false;
            }

            if (!LoadSceneGraph(scene, out var root, out var nameToNode))
            {
                return false;
            }

            Guid[]? materialIds = null;
            MaterialFile[]? materials = null;

            if (settings.ImportMaterials)
            {
                if (!LoadMaterials(modelName, scene, context, out materialIds, out materials, out var texturePaths))
                {
                    return false;
                }
            }

            if (!LoadMeshes(modelName, scene, context, root, nameToNode, materialIds, out var meshes, out var nameToMesh, out var pToMesh))
            {
                return false;
            }

            if (CheckForProblems(meshes))
            {
                return false;
            }

            if (!LoadAnimations(modelName, scene, context, pToMesh))
            {
                return false;
            }

            if (settings.ImportTextures)
            {
                /*if (!LoadTextures(targetPlatform, settings, sourceDir, scene, context, texturePaths))
                {
                    return false;
                }*/
            }

            return true;
        }

        private unsafe bool LoadTextures(TargetPlatform targetPlatform, ModelImporterSettings settings, string sourceDir, AssimpScene* scene, string outputPath, List<string> texturePaths)
        {
            for (int i = 0; i < scene->MNumTextures; i++)
            {
                Texture* tex = scene->MTextures[i];

                /*
                var outputFile = Path.Combine(outputPath, tex->MFilename);
                if (tex->MHeight != 0)
                {
                    Logger.Warn($"Failed to import texture {tex->MFilename}, importer doesn't support raw pixel formats.");
                    continue;
                }
                */

                Logger.Warn($"Failed to import texture {tex->MFilename}, importer doesn't support embedded textures.");

                /*
                var fs = System.IO.File.Create(outputFile);

                var sHint = Encoding.UTF8.GetString(new Span<byte>(tex->AchFormatHint, 4));

                var texImporter = ImporterRegistry.GetImporter<TextureImporter>();

                texImporter.Import(TargetPlatform.Windows, outputPath, outputFile, null);
                */
            }

            var device = Application.GraphicsDevice;
            var loader = device.TextureLoader;

            /*
            for (int i = 0; i < texturePaths.Count; i++)
            {
                var srcFile = Path.Combine(sourceDir, texturePaths[i]);
                if (!System.IO.File.Exists(srcFile))
                    continue;

                if (settings.TexPostProcessSteps == TexPostProcessSteps.None)
                {
                    try
                    {
                        var destFile = Path.Combine(outputPath, texturePaths[i]);
                        Directory.CreateDirectory(Path.GetDirectoryName(destFile));
                        System.IO.File.Copy(srcFile, destFile, true);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        MessageBox.Show("Failed to copy file", ex.Message);
                        return false;
                    }
                }
                else
                {
                    IScratchImage image;

                    try
                    {
                        image = loader.LoadFormFile(srcFile);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        MessageBox.Show("Failed to load texture", ex.Message);
                        return false;
                    }

                    if ((settings.TexPostProcessSteps & TexPostProcessSteps.Scale) != 0)
                    {
                        try
                        {
                            var newWidth = Math.Min(image.Metadata.Width, settings.MaxWidth);
                            var newHeight = Math.Min(image.Metadata.Height, settings.MaxHeight);

                            SwapImage(ref image, image.Resize(newWidth, newHeight, TexFilterFlags.Default));
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(ex);
                            MessageBox.Show("Failed to scale texture", ex.Message);
                            return false;
                        }
                    }

                    if ((settings.TexPostProcessSteps & TexPostProcessSteps.GenerateMips) != 0)
                    {
                        try
                        {
                            SwapImage(ref image, image.GenerateMipMaps(TexFilterFlags.Default));
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(ex);
                            MessageBox.Show("Failed to generate mips texture", ex.Message);
                            return false;
                        }
                    }

                    if ((settings.TexPostProcessSteps & TexPostProcessSteps.Convert) != 0)
                    {
                        try
                        {
                            if (FormatHelper.IsCompressed(settings.TexFormat))
                            {
                                SwapImage(ref image, image.Compress(device, settings.TexFormat, settings.TexCompressFlags));
                            }
                            else
                            {
                                SwapImage(ref image, image.Convert(settings.TexFormat, TexFilterFlags.Default));
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(ex);
                            MessageBox.Show("Failed to convert texture", ex.Message);
                            return false;
                        }
                    }

                    try
                    {
                        var newName = Path.GetFileNameWithoutExtension(texturePaths[i]) + $".{settings.TexFileFormat.ToString().ToLowerInvariant()}";
                        //ChangeNameOfTexture(texturePaths[i], newName);
                        var destFile = Path.Combine(outputPath, newName);
                        Directory.CreateDirectory(Path.GetDirectoryName(destFile));

                        switch (targetPlatform)
                        {
                            case TargetPlatform.Windows:
                                image.SaveToFile(destFile, TexFileFormat.DDS, 0);
                                break;

                            case TargetPlatform.Linux:
                            case TargetPlatform.Android:
                            case TargetPlatform.Other:
                                image.SaveToFile(destFile, TexFileFormat.TGA, 0);
                                break;
                        }

                        image.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        MessageBox.Show("Failed to save texture", ex.Message);
                        return false;
                    }
                }
            }
            */

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

        private unsafe bool LoadMaterials(string modelName, AssimpScene* scene, ImportContext context, [MaybeNullWhen(false)] out Guid[] materialIds, [MaybeNullWhen(false)] out MaterialFile[] materials, [MaybeNullWhen(false)] out List<string> texturePaths)
        {
            try
            {
                texturePaths = [];
                Dictionary<(MaterialFile, int), string> texturePathToGuid = new();
                materials = new MaterialFile[scene->MNumMaterials];
                materialIds = new Guid[scene->MNumMaterials];
                for (int i = 0; i < scene->MNumMaterials; i++)
                {
                    Material* mat = scene->MMaterials[i];

                    var material = materials[i] = new MaterialFile();

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

                        static int FindOrCreateIdx(List<MaterialTexture> textures, TextureType type)
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
                            textures.Add(new MaterialTexture() { Type = t });
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
                                properties.Add(new("Reflectivity", MaterialPropertyType.Reflectivity, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
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
                                texturePathToGuid.Add((material, FindOrCreateIdx(textures, (TextureType)semantic)), filePath);
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
                                FindOrCreate(textures, (TextureType)semantic).Flags = Convert((TextureFlags)MemoryMarshal.Cast<byte, int>(buffer)[0]);
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
                        }
                    }

                    if (material.Name == string.Empty)
                    {
                        material.Name = i.ToString();
                    }

                    material.Properties = properties;
                    material.Textures = textures;

                    try
                    {
                        context.EmitArtifact(material.Name, AssetType.Material, out string path);
                        material.Save(path, Encoding.UTF8);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        MessageBox.Show("Failed to save material file", ex.Message);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                materials = null;
                materialIds = null;
                texturePaths = null;
                Logger.Log(ex);
                MessageBox.Show("Failed to load materials", ex.Message);
                return false;
            }

            return true;
        }

        private unsafe bool LoadAnimations(string modelName, AssimpScene* scene, ImportContext context, Dictionary<Pointer<Mesh>, MeshData> pToMesh)
        {
            try
            {
                AnimationData[] animations = new AnimationData[scene->MNumAnimations];
                for (int i = 0; i < scene->MNumAnimations; i++)
                {
                    var anim = scene->MAnimations[i];
                    AnimationData animation = new(anim->MName, anim->MDuration, anim->MTicksPerSecond);
                    for (int j = 0; j < anim->MNumChannels; j++)
                    {
                        var chan = anim->MChannels[j];
                        NodeChannel channel = new(chan->MNodeName);
                        channel.PreState = (AnimationBehavior)chan->MPreState;
                        channel.PostState = (AnimationBehavior)chan->MPostState;
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

                    animations[i] = animation;
                }

                if (animations.Length > 0)
                {
                    try
                    {
                        //AnimationLibrary animationLibrary = new(animations);
                        //context.EmitArtifact(AssetType.Animation, out string path);
                        //animationLibrary.Save(path, Encoding.UTF8);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        MessageBox.Show("Failed to save animation library", ex.Message);
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

        private unsafe bool LoadMeshes(string modelName, AssimpScene* scene, ImportContext context, Node root, Dictionary<string, Node> nameToNode, Guid[]? materialIds, [MaybeNullWhen(false)] out MeshData[] meshes, [MaybeNullWhen(false)] out Dictionary<string, MeshData> nameToMesh, [MaybeNullWhen(false)] out Dictionary<Pointer<Mesh>, MeshData> pToMesh)
        {
            try
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

                    BoneData[] bones = new BoneData[msh->MNumBones];
                    if (msh->MNumBones > 0)
                    {
                        for (int j = 0; j < bones.Length; j++)
                        {
                            var bn = msh->MBones[j];
                            nameToNode[bn->MName].Flags |= NodeFlags.Bone;

                            IO.Binary.Meshes.VertexWeight[] weights = new IO.Binary.Meshes.VertexWeight[bn->MNumWeights];
                            for (int x = 0; x < weights.Length; x++)
                            {
                                weights[x] = new(bn->MWeights[x].MVertexId, bn->MWeights[x].MWeight);
                            }

                            bones[j] = new BoneData(bn->MName, weights, Matrix4x4.Transpose(bn->MOffsetMatrix));
                        }
                    }

                    Guid materialId = Guid.Empty;
                    if (materialIds != null)
                    {
                        materialId = materialIds[(int)msh->MMaterialIndex];
                    }

                    if (bones.Length > 0)
                    {
                        meshes[i] = new MeshData(msh->MName, Guid.NewGuid(), materialId, box, sphere, msh->MNumVertices, (uint)indices.Length, (uint)bones.Length, indices, colors, positions, uvs, normals, tangents, [.. bones]);
                    }
                    else
                    {
                        meshes[i] = new MeshData(msh->MName, Guid.NewGuid(), materialId, box, sphere, msh->MNumVertices, (uint)indices.Length, 0u, indices, colors, positions, uvs, normals, tangents, null);
                    }

                    nameToMesh.Add(msh->MName, meshes[i]);
                    pToMesh.Add(msh, meshes[i]);
                }

                if (meshes.Length > 0 || root.Children.Count > 0)
                {
                    try
                    {
                        ModelFile modelFile = new(string.Empty, meshes, root);
                        context.EmitArtifact(modelName, AssetType.Model, out string path);
                        modelFile.Save(path, Encoding.UTF8);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        MessageBox.Show("Failed to save model file", ex.Message);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                meshes = null;
                nameToMesh = null;
                pToMesh = null;
                Logger.Log(ex);
                MessageBox.Show("Failed to load meshes", ex.Message);
                return false;
            }
            return true;
        }

        private unsafe bool LoadSceneGraph(AssimpScene* scene, [MaybeNullWhen(false)] out Node root, [MaybeNullWhen(false)] out Dictionary<string, Node> nameToNode)
        {
            try
            {
                List<Node> nodes = [];
                nameToNode = [];
                root = WalkNode(nameToNode, scene->MRootNode, null);
            }
            catch (Exception ex)
            {
                nameToNode = null;
                root = null;
                Logger.Log(ex);
                MessageBox.Show("Failed to load scene graph", ex.Message);
                return false;
            }
            return true;
        }

        private unsafe Node WalkNode(Dictionary<string, Node> nameToNode, AssimpNode* node, Node parent)
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
                var child = WalkNode(nameToNode, node->MChildren[i], sceneNode);
                sceneNode.Children.Add(child);
            }

            nameToNode.Add(name, sceneNode);

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