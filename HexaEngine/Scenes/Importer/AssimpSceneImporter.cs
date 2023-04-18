using HexaEngine.Core.IO.Meshes;
using HexaEngine.Core.Scenes;

namespace HexaEngine.Scenes.Importer
{
    using HexaEngine.Core;
    using HexaEngine.Core.Animations;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.Lights.Types;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Core.Extensions;
    using HexaEngine.Mathematics;
    using HexaEngine.Projects;
    using HexaEngine.Scenes.Components.Renderer;
    using Silk.NET.Assimp;
    using System.Diagnostics;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using System.Text;
    using Animation = Core.Animations.Animation;
    using AssimpScene = Silk.NET.Assimp.Scene;
    using BlendMode = Silk.NET.Assimp.BlendMode;
    using AssimpMaterialProperty = Silk.NET.Assimp.MaterialProperty;
    using MaterialProperty = Core.IO.Materials.MaterialProperty;
    using Scene = Scene;
    using TextureFlags = Silk.NET.Assimp.TextureFlags;
    using TextureMapMode = Silk.NET.Assimp.TextureMapMode;
    using TextureOp = Silk.NET.Assimp.TextureOp;
    using TextureType = Silk.NET.Assimp.TextureType;
    using HexaEngine.Core.Graphics.Reflection;

    public struct BoneInfo
    {
        /*id is index in finalBoneMatrices*/
        public int Id;

        /*offset matrix transforms vertex from model space to bone space*/
        public Matrix4x4 Offset;
    }

    public class AssimpSceneImporter
    {
        private static readonly Assimp assimp = Assimp.GetApi();

        private readonly Dictionary<Pointer<Node>, GameObject> nodesT = new();
        private readonly Dictionary<GameObject, Pointer<Node>> nodesP = new();
        private readonly Dictionary<Pointer<Mesh>, Objects.Animature> animatureT = new();
        private readonly Dictionary<Pointer<Mesh>, MeshData> meshesT = new();
        private readonly Dictionary<Pointer<Node>, ModelFile> modelsT = new();
        private readonly Dictionary<string, Core.Scenes.Camera> camerasT = new();
        private readonly Dictionary<string, Core.Lights.Light> lightsT = new();
        private readonly Dictionary<string, Animation> animationsT = new();

        private List<GameObject> nodes;
        private List<ModelFile> models;
        private MeshData[] meshes;
        private MaterialData[] materials;
        private Animation[] animations;
        private Core.Scenes.Camera[] cameras;
        private Core.Lights.Light[] lights;
        private GameObject root;
        private unsafe AssimpScene* scene;

        public IReadOnlyList<ModelFile> Models => models;

        public MeshData[] Meshes => meshes;

        public MaterialData[] Materials => materials;

        public PostProcessSteps PostProcessSteps = PostProcessSteps.CalculateTangentSpace | PostProcessSteps.MakeLeftHanded | PostProcessSteps.Triangulate | PostProcessSteps.FindInvalidData;

        public Task LoadAsync(string path)
        {
            return Task.Run(() => Load(path));
        }

        public Task ImportAsync(Scene scene)
        {
            return Task.Run(() => Import(scene));
        }

        public Task ImportAsync()
        {
            return Task.Run(() => Import(SceneManager.Current));
        }

