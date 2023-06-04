namespace HexaEngine.Scenes.Components.Renderer
{
    using HexaEngine.Core;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Renderers;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.Mathematics;
    using System.Numerics;
    using System.Threading.Tasks;

    [EditorComponent(typeof(SkinnedMeshRenderer), "Skinned Mesh Renderer")]
    public class SkinnedMeshRenderer : IRendererComponent
    {
        private string model = string.Empty;

        private GameObject? gameObject;
        private ModelManager modelManager;
        private MaterialManager materialManager;
        private StructuredBuffer<Matrix4x4> matrixBuffer;
        private StructuredBuffer<uint> offsetBuffer;
        private StructuredBuffer<Matrix4x4> boneMatrixBuffer;
        private StructuredBuffer<uint> boneOffsetBuffer;
        private ResourceRef<IBuffer> camera;

        private Node[]? nodes;
        private Mesh[]? meshes;
        private Material[]? materials;
        private int[][]? drawables;
        private Matrix4x4[]? locals;
        private Matrix4x4[]? globals;
        private Matrix4x4[]? boneLocals;
        private Matrix4x4[]? boneGlobals;
        private PlainNode[]? plainNodes;
        private PlainNode[]? bones;
        private BoundingBox boundingBox;

        static SkinnedMeshRenderer()
        {
        }

        [EditorProperty("Model", null, "*.mesh")]
        public string Model
        {
            get => model;
            set
            {
                model = value;
                UpdateModel();
            }
        }

        [JsonIgnore]
        public uint QueueIndex { get; } = (uint)RenderQueueIndex.Geometry;

        [JsonIgnore]
        public RendererFlags Flags { get; } = RendererFlags.All;

        [JsonIgnore]
        public BoundingBox BoundingBox { get => BoundingBox.Transform(boundingBox, gameObject.Transform); }

        public void SetLocal(Matrix4x4 local, uint nodeId)
        {
            if (locals != null)
                locals[nodeId] = local;
            gameObject.SendUpdateTransformed();
        }

        public Matrix4x4 GetLocal(uint nodeId)
        {
            if (locals == null)
                return Matrix4x4.Identity;
            return locals[nodeId];
        }

        public void SetBoneLocal(Matrix4x4 local, uint boneId)
        {
            if (boneLocals != null)
                boneLocals[boneId] = local;
            gameObject.SendUpdateTransformed();
        }

        public Matrix4x4 GetBoneLocal(uint boneId)
        {
            if (boneLocals == null)
                return Matrix4x4.Identity;
            return boneLocals[boneId];
        }

        public int GetNodeIdByName(string name)
        {
            if (plainNodes == null)
                return -1;
            for (int i = 0; i < plainNodes.Length; i++)
            {
                var node = plainNodes[i];
                if (node.Name == name)
                    return node.Id;
            }
            return -1;
        }

        public int GetBoneIdByName(string name)
        {
            if (bones == null)
                return -1;
            for (int i = 0; i < bones.Length; i++)
            {
                var bone = bones[i];
                if (bone.Name == name)
                    return bone.Id;
            }
            return -1;
        }

        public void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            this.gameObject = gameObject;

            modelManager = gameObject.GetScene().ModelManager;
            materialManager = gameObject.GetScene().MaterialManager;
            camera = ResourceManager2.Shared.GetBuffer("CBCamera");

            matrixBuffer = new(device, CpuAccessFlags.Write);
            offsetBuffer = new(device, 1, CpuAccessFlags.Write);
            offsetBuffer.Add(0);
            offsetBuffer.Update(device.Context);
            boneMatrixBuffer = new(device, CpuAccessFlags.Write);
            boneOffsetBuffer = new(device, 1, CpuAccessFlags.Write);
            boneOffsetBuffer.Add(0);
            boneOffsetBuffer.Update(device.Context);

            UpdateModel();
        }

        public void Destory()
        {
            if (meshes == null)
                return;

            for (int i = 0; i < meshes.Length; i++)
            {
                ResourceManager.UnloadMesh(meshes[i]);
                ResourceManager.UnloadMaterial(materials[i]);
            }

            matrixBuffer.Dispose();
            offsetBuffer.Dispose();
            boneMatrixBuffer.Dispose();
            boneOffsetBuffer.Dispose();
            nodes = null;
            meshes = null;
            materials = null;
            drawables = null;
            locals = null;
            globals = null;
            boneLocals = null;
            boneGlobals = null;
            plainNodes = null;
            bones = null;
        }

        public void Update(IGraphicsContext context)
        {
            if (drawables == null)
                return;

            globals[0] = locals[0] * gameObject.Transform.Global;

            for (int i = 1; i < plainNodes.Length; i++)
            {
                var node = plainNodes[i];
                globals[i] = locals[node.Id] * globals[node.ParentId];
            }

            matrixBuffer.ResetCounter();

            for (int i = 0; i < drawables.Length; i++)
            {
                var drawable = drawables[i];
                if (drawable == null)
                    continue;
                for (int j = 0; j < drawable.Length; j++)
                {
                    var id = drawable[j];
                    matrixBuffer.Add(Matrix4x4.Transpose(globals[id]));
                }
            }

            matrixBuffer.Update(context);

            if (bones == null)
                return;

            for (int i = 0; i < bones.Length; i++)
            {
                var bone = bones[i];

                if (bone.ParentId == -1)
                {
                    boneGlobals[i] = boneLocals[bone.Id];
                }
                else
                {
                    boneGlobals[i] = boneLocals[bone.Id] * boneGlobals[bone.ParentId];
                }
            }

            boneMatrixBuffer.ResetCounter();

            for (int i = 0; i < meshes.Length; i++)
            {
                var mesh = meshes[i];
                if (mesh == null)
                    continue;
                for (int j = 0; j < mesh.Data.BoneCount; j++)
                {
                    var bone = mesh.Data.Bones[j];
                    var id = GetBoneIdByName(bone.Name);
                    boneMatrixBuffer.Add(Matrix4x4.Transpose(bone.Offset * boneGlobals[id]));
                }
            }

            boneMatrixBuffer.Update(context);
        }

        public void DrawDepth(IGraphicsContext context, IBuffer camera)
        {
            if (drawables == null) return;

            uint matrixOffset = 0;
            uint boneMatrixOffset = 0;

            context.VSSetShaderResource(matrixBuffer.SRV, 0);
            context.VSSetShaderResource(offsetBuffer.SRV, 1);
            context.VSSetShaderResource(boneMatrixBuffer.SRV, 2);
            context.VSSetShaderResource(boneOffsetBuffer.SRV, 3);

            for (int i = 0; i < drawables.Length; i++)
            {
                offsetBuffer[0] = matrixOffset;
                offsetBuffer.Update(context);

                boneOffsetBuffer[0] = boneMatrixOffset;
                boneOffsetBuffer.Update(context);

                var drawable = drawables[i];
                var mesh = meshes[i];
                var material = materials[i];

                if (mesh == null || material == null)
                    continue;

                mesh.BeginDraw(context);
                material.DrawDepth(context, camera, (uint)mesh.IndexCount, (uint)drawable.Length);

                matrixOffset += (uint)drawable.Length;
                boneMatrixOffset += mesh.Data.BoneCount;
            }
        }

        public void DrawShadows(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            if (drawables == null) return;

            uint matrixOffset = 0;
            uint boneMatrixOffset = 0;

            context.VSSetShaderResource(matrixBuffer.SRV, 0);
            context.VSSetShaderResource(offsetBuffer.SRV, 1);
            context.VSSetShaderResource(boneMatrixBuffer.SRV, 2);
            context.VSSetShaderResource(boneOffsetBuffer.SRV, 3);

            for (int i = 0; i < drawables.Length; i++)
            {
                offsetBuffer[0] = matrixOffset;
                offsetBuffer.Update(context);

                boneOffsetBuffer[0] = boneMatrixOffset;
                boneOffsetBuffer.Update(context);

                var drawable = drawables[i];
                var mesh = meshes[i];
                var material = materials[i];

                if (mesh == null || material == null)
                    continue;

                mesh.BeginDraw(context);
                material.DrawShadow(context, light, type, (uint)mesh.IndexCount, (uint)drawable.Length);

                matrixOffset += (uint)drawable.Length;
                boneMatrixOffset += mesh.Data.BoneCount;
            }
        }

        public void VisibilityTest(IGraphicsContext context)
        {
        }

        public void Draw(IGraphicsContext context)
        {
            if (drawables == null) return;

            uint matrixOffset = 0;
            uint boneMatrixOffset = 0;

            context.VSSetShaderResource(matrixBuffer.SRV, 0);
            context.VSSetShaderResource(offsetBuffer.SRV, 1);
            context.VSSetShaderResource(boneMatrixBuffer.SRV, 2);
            context.VSSetShaderResource(boneOffsetBuffer.SRV, 3);

            for (int i = 0; i < drawables.Length; i++)
            {
                offsetBuffer[0] = matrixOffset;
                offsetBuffer.Update(context);

                boneOffsetBuffer[0] = boneMatrixOffset;
                boneOffsetBuffer.Update(context);

                var drawable = drawables[i];
                var mesh = meshes[i];
                var material = materials[i];

                if (mesh == null || material == null)
                    continue;

                mesh.BeginDraw(context);
                material.Draw(context, camera.Value, (uint)mesh.IndexCount, (uint)drawable.Length);

                matrixOffset += (uint)drawable.Length;
                boneMatrixOffset += mesh.Data.BoneCount;
            }
        }

        private void UpdateModel()
        {
            Task.Factory.StartNew(async state =>
            {
                if (state is not SkinnedMeshRenderer component)
                {
                    return;
                }

                if (component.gameObject == null)
                {
                    return;
                }

                if (component.modelManager == null)
                {
                    return;
                }

                component.nodes = null;

                var tmpMeshes = component.meshes;
                component.meshes = null;
                var tmpMaterials = component.materials;
                component.materials = null;
                component.plainNodes = null;
                component.drawables = null;

                if (tmpMeshes != null && tmpMaterials != null)
                {
                    for (int i = 0; i < tmpMeshes.Length; i++)
                    {
                        ResourceManager.UnloadMesh(tmpMeshes[i]);
                        ResourceManager.UnloadMaterial(tmpMaterials[i]);
                    }
                }

                var path = Paths.CurrentAssetsPath + component.model;
                if (FileSystem.Exists(path))
                {
                    Model source = component.modelManager.Load(path);
                    MaterialLibrary library = component.materialManager.Load(Paths.CurrentMaterialsPath + source.MaterialLibrary + ".matlib");

                    int nodeCount = 0;
                    source.Root.CountNodes(ref nodeCount);
                    component.nodes = new Node[nodeCount];
                    int index = 0;
                    source.Root.FillNodes(component.nodes, ref index);

                    component.globals = new Matrix4x4[nodeCount];
                    component.locals = new Matrix4x4[nodeCount];
                    index = 0;
                    source.Root.FillTransforms(component.locals, ref index);

                    component.plainNodes = new PlainNode[nodeCount];
                    index = 0;
                    source.Root.TraverseTree(component.plainNodes, ref index);

                    int boneCount = 0;
                    source.Root.CountBones(ref boneCount);

                    component.boneLocals = new Matrix4x4[boneCount];
                    index = 0;
                    source.Root.FillBoneTransforms(component.boneLocals, ref index);

                    component.boneGlobals = new Matrix4x4[boneCount];

                    component.bones = new PlainNode[boneCount];
                    index = 0;
                    source.Root.TraverseTreeBones(component.bones, ref index);

                    component.materials = new Material[source.Header.MeshCount];
                    component.meshes = new Mesh[source.Header.MeshCount];
                    component.drawables = new int[source.Header.MeshCount][];

                    List<int> meshInstances = new();
                    for (uint i = 0; i < source.Header.MeshCount; i++)
                    {
                        for (int j = 0; j < nodeCount; j++)
                        {
                            var node = component.nodes[j];
                            if (node.Meshes.Contains(i))
                            {
                                meshInstances.Add(j);
                            }
                        }

                        component.drawables[i] = meshInstances.ToArray();
                        meshInstances.Clear();
                    }

                    for (uint i = 0; i < source.Header.MeshCount; i++)
                    {
                        var data = source.GetMesh(i);
                        Material material = await ResourceManager.LoadMaterialAsync(data, library.GetMaterial(data.MaterialName));
                        Mesh mesh = await ResourceManager.LoadMeshAsync(data);

                        component.materials[i] = material;
                        component.meshes[i] = mesh;
                        component.boundingBox = BoundingBox.CreateMerged(component.boundingBox, mesh.BoundingBox);
                    }

                    component.gameObject.SendUpdateTransformed();
                }
            }, this);
        }
    }
}