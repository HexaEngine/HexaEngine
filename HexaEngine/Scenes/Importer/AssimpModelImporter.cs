namespace HexaEngine.Scenes.Importer
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Extensions;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;
    using HexaEngine.Core.IO.Animations;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.UI;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;
    using HexaEngine.Projects;
    using Silk.NET.Assimp;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using System.Text;
    using AnimationData = Core.IO.Animations.Animation;
    using AssimpMaterialProperty = Silk.NET.Assimp.MaterialProperty;
    using AssimpNode = Silk.NET.Assimp.Node;
    using AssimpScene = Silk.NET.Assimp.Scene;
    using BlendMode = Silk.NET.Assimp.BlendMode;
    using MaterialProperty = Core.IO.Materials.MaterialProperty;
    using Node = Core.IO.Meshes.Node;
    using TextureFlags = Silk.NET.Assimp.TextureFlags;
    using TextureMapMode = Silk.NET.Assimp.TextureMapMode;
    using TextureOp = Silk.NET.Assimp.TextureOp;
    using TextureType = Silk.NET.Assimp.TextureType;

    public class AssimpModelImporter
    {
        private static readonly Assimp assimp = Assimp.GetApi();

        private readonly Dictionary<Pointer<AssimpNode>, Node> nodesT = new();
        private readonly Dictionary<Node, Pointer<AssimpNode>> nodesP = new();
        private readonly Dictionary<string, Node> nodesN = new();
        private readonly Dictionary<Pointer<Mesh>, MeshData> meshesT = new();

        private readonly Dictionary<string, AnimationData> animationsT = new();

        private string name;
        private string dir;
        private List<Node> nodes;
        private List<string> textures;
        private MeshData[] meshes;
        private MaterialData[] materials;
        private AnimationData[] animations;
        private Node root;
        private AnimationLibrary animationLibrary;
        private MaterialLibrary materialLibrary;
        private ModelFile modelFile;
        private unsafe AssimpScene* scene;

        public IReadOnlyList<MeshData> Meshes => meshes;

        public IReadOnlyList<MaterialData> Materials => materials;

        public IReadOnlyList<string> Textures => textures;

        public PostProcessSteps PostProcessSteps = PostProcessSteps.CalculateTangentSpace | PostProcessSteps.MakeLeftHanded | PostProcessSteps.FindInvalidData;

        public TexPostProcessSteps TexPostProcessSteps = TexPostProcessSteps.None;

        public TexFileFormat TexFileFormat;

        public Format TexFormat;

        public float TexScaleFactor = 1;

        public TexCompressFlags TexCompressFlags = TexCompressFlags.Parallel;

        public Task<bool> LoadAsync(string path)
        {
            return Task.Run(() => Load(path));
        }

        public Task<bool> ImportAsync(IGraphicsDevice device)
        {
            return Task.Run(() => Import(device));
        }

        public unsafe bool Load(string path)
        {
            name = Path.GetFileNameWithoutExtension(path);
            dir = Path.GetDirectoryName(path);

            LogStream stream = new(new(Log));
            assimp.AttachLogStream(&stream);
            assimp.EnableVerboseLogging(Assimp.True);

            scene = assimp.ImportFile(path, (uint)(ImporterFlags.SupportBinaryFlavour | ImporterFlags.SupportCompressedFlavour));

            if (scene == null)
            {
                Logger.Error($"Failed to load {path}");
                MessageBox.Show($"Failed to load scene", $"Failed to load {path}");
                return false;
            }

            scene = assimp.ApplyPostProcessing(scene, (uint)PostProcessSteps);

            if (scene == null)
            {
                Logger.Error($"Failed to apply post processing {path}");
                MessageBox.Show($"Failed to apply post processing", $"Failed to apply post processing {path}");
                return false;
            }

            if (!LoadSceneGraph(scene))
            {
                return false;
            }

            if (!LoadMaterials(scene))
            {
                return false;
            }

            if (!LoadMeshes(scene))
            {
                return false;
            }

            if (!LoadAnimations(scene))
            {
                return false;
            }

            return true;
        }

        public bool CheckForProblems()
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

        public bool ChangeNameOfMaterial(MaterialData material, string newName)
        {
            if (materials.Any(x => x.Name == newName))
            {
                return false;
            }

            string oldName = material.Name;

            ModelFile source = modelFile;
            for (int j = 0; j < source.Meshes.Count; j++)
            {
                MeshData data = source.GetMesh(j);
                if (data.MaterialName == oldName)
                {
                    source.GetMesh(j).MaterialName = newName;
                }
            }

            material.Name = newName;
            return true;
        }

        public bool ChangeNameOfMaterialLibrary(MaterialLibrary library, string newName)
        {
            return false;
        }

        public bool ChangeNameOfTexture(string oldName, string newName)
        {
            if (textures.Contains(newName))
            {
                return false;
            }

            for (int i = 0; i < materials.Length; i++)
            {
                var mat = materials[i];
                for (int j = 0; j < mat.Textures.Count; j++)
                {
                    if (mat.Textures[j].File == oldName)
                    {
                        mat.Textures.MutateItem(j, x => { x.File = newName; return x; });
                    }
                }
            }

            var index = textures.IndexOf(oldName);
            textures[index] = newName;
            return true;
        }

        public unsafe bool Import(IGraphicsDevice device)
        {
            return InjectResources(device, device.TextureLoader);
        }

        public unsafe void Clear()
        {
            nodesT.Clear();
            nodesP.Clear();
            meshesT.Clear();

            if (scene != null)
            {
                assimp.ReleaseImport(scene);
                scene = null;
            }
        }

        private unsafe bool LoadMaterials(AssimpScene* scene)
        {
            try
            {
                textures = new();
                materials = new MaterialData[scene->MNumMaterials];
                for (int i = 0; i < scene->MNumMaterials; i++)
                {
                    Material* mat = scene->MMaterials[i];

                    var material = materials[i] = new MaterialData();

                    List<MaterialProperty> properties = new();
                    List<MaterialTexture> textures = new();
                    List<MaterialShader> shaders = new();

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
                                var file = FindOrCreate(textures, (TextureType)semantic).File = Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1));
                                if (!this.textures.Contains(file))
                                {
                                    this.textures.Add(file);
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

                    bool rm = false;
                    bool orm = false;
                    for (int j = 0; j < material.Textures.Count; j++)
                    {
                        if (material.Textures[j].Type == Core.IO.Materials.MaterialTextureType.RoughnessMetallic)
                        {
                            rm = true;
                        }
                        if (material.Textures[j].Type == Core.IO.Materials.MaterialTextureType.AmbientOcclusionRoughnessMetallic)
                        {
                            orm = true;
                        }
                        if (material.Textures[j].File == null)
                        {
                            material.Textures.MutateItem(j, x => { x.File = string.Empty; return x; });
                        }
                    }

                    if (rm)
                    {
                        for (int k = 0; k < material.Textures.Count; k++)
                        {
                            if (material.Textures[k].Type == Core.IO.Materials.MaterialTextureType.Metallic)
                            {
                                material.Textures.MutateItem(k, x => { x.File = string.Empty; return x; });
                            }

                            if (material.Textures[k].Type == Core.IO.Materials.MaterialTextureType.Roughness)
                            {
                                material.Textures.MutateItem(k, x => { x.File = string.Empty; return x; });
                            }
                        }
                    }

                    if (orm)
                    {
                        for (int k = 0; k < material.Textures.Count; k++)
                        {
                            if (material.Textures[k].Type == Core.IO.Materials.MaterialTextureType.Metallic)
                            {
                                material.Textures.MutateItem(k, x => { x.File = string.Empty; return x; });
                            }

                            if (material.Textures[k].Type == Core.IO.Materials.MaterialTextureType.Roughness)
                            {
                                material.Textures.MutateItem(k, x => { x.File = string.Empty; return x; });
                            }

                            if (material.Textures[k].Type == Core.IO.Materials.MaterialTextureType.RoughnessMetallic)
                            {
                                material.Textures.MutateItem(k, x => { x.File = string.Empty; return x; });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                MessageBox.Show("Failed to load materials", ex.Message);
                return false;
            }

            return true;
        }

        private unsafe bool LoadAnimations(AssimpScene* scene)
        {
            try
            {
                animations = new AnimationData[scene->MNumAnimations];
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
                            channel.Keyframes.Add(new() { Time = key.MTime, MeshName = meshesT[scene->MMeshes[key.MValue]].Name });
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
                    animationsT.Add(anim->MName, animation);
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

        private unsafe bool LoadMeshes(AssimpScene* scene)
        {
            try
            {
                meshes = new MeshData[scene->MNumMeshes];
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
                    Vector3[]? bitangents = ToManaged(msh->MBitangents, msh->MNumVertices);

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
                            nodesN[bn->MName].Flags |= NodeFlags.Bone;

                            Core.Meshes.VertexWeight[] weights = new Core.Meshes.VertexWeight[bn->MNumWeights];
                            for (int x = 0; x < weights.Length; x++)
                            {
                                weights[x] = new(bn->MWeights[x].MVertexId, bn->MWeights[x].MWeight);
                            }

                            bones[j] = new BoneData(bn->MName, weights, Matrix4x4.Transpose(bn->MOffsetMatrix));
                        }
                    }
                    if (bones.Length > 0)
                    {
                        meshes[i] = new MeshData(msh->MName, materials[(int)msh->MMaterialIndex].Name, box, sphere, msh->MNumVertices, (uint)indices.Length, (uint)bones.Length, indices, colors, positions, uvs, normals, tangents, bitangents, bones.ToArray());
                    }
                    else
                    {
                        meshes[i] = new MeshData(msh->MName, materials[(int)msh->MMaterialIndex].Name, box, sphere, msh->MNumVertices, (uint)indices.Length, 0u, indices, colors, positions, uvs, normals, tangents, bitangents, null);
                    }

                    meshesT.Add(msh, meshes[i]);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                MessageBox.Show("Failed to load meshes", ex.Message);
                return false;
            }
            return true;
        }

        private unsafe bool LoadSceneGraph(AssimpScene* scene)
        {
            try
            {
                nodes = new();
                root = WalkNode(scene->MRootNode, null);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                MessageBox.Show("Failed to load scene graph", ex.Message);
                return false;
            }
            return true;
        }

        private unsafe Node WalkNode(AssimpNode* node, Node parent)
        {
            string name = node->MName;
            Matrix4x4 transform = Matrix4x4.Transpose(node->MTransformation);

            Node sceneNode = new(name, transform, node->MNumMeshes == 0 ? NodeFlags.None : NodeFlags.Drawable, parent, new(new()));

            for (int i = 0; i < node->MNumMeshes; i++)
            {
                sceneNode.Meshes.Add(node->MMeshes[i]);
            }

            for (int i = 0; i < node->MNumChildren; i++)
            {
                var child = WalkNode(node->MChildren[i], sceneNode);
                sceneNode.Children.Add(child);
            }

            nodesT.Add(node, sceneNode);
            nodesP.Add(sceneNode, node);
            nodesN.Add(name, sceneNode);
            nodes.Add(sceneNode);
            return sceneNode;
        }

        private static void SwapImage(ref IScratchImage before, IScratchImage after)
        {
            before.Dispose();
            before = after;
        }

        private unsafe bool InjectResources(IGraphicsDevice device, ITextureLoader textureLoader)
        {
            for (int i = 0; i < textures.Count; i++)
            {
                var srcFile = Path.Combine(dir, textures[i]);
                if (!System.IO.File.Exists(srcFile))
                    continue;
                var dest = Path.Combine(ProjectManager.CurrentProjectAssetsFolder ?? throw new(), "textures");

                if (TexPostProcessSteps == TexPostProcessSteps.None)
                {
                    try
                    {
                        var destFile = Path.Combine(dest, textures[i]);
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
                        image = textureLoader.LoadFormFile(srcFile);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        MessageBox.Show("Failed to load texture", ex.Message);
                        return false;
                    }

                    if ((TexPostProcessSteps & TexPostProcessSteps.Scale) != 0)
                    {
                        try
                        {
                            SwapImage(ref image, image.Resize(TexScaleFactor, TexFilterFlags.Default));
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(ex);
                            MessageBox.Show("Failed to scale texture", ex.Message);
                            return false;
                        }
                    }

                    if ((TexPostProcessSteps & TexPostProcessSteps.GenerateMips) != 0)
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

                    if ((TexPostProcessSteps & TexPostProcessSteps.Convert) != 0)
                    {
                        try
                        {
                            if (FormatHelper.IsCompressed(TexFormat))
                            {
                                SwapImage(ref image, image.Compress(device, TexFormat, TexCompressFlags));
                            }
                            else
                            {
                                SwapImage(ref image, image.Convert(TexFormat, TexFilterFlags.Default));
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
                        var newName = Path.GetFileNameWithoutExtension(textures[i]) + $".{TexFileFormat.ToString().ToLowerInvariant()}";
                        ChangeNameOfTexture(textures[i], newName);
                        var destFile = Path.Combine(dest, newName);
                        Directory.CreateDirectory(Path.GetDirectoryName(destFile));
                        image.SaveToFile(destFile, TexFileFormat, 0);
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

            if (animations.Length > 0)
            {
                try
                {
                    animationLibrary = new(animations);
                    animationLibrary.Save(Path.Combine(ProjectManager.CurrentProjectAssetsFolder ?? throw new(), "animations", name + ".anim"), Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    MessageBox.Show("Failed to save animation library", ex.Message);
                    return false;
                }
            }
            if (materials.Length > 0)
            {
                try
                {
                    materialLibrary = new(materials);
                    materialLibrary.Save(Path.Combine(ProjectManager.CurrentProjectAssetsFolder ?? throw new(), "materials", name + ".matlib"), Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    MessageBox.Show("Failed to save material library", ex.Message);
                    return false;
                }
            }
            if (meshes.Length > 0 || root.Children.Count > 0)
            {
                try
                {
                    modelFile = new(name + ".matlib", meshes, root);
                    modelFile.Save(Path.Combine(ProjectManager.CurrentProjectAssetsFolder ?? throw new(), "meshes", name + ".model"), Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    MessageBox.Show("Failed to save model file", ex.Message);
                    return false;
                }
            }

            return true;
        }

        private static unsafe void Log(byte* message, byte* userdata)
        {
            string msg = Encoding.UTF8.GetString(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(message));
            Logger.Log(msg);
        }

        public static Core.IO.Materials.MaterialTextureType Convert(TextureType type)
        {
            return type switch
            {
                TextureType.None => Core.IO.Materials.MaterialTextureType.None,
                TextureType.Diffuse => Core.IO.Materials.MaterialTextureType.Diffuse,
                TextureType.Specular => Core.IO.Materials.MaterialTextureType.Specular,
                TextureType.Ambient => Core.IO.Materials.MaterialTextureType.Ambient,
                TextureType.Emissive => Core.IO.Materials.MaterialTextureType.Emissive,
                TextureType.Height => Core.IO.Materials.MaterialTextureType.Height,
                TextureType.Normals => Core.IO.Materials.MaterialTextureType.Normal,
                TextureType.Shininess => Core.IO.Materials.MaterialTextureType.Shininess,
                TextureType.Opacity => Core.IO.Materials.MaterialTextureType.Opacity,
                TextureType.Displacement => Core.IO.Materials.MaterialTextureType.Displacement,
                TextureType.Lightmap => Core.IO.Materials.MaterialTextureType.AmbientOcclusionRoughnessMetallic,
                TextureType.Reflection => Core.IO.Materials.MaterialTextureType.Reflection,
                TextureType.BaseColor => Core.IO.Materials.MaterialTextureType.BaseColor,
                TextureType.NormalCamera => Core.IO.Materials.MaterialTextureType.NormalCamera,
                TextureType.EmissionColor => Core.IO.Materials.MaterialTextureType.EmissionColor,
                TextureType.Metalness => Core.IO.Materials.MaterialTextureType.Metallic,
                TextureType.DiffuseRoughness => Core.IO.Materials.MaterialTextureType.Roughness,
                TextureType.AmbientOcclusion => Core.IO.Materials.MaterialTextureType.AmbientOcclusion,
                TextureType.Sheen => Core.IO.Materials.MaterialTextureType.Sheen,
                TextureType.Clearcoat => Core.IO.Materials.MaterialTextureType.Clearcoat,
                TextureType.Transmission => Core.IO.Materials.MaterialTextureType.Transmission,
                TextureType.Unknown => Core.IO.Materials.MaterialTextureType.RoughnessMetallic,
                _ => throw new NotImplementedException(),
            };
        }

        public static Core.IO.Materials.BlendMode Convert(BlendMode mode)
        {
            return mode switch
            {
                BlendMode.Default => Core.IO.Materials.BlendMode.Default,
                BlendMode.Additive => Core.IO.Materials.BlendMode.Additive,
                _ => throw new NotImplementedException(),
            };
        }

        public static Core.IO.Materials.TextureOp Convert(TextureOp op)
        {
            return op switch
            {
                TextureOp.Multiply => Core.IO.Materials.TextureOp.Multiply,
                TextureOp.Add => Core.IO.Materials.TextureOp.Add,
                TextureOp.Subtract => Core.IO.Materials.TextureOp.Subtract,
                TextureOp.Divide => Core.IO.Materials.TextureOp.Divide,
                TextureOp.SmoothAdd => Core.IO.Materials.TextureOp.SmoothAdd,
                TextureOp.SignedAdd => Core.IO.Materials.TextureOp.SignedAdd,
                _ => throw new NotImplementedException(),
            };
        }

        public static Core.IO.Materials.TextureMapMode Convert(TextureMapMode mode)
        {
            return mode switch
            {
                TextureMapMode.Wrap => Core.IO.Materials.TextureMapMode.Wrap,
                TextureMapMode.Clamp => Core.IO.Materials.TextureMapMode.Clamp,
                TextureMapMode.Decal => Core.IO.Materials.TextureMapMode.Decal,
                TextureMapMode.Mirror => Core.IO.Materials.TextureMapMode.Mirror,
                _ => throw new NotImplementedException(),
            };
        }

        public static Core.IO.Materials.TextureFlags Convert(TextureFlags flags)
        {
            return flags switch
            {
                0 => Core.IO.Materials.TextureFlags.None,
                TextureFlags.Invert => Core.IO.Materials.TextureFlags.Invert,
                TextureFlags.UseAlpha => Core.IO.Materials.TextureFlags.UseAlpha,
                TextureFlags.IgnoreAlpha => Core.IO.Materials.TextureFlags.IgnoreAlpha,
                _ => throw new NotImplementedException(),
            };
        }
    }
}