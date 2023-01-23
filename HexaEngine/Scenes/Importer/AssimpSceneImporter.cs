﻿using HexaEngine.Core.IO.Meshes;
using HexaEngine.Core.Scenes;

namespace HexaEngine.Scenes.Importer
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics.Reflection;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;
    using HexaEngine.Projects;
    using HexaEngine.Scenes.Components.Renderer;
    using Silk.NET.Assimp;
    using Silk.NET.Core.Native;
    using System.Diagnostics;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using System.Text;
    using AssimpScene = Silk.NET.Assimp.Scene;
    using Scene = Scene;

    public class AssimpSceneImporter
    {
        private static readonly Assimp assimp = Assimp.GetApi();

        private readonly Dictionary<Pointer<Node>, GameObject> nodesT = new();
        private readonly Dictionary<GameObject, Pointer<Node>> nodesP = new();
        private readonly Dictionary<Pointer<Node>, Objects.Animature> animatureT = new();
        private readonly Dictionary<Pointer<Silk.NET.Assimp.Mesh>, MeshSource> meshesT = new();
        private readonly Dictionary<string, Core.Scenes.Camera> camerasT = new();
        private readonly Dictionary<string, Core.Lights.Light> lightsT = new();
        private List<GameObject> nodes;
        private MeshSource[] meshes;
        private Model[] models;
        private MaterialDesc[] materials;

        private Core.Scenes.Camera[] cameras;
        private Core.Lights.Light[] lights;
        private GameObject root;
        private unsafe AssimpScene* scene;

        public Model[] Models => models;

        public MeshSource[] Meshes => meshes;

        public MaterialDesc[] Materials => materials;

        public PostProcessSteps PostProcessSteps = PostProcessSteps.CalculateTangentSpace | PostProcessSteps.MakeLeftHanded | PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs | PostProcessSteps.FindInvalidData;

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

            LoadCameras(scene);

            LoadLights(scene);

            Free(name);
        }

        public bool CheckForProblems()
        {
            for (int i = 0; i < models.Length; i++)
            {
                Model model = models[i];
                if (model.Mesh.Length > 255)
                {
                    return true;
                }
                if (model.Material.Name.Length > 255)
                {
                    return true;
                }
            }
            return false;
        }

        public bool ChangeNameOfMesh(MeshSource source, string newName)
        {
            if (meshes.Any(x => x.Path == newName))
                return false;
            string oldName = source.Path;
            for (int i = 0; i < models.Length; i++)
            {
                Model model = models[i];
                if (model.Mesh == oldName)
                {
                    model.Mesh = newName;
                }
            }
            source.Path = newName;
            return true;
        }

        public bool ChangeNameOfMaterial(MaterialDesc material, string newName)
        {
            if (materials.Any(x => x.Name == newName))
                return false;
            string oldName = material.Name;
            for (int i = 0; i < models.Length; i++)
            {
                Model source = models[i];
                if (source.Material.Name == oldName)
                {
                    // TODO: Do not store material as instance in model
                    //source.Material = newName;
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
            materials = new MaterialDesc[scene->MNumMaterials];
            for (int i = 0; i < scene->MNumMaterials; i++)
            {
                Silk.NET.Assimp.Material* mat = scene->MMaterials[i];
                Dictionary<(string, object), object> props = new();
                AssimpMaterialTexture[] texs = new AssimpMaterialTexture[(int)TextureType.Unknown + 1];
                for (int j = 0; j < texs.Length; j++)
                {
                    texs[j].Type = (TextureType)j;
                }
                AssimpMaterial material = new();

                for (int j = 0; j < mat->MNumProperties; j++)
                {
                    MaterialProperty* prop = mat->MProperties[j];
                    if (prop == null) continue;
                    Span<byte> buffer = new(prop->MData, (int)prop->MDataLength);
                    string key = prop->MKey;
                    int semantic = (int)prop->MSemantic;

                    switch (key)
                    {
                        case Assimp.MatkeyName:
                            material.Name = Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1));
                            break;

                        case Assimp.MatkeyTwosided:
                            material.Twosided = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyShadingModel:
                            material.ShadingModel = (ShadingMode)MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;

                        case Assimp.MatkeyEnableWireframe:
                            material.EnableWireframe = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyBlendFunc:
                            material.BlendFunc = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyOpacity:
                            material.Opacity = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyTransparencyfactor:
                            material.Transparencyfactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyBumpscaling:
                            material.Bumpscaling = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyShininess:
                            material.Shininess = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyReflectivity:
                            material.Reflectivity = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyShininessStrength:
                            material.ShininessStrength = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyRefracti:
                            material.Refracti = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyColorDiffuse:
                            material.ColorDiffuse = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyColorAmbient:
                            material.ColorAmbient = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyColorSpecular:
                            material.ColorSpecular = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyColorEmissive:
                            material.ColorEmissive = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyColorTransparent:
                            material.ColorTransparent = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyColorReflective:
                            material.ColorReflective = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyUseColorMap:
                            material.UseColorMap = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyBaseColor:
                            material.BaseColor = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyUseMetallicMap:
                            material.UseMetallicMap = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyMetallicFactor:
                            material.MetallicFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyUseRoughnessMap:
                            material.UseRoughnessMap = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyRoughnessFactor:
                            material.RoughnessFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyAnisotropyFactor:
                            material.AnisotropyFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeySpecularFactor:
                            material.SpecularFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyGlossinessFactor:
                            material.GlossinessFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeySheenColorFactor:
                            material.SheenColorFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeySheenRoughnessFactor:
                            material.SheenRoughnessFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyClearcoatFactor:
                            material.ClearcoatFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyClearcoatRoughnessFactor:
                            material.ClearcoatRoughnessFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyTransmissionFactor:
                            material.TransmissionFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyVolumeThicknessFactor:
                            material.VolumeThicknessFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyVolumeAttenuationDistance:
                            material.VolumeAttenuationDistance = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyVolumeAttenuationColor:
                            material.VolumeAttenuationColor = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyUseEmissiveMap:
                            material.UseEmissiveMap = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyEmissiveIntensity:
                            material.EmissiveIntensity = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyUseAOMap:
                            material.UseAOMap = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyTextureBase:
                            texs[semantic].File = Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1));
                            break;

                        case Assimp.MatkeyUvwsrcBase:
                            texs[semantic].UVWSrc = MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;

                        case Assimp.MatkeyTexopBase:
                            texs[semantic].Op = (TextureOp)MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;

                        case Assimp.MatkeyMappingBase:
                            texs[semantic].Mapping = MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;

                        case Assimp.MatkeyTexblendBase:
                            texs[semantic].Blend = (BlendMode)MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;

                        case Assimp.MatkeyMappingmodeUBase:
                            texs[semantic].U = (TextureMapMode)MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;

                        case Assimp.MatkeyMappingmodeVBase:
                            texs[semantic].V = (TextureMapMode)MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;

                        case Assimp.MatkeyTexmapAxisBase:
                            break;

                        case Assimp.MatkeyUvtransformBase:
                            break;

                        case Assimp.MatkeyTexflagsBase:
                            texs[semantic].Flags = (TextureFlags)MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;
                    }
                }
                if (material.Name == string.Empty)
                    material.Name = i.ToString();

                material.Textures = texs;
                materials[i] = new MaterialDesc()
                {
                    BaseColor = material.BaseColor,
                    Ao = 1,
                    Emissive = material.ColorEmissive,
                    Metalness = material.MetallicFactor,
                    Roughness = material.RoughnessFactor,
                    Opacity = material.Opacity,
                    Name = material.Name,
                    BaseColorTextureMap = material.Textures[(int)TextureType.BaseColor].File ?? string.Empty,
                    AoTextureMap = material.Textures[(int)TextureType.AmbientOcclusion].File ?? string.Empty,
                    DisplacementTextureMap = material.Textures[(int)TextureType.Displacement].File ?? string.Empty,
                    EmissiveTextureMap = material.Textures[(int)TextureType.EmissionColor].File ?? string.Empty,
                    MetalnessTextureMap = material.Textures[(int)TextureType.Metalness].File ?? string.Empty,
                    NormalTextureMap = material.Textures[(int)TextureType.Normals].File ?? string.Empty,
                    RoughnessTextureMap = material.Textures[(int)TextureType.DiffuseRoughness].File ?? string.Empty,
                    RoughnessMetalnessTextureMap = material.Textures[(int)TextureType.Unknown].File ?? string.Empty,
                };
            }
        }

        private unsafe void LoadMeshes(AssimpScene* scene)
        {
            meshes = new MeshSource[scene->MNumMeshes];
            models = new Model[scene->MNumMeshes];
            for (int i = 0; i < scene->MNumMeshes; i++)
            {
                Silk.NET.Assimp.Mesh* msh = scene->MMeshes[i];

                MeshVertex[] vertices = new MeshVertex[msh->MNumVertices];
                uint[] indices = new uint[msh->MNumFaces * 3];
                for (int j = 0; j < msh->MNumFaces; j++)
                {
                    var face = msh->MFaces[j];
                    for (int k = 0; k < 3; k++)
                    {
                        indices[j * 3 + k] = face.MIndices[k];
                    }
                }

                Vector3 min = new(0, 0, 0);
                Vector3 max = new(0, 0, 0);
                for (int j = 0; j < msh->MNumVertices; j++)
                {
                    Vector3 pos = msh->MVertices[j];
                    Vector3 nor = msh->MNormals[j];
                    Vector3 tex = default;
                    Vector3 tan = default;
                    if (j == 0)
                    {
                        min = max = pos;
                    }

                    if (msh->MTextureCoords[0] != null)
                        tex = msh->MTextureCoords[0][j];
                    if (msh->MTangents != null)
                        tan = msh->MTangents[j];

                    MeshVertex vertex = new(pos, new(tex.X, tex.Y), nor, tan);
                    vertices[j] = vertex;
                    max = Vector3.Max(max, pos);
                    min = Vector3.Min(min, pos);
                }

                Objects.Animature? animature = null;

                MeshBone[]? bones = new MeshBone[msh->MNumBones];
                if (msh->MNumBones > 0)
                {
                    var node = msh->MBones[0]->MArmature;
                    animature = new(node->MName);
                    animatureT.Add(node, animature);
                    for (int j = 0; j < bones.Length; j++)
                    {
                        var bn = msh->MBones[j];
                        MeshWeight[] weights = new MeshWeight[bn->MNumWeights];
                        for (int x = 0; x < weights.Length; x++)
                        {
                            weights[x] = new() { VertexId = bn->MWeights[x].MVertexId, Weight = bn->MWeights[x].MWeight };
                        }

                        bones[j] = new MeshBone(bn->MName, null, nodesT[bn->MNode], weights, bn->MOffsetMatrix);
                        animature.AddBone(bones[j]);
                    }
                }

                BoundingBox box = new(min, max);

                Vector3 center = box.Center;
                float radius = box.Extent.Length();
                BoundingSphere sphere = new(center, radius);

                meshes[i] = new MeshSource(msh->MName, vertices, indices, box, sphere);
                models[i] = new(msh->MName, materials[(int)msh->MMaterialIndex]);

                meshesT.Add(msh, meshes[i]);
            }

            FileSystem.Refresh();
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
            for (int i = 0; i < meshes.Length; i++)
            {
                meshes[i].Save(Path.Combine(ProjectManager.CurrentProjectAssetsFolder ?? throw new(), "meshes"));
            }
            for (int i = 0; i < materials.Length; i++)
            {
                scene.MaterialManager.Add(materials[i]);
            }
            FileSystem.Refresh();

            for (int x = 0; x < nodes.Count; x++)
            {
                GameObject node = nodes[x];
                Node* p = nodesP[node];
                for (int i = 0; i < p->MNumMeshes; i++)
                {
                    var renderer = node.GetOrCreateComponent<RendererComponent>();
                    var model = models[(int)p->MMeshes[i]];
                    renderer.AddMesh(model);
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
            public MeshWeight[] Weights;
            public Matrix4x4 Offset;
            public Node* Node;

            public AssimpBone(string name, MeshWeight[] weights, Matrix4x4 offset, Node* node)
            {
                Name = name;
                Weights = weights;
                Offset = offset;
                Node = node;
            }
        }
    }
}