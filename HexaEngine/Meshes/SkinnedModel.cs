namespace HexaEngine.Meshes
{
    using HexaEngine.Components.Renderer;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Core.IO.Binary.Meshes;
    using HexaEngine.Mathematics;
    using HexaEngine.Resources;
    using HexaEngine.Resources.Factories;
    using HexaEngine.Scenes.Managers;
    using System.Numerics;
    using System.Threading.Tasks;

    public class SkinnedModel : IDisposable
    {
        private readonly Stream stream;
        private readonly ModelFile modelFile;
        private readonly MaterialAssetMappingCollection materialAssets;

        private Node[] nodes;
        private Mesh[] meshes;
        private Material[] materials;
        private int[][] drawables;
        private Matrix4x4[] locals;
        private Matrix4x4[] globals;
        private Matrix4x4[] boneLocals;
        private Matrix4x4[] boneGlobals;
        private PlainNode[] plainNodes;
        private PlainNode[] bones;

        private ModelMaterialShaderFlags shaderFlags;

        private BoundingBox boundingBox;

        private bool loaded;
        private bool disposedValue;

        public SkinnedModel(Stream stream, MaterialAssetMappingCollection materialAssets)
        {
            this.stream = stream;
            this.materialAssets = materialAssets;
            modelFile = ModelFile.Load(stream, MeshLoadMode.Streaming);
            materialAssets.Update(modelFile);

            int nodeCount = 0;
            modelFile.Root.CountNodes(ref nodeCount);
            nodes = new Node[nodeCount];
            int index = 0;
            modelFile.Root.FillNodes(nodes, ref index);

            globals = new Matrix4x4[nodeCount];
            locals = new Matrix4x4[nodeCount];
            index = 0;
            modelFile.Root.FillTransforms(locals, ref index);

            plainNodes = new PlainNode[nodeCount];
            index = 0;
            modelFile.Root.TraverseTree(plainNodes, ref index);

            int boneCount = 0;
            modelFile.Root.CountBones(ref boneCount);

            boneLocals = new Matrix4x4[boneCount];
            index = 0;
            modelFile.Root.FillBoneTransforms(boneLocals, ref index);

            boneGlobals = new Matrix4x4[boneCount];

            bones = new PlainNode[boneCount];
            index = 0;
            modelFile.Root.TraverseTreeBones(bones, ref index);

            materials = new Material[modelFile.Header.MeshCount];
            meshes = new Mesh[modelFile.Header.MeshCount];
            drawables = new int[modelFile.Header.MeshCount][];

            List<int> meshInstances = new();
            for (uint i = 0; i < modelFile.Header.MeshCount; i++)
            {
                for (int j = 0; j < nodeCount; j++)
                {
                    var node = nodes[j];
                    if (node.Meshes.Contains(i))
                    {
                        meshInstances.Add(j);
                    }
                }

                drawables[i] = meshInstances.ToArray();
                meshInstances.Clear();
            }
        }

        public ModelFile ModelFile => modelFile;

        public Node[] Nodes => nodes;

        public PlainNode[] PlainNodes => plainNodes;

        public int[][] Drawables => drawables;

        public Mesh[] Meshes => meshes;

        public Material[] Materials => materials;

        public Matrix4x4[] Locals => locals;

        public Matrix4x4[] Globals => globals;

        public Matrix4x4[] BoneLocals => boneLocals;

        public Matrix4x4[] BoneGlobals => boneGlobals;

        public PlainNode[] Bones => bones;

        public ModelMaterialShaderFlags ShaderFlags => shaderFlags;

        public BoundingBox BoundingBox => boundingBox;

        public void Load()
        {
            for (int i = 0; i < modelFile.Meshes.Count; i++)
            {
                MeshData meshData = modelFile.GetMesh(i);
                MeshLODData lod = meshData.LoadLODData(0, stream);
                Mesh mesh = ResourceManager.Shared.LoadMesh(meshData, lod);
                MaterialData materialDesc = materialAssets.GetMaterial(meshData.Name);
                MaterialShaderDesc shaderDesc = Model.GetMaterialShaderDesc(mesh, materialDesc, false, out var flags);
                Material material = ResourceManager.Shared.LoadMaterial<SkinnedModel>(shaderDesc, materialDesc);

                materials[i] = material;
                meshes[i] = mesh;
                boundingBox = BoundingBox.CreateMerged(boundingBox, mesh.BoundingBox);
                shaderFlags |= flags;
            }

            loaded = true;
        }

        public async Task LoadAsync()
        {
            for (int i = 0; i < modelFile.Meshes.Count; i++)
            {
                MeshData meshData = modelFile.GetMesh(i);
                MeshLODData lod = meshData.LoadLODData(0, stream);
                Mesh mesh = await ResourceManager.Shared.LoadMeshAsync(meshData, lod);
                MaterialData materialDesc = materialAssets.GetMaterial(meshData.Name);
                MaterialShaderDesc shaderDesc = Model.GetMaterialShaderDesc(mesh, materialDesc, false, out var flags);
                Material material = await ResourceManager.Shared.LoadMaterialAsync<SkinnedModel>(shaderDesc, materialDesc);

                materials[i] = material;
                meshes[i] = mesh;
                boundingBox = BoundingBox.CreateMerged(boundingBox, mesh.BoundingBox);
                shaderFlags |= flags;
            }

            loaded = true;
        }

        public void Unload()
        {
            shaderFlags = ModelMaterialShaderFlags.None;
            for (int i = 0; i < meshes.Length; i++)
            {
                meshes[i].Dispose();
                materials[i].Dispose();
            }

            loaded = false;
        }

        public void SetLocal(Matrix4x4 local, uint nodeId)
        {
            if (locals != null)
                locals[nodeId] = local;
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Unload();
                disposedValue = true;
            }
        }

        ~SkinnedModel()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}