        public unsafe void Load(string path)
        {
            var name = "HexaEngine".ToUTF8();
            LogStream stream = new(new(Log), name);
            assimp.AttachLogStream(&stream);
            assimp.EnableVerboseLogging(Assimp.True);

            scene = assimp.ImportFile(path, (uint)(ImporterFlags.SupportBinaryFlavour | ImporterFlags.SupportTextFlavour | ImporterFlags.SupportCompressedFlavour));
            assimp.ApplyPostProcessing(scene, (uint)PostProcessSteps);

            LoadSceneGraph(scene);

            LoadMaterials(scene);

            LoadMeshes(scene);

            LoadModels(scene);

            LoadAnimations(scene);

            LoadCameras(scene);

            LoadLights(scene);

            Free(name);
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

        public bool ChangeNameOfModel(ModelFile source, string newName)
        {
            if (meshes.Any(x => x.Name == newName))
                return false;

            source.Name = newName;
            return true;
        }

        public bool ChangeNameOfMaterial(MaterialData material, string newName)
        {
            if (materials.Any(x => x.Name == newName))
                return false;
            string oldName = material.Name;
            for (int i = 0; i < meshes.Length; i++)
            {
                ModelFile source = models[i];
                for (ulong j = 0; j < source.Header.MeshCount; j++)
                {
                    MaterialData mat = source.GetMaterial(j);
                    if (mat.Name == oldName)
                    {
                        mat.Name = newName;
                        source.SetMaterial(j, mat);
                    }
                }
            }
            material.Name = newName;
            return true;
        }

        public unsafe void Import(Scene scene)
        {
            InjectResources(scene);

            if (root.Name == "ROOT")
                scene.Merge(root);
            else
                scene.AddChild(root);
        }

        public unsafe void Clear()
        {
            nodesT.Clear();
            nodesP.Clear();
            meshesT.Clear();
            camerasT.Clear();
            lightsT.Clear();

            if (scene != null)
            {
                assimp.ReleaseImport(scene);
                scene = null;
            }
        }

        private unsafe void LoadTextures(AssimpScene* scene)
        {
            AssimpTexture[] textures = new AssimpTexture[scene->MNumTextures];
            for (int i = 0; i < scene->MNumTextures; i++)
            {
                var tex = scene->MTextures[i];
                textures[i] = new()
                {
                    Data = tex->PcData,
                    Format = new Span<int>(tex->AchFormatHint, 1)[0],
                    Height = (int)tex->MHeight,
                    Width = (int)tex->MWidth
                };
            }
        }

        private unsafe void LoadMaterials(AssimpScene* scene)
        {
            materials = new MaterialData[scene->MNumMaterials];
            for (int i = 0; i < scene->MNumMaterials; i++)
            {
                Material* mat = scene->MMaterials[i];
                Dictionary<(string, object), object> props = new();
                AssimpMaterialTexture[] texs = new AssimpMaterialTexture[(int)TextureType.Transmission];
                for (int j = 0; j < texs.Length; j++)
                {
                    texs[j].Type = (TextureType)j;
                }

                var material = materials[i] = new MaterialData();

                List<MaterialProperty> properties = new();
                List<MaterialTexture> textures = new();

                for (int j = 0; j < mat->MNumProperties; j++)
                {
                    AssimpMaterialProperty* prop = mat->MProperties[j];
                    if (prop == null) continue;
                    Span<byte> buffer = new(prop->MData, (int)prop->MDataLength);
                    string key = prop->MKey;
                    int semantic = (int)prop->MSemantic;

                    static ref MaterialTexture FindOrCreate(List<MaterialTexture> textures, TextureType type)
                    {
                        var t = AssimpMaterialTexture.Convert(type);
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
                            properties.Add(new("Refractive", MaterialPropertyType.Refractive, MaterialValueType.Float, default, sizeof(float), buffer.ToArray()));
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
                            FindOrCreate(textures, (TextureType)semantic).File = Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1));
                            break;

                        case Assimp.MatkeyUvwsrcBase:
                            FindOrCreate(textures, (TextureType)semantic).UVWSrc = MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;

                        case Assimp.MatkeyTexopBase:
                            FindOrCreate(textures, (TextureType)semantic).Op = AssimpMaterialTexture.Convert((TextureOp)MemoryMarshal.Cast<byte, int>(buffer)[0]);
                            break;

                        case Assimp.MatkeyMappingBase:
                            FindOrCreate(textures, (TextureType)semantic).Mapping = MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;

                        case Assimp.MatkeyTexblendBase:
                            FindOrCreate(textures, (TextureType)semantic).Blend = AssimpMaterialTexture.Convert((BlendMode)MemoryMarshal.Cast<byte, int>(buffer)[0]);
                            break;

                        case Assimp.MatkeyMappingmodeUBase:
                            FindOrCreate(textures, (TextureType)semantic).U = AssimpMaterialTexture.Convert((TextureMapMode)MemoryMarshal.Cast<byte, int>(buffer)[0]);
                            break;

                        case Assimp.MatkeyMappingmodeVBase:
                            FindOrCreate(textures, (TextureType)semantic).V = AssimpMaterialTexture.Convert((TextureMapMode)MemoryMarshal.Cast<byte, int>(buffer)[0]);
                            break;

                        case Assimp.MatkeyTexmapAxisBase:
                            break;

                        case Assimp.MatkeyUvtransformBase:
                            break;

                        case Assimp.MatkeyTexflagsBase:
                            FindOrCreate(textures, (TextureType)semantic).Flags = AssimpMaterialTexture.Convert((TextureFlags)MemoryMarshal.Cast<byte, int>(buffer)[0]);
                            break;

                        case Assimp.MatkeyShaderVertex:
                            material.VertexShader = Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1));
                            break;

                        case Assimp.MatkeyShaderTesselation:
                            material.HullShader = Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1));
                            break;

                        case Assimp.MatkeyShaderPrimitive:
                            material.DomainShader = Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1));
                            break;

                        case Assimp.MatkeyShaderGeo:
                            material.GeometryShader = Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1));
                            break;

                        case Assimp.MatkeyShaderFragment:
                            material.PixelShader = Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1));
                            break;

                        case Assimp.MatkeyShaderCompute:
                            material.ComputeShader = Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1));
                            break;
                    }
                }

                if (material.Name == string.Empty)
                    material.Name = i.ToString();

                material.Properties = properties.ToArray();
                material.Textures = textures.ToArray();

                bool rm = false;
                bool orm = false;
                for (int j = 0; j < material.Textures.Length; j++)
                {
                    if (material.Textures[j].Type == Core.IO.Materials.TextureType.RoughnessMetalness)
                    {
                        rm = true;
                    }
                    if (material.Textures[j].Type == Core.IO.Materials.TextureType.AmbientOcclusionRoughnessMetalness)
                    {
                        orm = true;
                    }
                    if (material.Textures[j].File == null)
                    {
                        material.Textures[j].File = string.Empty;
                    }
                }

                if (rm)
                {
                    for (int k = 0; k < material.Textures.Length; k++)
                    {
                        if (material.Textures[k].Type == Core.IO.Materials.TextureType.Metalness)
                            material.Textures[k].File = string.Empty;
                        if (material.Textures[k].Type == Core.IO.Materials.TextureType.Roughness)
                            material.Textures[k].File = string.Empty;
                    }
                }

                if (orm)
                {
                    for (int k = 0; k < material.Textures.Length; k++)
                    {
                        if (material.Textures[k].Type == Core.IO.Materials.TextureType.Metalness)
                            material.Textures[k].File = string.Empty;
                        if (material.Textures[k].Type == Core.IO.Materials.TextureType.Roughness)
                            material.Textures[k].File = string.Empty;
                        if (material.Textures[k].Type == Core.IO.Materials.TextureType.RoughnessMetalness)
                            material.Textures[k].File = string.Empty;
                    }
                }
            }
        }

        private unsafe void LoadAnimations(AssimpScene* scene)
        {
            animations = new Animation[scene->MNumAnimations];
            for (int i = 0; i < scene->MNumAnimations; i++)
            {
                var anim = scene->MAnimations[i];
                Animation animation = new(anim->MName, anim->MDuration, anim->MTicksPerSecond);
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
                animationsT.Add(animation.Name, animation);
            }
        }

        private unsafe void ExtractBoneWeightForVertices(SkinnedMeshVertex[] vertices, Mesh* mesh, AssimpScene* scene)
        {
            Dictionary<string, BoneInfo> boneInfoMap = new();
            int boneCount = 0;

            for (int boneIndex = 0; boneIndex < mesh->MNumBones; ++boneIndex)
            {
                int boneID = -1;
                var boneName = mesh->MBones[boneIndex]->MName;
                if (!boneInfoMap.ContainsKey(boneName))
                {
                    BoneInfo newBoneInfo;
                    newBoneInfo.Id = boneCount;
                    newBoneInfo.Offset = mesh->MBones[boneIndex]->MOffsetMatrix;
                    boneInfoMap[boneName] = newBoneInfo;
                    boneID = boneCount;
                    boneCount++;
                }
                else
                {
                    boneID = boneInfoMap[boneName].Id;
                }
                Trace.Assert(boneID != -1);
                var weights = mesh->MBones[boneIndex]->MWeights;
                uint numWeights = mesh->MBones[boneIndex]->MNumWeights;

                fixed (SkinnedMeshVertex* pVertices = &vertices[0])
                    for (int weightIndex = 0; weightIndex < numWeights; ++weightIndex)
                    {
                        uint vertexId = weights[weightIndex].MVertexId;
                        float weight = weights[weightIndex].MWeight;
                        Trace.Assert(vertexId <= vertices.Length);
                        SetVertexBoneData(&pVertices[vertexId], boneID, weight);
                    }
            }
        }

        private unsafe void SetVertexBoneData(SkinnedMeshVertex* vertex, int boneID, float weight)
        {
            for (int i = 0; i < 4; ++i)
            {
                if (vertex->BoneIds[i] < 0)
                {
                    vertex->Weights[i] = weight;
                    vertex->BoneIds[i] = boneID;
                    break;
                }
            }
        }

        private unsafe void LoadMeshes(AssimpScene* scene)
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

                Objects.Animature? animature = null;

                BoneData[] bones = new BoneData[msh->MNumBones];
                List<Core.Meshes.VertexWeight> weightList = new();
                if (msh->MNumBones > 0)
                {
                    Queue<GameObject> queue = new();

                    animature = new(msh->MName);
                    Stack<Pointer<Node>> walkstack = new();

                    for (int j = 0; j < bones.Length; j++)
                    {
                        var bn = msh->MBones[j];
                        var node = FindNode(scene, bn->MName);

                        Core.Meshes.VertexWeight[] weights = new Core.Meshes.VertexWeight[bn->MNumWeights];
                        for (int x = 0; x < weights.Length; x++)
                        {
                            weights[x] = new(bn->MWeights[x].MVertexId, bn->MWeights[x].MWeight);
                        }
                        weightList.AddRange(weights);
                        bones[j] = new BoneData(bn->MName, weights, bn->MOffsetMatrix);
                        animature.AddBone(bones[j]);
                    }

                    animatureT.Add(msh, animature);
                }
                if (weightList.Count > 0)
                {
                    meshes[i] = new MeshData(msh->MName, materials[(int)msh->MMaterialIndex], box, sphere, msh->MNumVertices, (uint)indices.Length, (uint)weightList.Count, indices, colors, positions, uvs, normals, tangents, bitangents, bones.ToArray());
                }
                else
                {
                    meshes[i] = new MeshData(msh->MName, materials[(int)msh->MMaterialIndex], box, sphere, msh->MNumVertices, (uint)indices.Length, 0u, indices, colors, positions, uvs, normals, tangents, bitangents, null);
                }

                meshesT.Add(msh, meshes[i]);
            }

            FileSystem.Refresh();
        }

        private unsafe void LoadModels(AssimpScene* scene)
        {
            models = new();
            for (int x = 0; x < nodes.Count; x++)
            {
                GameObject node = nodes[x];
                Node* p = nodesP[node];
                if (p->MNumMeshes == 0)
                    continue;
                MeshData[] meshes = new MeshData[p->MNumMeshes];
                for (int i = 0; i < p->MNumMeshes; i++)
                {
                    meshes[i] = this.meshes[(int)p->MMeshes[i]];
                }
                ModelFile model = new(p->MName, meshes);
                models.Add(model);
                modelsT.Add(p, model);
            }
        }

        private unsafe void LoadCameras(AssimpScene* scene)
        {
            cameras = new Core.Scenes.Camera[scene->MNumCameras];

            for (int i = 0; i < scene->MNumCameras; i++)
            {
                var cam = scene->MCameras[i];
                var camera = cameras[i] = new()
                {
                    Name = cam->MName,
                };
                camera.Transform.Fov = cam->MHorizontalFOV.ToDeg();
                camera.Transform.Width = cam->MOrthographicWidth;
                camera.Transform.Height = 1f / cam->MOrthographicWidth * cam->MAspect;
                camera.Transform.Far = cam->MClipPlaneFar;
                camera.Transform.Near = cam->MClipPlaneNear;
                camerasT.Add(camera.Name, camera);
            }
        }

        private unsafe void LoadLights(AssimpScene* scene)
        {
            lights = new Core.Lights.Light[scene->MNumLights];
            for (int i = 0; i < scene->MNumLights; i++)
            {
                var lig = scene->MLights[i];
                Core.Lights.Light light;
                switch (lig->MType)
                {
                    case LightSourceType.Undefined:
                        throw new NotSupportedException();

                    case LightSourceType.Directional:
                        var dir = new DirectionalLight();
                        dir.Color = new Vector4(lig->MColorDiffuse, 1);
                        light = dir;
                        break;

                    case LightSourceType.Point:
                        var point = new PointLight();
                        point.Color = new Vector4(Vector3.Normalize(lig->MColorDiffuse), 1);
                        point.Strength = lig->MColorDiffuse.Length();
                        light = point;
                        break;

                    case LightSourceType.Spot:
                        var spot = new Spotlight();
                        spot.Color = new Vector4(lig->MColorDiffuse, 1);
                        spot.Strength = 1;
                        spot.ConeAngle = lig->MAngleOuterCone.ToDeg();
                        spot.Blend = lig->MAngleInnerCone.ToDeg() / spot.ConeAngle;
                        light = spot;
                        break;

                    case LightSourceType.Ambient:
                        throw new NotSupportedException();

                    case LightSourceType.Area:
                        throw new NotSupportedException();
                    default:
                        throw new NotSupportedException();
                }

                light.Name = lig->MName;
                lights[i] = light;
                lightsT.Add(light.Name, light);
            }
        }

        private unsafe void LoadSceneGraph(AssimpScene* scene)
        {
            nodes = new();
            root = WalkNode(scene->MRootNode, null);
        }

        private readonly Stack<Pointer<Node>> findWalkStack = new();

        private unsafe Node* FindNode(AssimpScene* scene, string name)
        {
            findWalkStack.Clear();
            findWalkStack.Push(scene->MRootNode);
            while (findWalkStack.Count > 0)
            {
                Node* node = findWalkStack.Pop();
                if (node->MName == name)
                {
                    return node;
                }
                else
                {
                    for (int i = 0; i < node->MNumChildren; i++)
                    {
                        findWalkStack.Push(node->MChildren[i]);
                    }
                }
            }
            return null;
        }

        private unsafe GameObject WalkNode(Node* node, GameObject? parent)
        {
            string name = node->MName;
            GameObject sceneNode = new();

            if (camerasT.TryGetValue(name, out var camera))
            {
                sceneNode = camera;
            }

            if (lightsT.TryGetValue(name, out var light))
            {
                sceneNode = light;
            }

            sceneNode.Name = name;
            Matrix4x4.Decompose(Matrix4x4.Transpose(node->MTransformation), out var scale, out var orientation, out var position);
            sceneNode.Transform.PositionRotationScale = (position, orientation, scale);

            for (int i = 0; i < node->MNumChildren; i++)
            {
                var child = WalkNode(node->MChildren[i], sceneNode);
                sceneNode.AddChild(child);
            }

            nodesT.Add(node, sceneNode);
            nodesP.Add(sceneNode, node);
            nodes.Add(sceneNode);
            return sceneNode;
        }

        private unsafe void InjectResources(Scene scene)
        {
            for (int i = 0; i < models.Count; i++)
            {
                models[i].Save(Path.Combine(ProjectManager.CurrentProjectAssetsFolder ?? throw new(), "meshes"), Encoding.UTF8);
            }

            for (int i = 0; i < models.Count; i++)
            {
                MaterialLibrary library = new(models[i].Name, models[i].GetMaterials());
                library.Save(Path.Combine(ProjectManager.CurrentProjectAssetsFolder ?? throw new(), "materials"), Encoding.UTF8);
            }

            for (int i = 0; i < animations.Length; i++)
            {
                animations[i].Save(Path.Combine(ProjectManager.CurrentProjectAssetsFolder ?? throw new(), "animations"));
            }

            FileSystem.Refresh();

            for (int x = 0; x < nodes.Count; x++)
            {
                GameObject node = nodes[x];
                Node* p = nodesP[node];

                if (modelsT.TryGetValue(p, out ModelFile? value))
                {
                    var component = node.GetOrCreateComponent<ModelRendererComponent>();
                    var model = value;
                    component.Model = "meshes/" + model.Name + ".model";
                }
            }
        }

        private static unsafe void Log(byte* a, byte* b)
        {
            string user = Encoding.UTF8.GetString(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(b));
            string msg = Encoding.UTF8.GetString(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(a));
            ImGuiConsole.Log(msg);
        }

        public struct AssimpMaterial
        {
            public string Name = string.Empty;
            public bool Twosided = false;
            public ShadingMode ShadingModel = ShadingMode.Gouraud;
            public bool EnableWireframe = false;
            public bool BlendFunc = false;
            public float Opacity = 1;
            public float Transparencyfactor = 1;
            public float Bumpscaling = 1;
            public float Shininess = 0;
            public float Reflectivity = 0;
            public float ShininessStrength = 1;
            public float Refracti = 1;
            public Vector3 ColorDiffuse = Vector3.Zero;
            public Vector3 ColorAmbient = Vector3.Zero;
            public Vector3 ColorSpecular = Vector3.Zero;
            public Vector3 ColorEmissive = Vector3.Zero;
            public Vector3 ColorTransparent = Vector3.Zero;
            public Vector3 ColorReflective = Vector3.Zero;

            public bool UseColorMap = false;
            public Vector3 BaseColor = Vector3.Zero;
            public bool UseMetallicMap = false;
            public float MetallicFactor = 0f;
            public bool UseRoughnessMap = false;
            public float RoughnessFactor = 0.5f;
            public float AnisotropyFactor = 0f;
            public float SpecularFactor = 0.5f;
            public float GlossinessFactor = 0f;
            public float SheenColorFactor = 0f;
            public float SheenRoughnessFactor = 0.5f;
            public float ClearcoatFactor = 0f;
            public float ClearcoatRoughnessFactor = 0.03f;
            public float TransmissionFactor = 0f;
            public float VolumeThicknessFactor = 0;
            public float VolumeAttenuationDistance = 0;
            public Vector3 VolumeAttenuationColor = Vector3.Zero;
            public bool UseEmissiveMap = false;
            public float EmissiveIntensity = 1;
            public bool UseAOMap = false;
            public AssimpMaterialTexture[] Textures = Array.Empty<AssimpMaterialTexture>();

            public AssimpMaterial()
            {
            }
        }

        public struct AssimpMaterialTexture
        {
            public TextureType Type;
            public string File;
            public BlendMode Blend;
            public TextureOp Op;
            public int Mapping;
            public int UVWSrc;
            public TextureMapMode U;
            public TextureMapMode V;
            public TextureFlags Flags;

            public static implicit operator MaterialTexture(AssimpMaterialTexture texture)
            {
                return new MaterialTexture(Convert(texture.Type),
                                           texture.File ?? string.Empty,
                                           Convert(texture.Blend),
                                           Convert(texture.Op),
                                           texture.Mapping,
                                           texture.UVWSrc,
                                           Convert(texture.U),
                                           Convert(texture.V),
                                           Convert(texture.Flags));
            }

            public static Core.IO.Materials.TextureType Convert(TextureType type)
            {
                return type switch
                {
                    TextureType.None => Core.IO.Materials.TextureType.None,
                    TextureType.Diffuse => Core.IO.Materials.TextureType.Diffuse,
                    TextureType.Specular => Core.IO.Materials.TextureType.Specular,
                    TextureType.Ambient => Core.IO.Materials.TextureType.Ambient,
                    TextureType.Emissive => Core.IO.Materials.TextureType.Emissive,
                    TextureType.Height => Core.IO.Materials.TextureType.Height,
                    TextureType.Normals => Core.IO.Materials.TextureType.Normal,
                    TextureType.Shininess => Core.IO.Materials.TextureType.Shininess,
                    TextureType.Opacity => Core.IO.Materials.TextureType.Opacity,
                    TextureType.Displacement => Core.IO.Materials.TextureType.Displacement,
                    TextureType.Lightmap => Core.IO.Materials.TextureType.AmbientOcclusionRoughnessMetalness,
                    TextureType.Reflection => Core.IO.Materials.TextureType.Reflection,
                    TextureType.BaseColor => Core.IO.Materials.TextureType.BaseColor,
                    TextureType.NormalCamera => Core.IO.Materials.TextureType.NormalCamera,
                    TextureType.EmissionColor => Core.IO.Materials.TextureType.EmissionColor,
                    TextureType.Metalness => Core.IO.Materials.TextureType.Metalness,
                    TextureType.DiffuseRoughness => Core.IO.Materials.TextureType.Roughness,
                    TextureType.AmbientOcclusion => Core.IO.Materials.TextureType.AmbientOcclusion,
                    TextureType.Sheen => Core.IO.Materials.TextureType.Sheen,
                    TextureType.Clearcoat => Core.IO.Materials.TextureType.Clearcoat,
                    TextureType.Transmission => Core.IO.Materials.TextureType.Transmission,
                    TextureType.Unknown => Core.IO.Materials.TextureType.RoughnessMetalness,
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

        public unsafe struct AssimpTexture
        {
            public int Format;
            public int Width;
            public int Height;
            public void* Data;
        }

        public unsafe struct AssimpBone
        {
            public string Name;
            public Core.Meshes.VertexWeight[] Weights;
            public Matrix4x4 Offset;
            public Node* Node;

            public AssimpBone(string name, Core.Meshes.VertexWeight[] weights, Matrix4x4 offset, Node* node)
            {
                Name = name;
                Weights = weights;
                Offset = offset;
                Node = node;
            }
        }
    }
}