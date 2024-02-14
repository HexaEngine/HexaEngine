namespace HexaEngine.Meshes
{
    using HexaEngine.Components.Renderer;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Mathematics;
    using HexaEngine.Resources;
    using HexaEngine.Resources.Factories;
    using HexaEngine.Scenes.Managers;
    using System.Numerics;
    using System.Threading.Tasks;

    public class Model : IDisposable
    {
        private readonly ModelFile modelFile;
        private readonly MaterialAssetMappingCollection materialAssets;

        private readonly Node[] nodes;
        private readonly Mesh[] meshes;
        private readonly Material[] materials;
        private readonly int[][] drawables;
        private readonly Matrix4x4[] locals;
        private readonly Matrix4x4[] globals;
        private readonly PlainNode[] plainNodes;

        private BoundingBox boundingBox;

        private MaterialShaderFlags shaderFlags;

        private bool loaded;

        private bool disposedValue;

        public Model(ModelFile modelFile, MaterialAssetMappingCollection materialAssets)
        {
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

            this.modelFile = modelFile;
            this.materialAssets = materialAssets;
        }

        public ModelFile ModelFile => modelFile;

        public Node[] Nodes => nodes;

        public PlainNode[] PlainNodes => plainNodes;

        public Mesh[] Meshes => meshes;

        public Material[] Materials => materials;

        public int[][] Drawables => drawables;

        public Matrix4x4[] Locals => locals;

        public Matrix4x4[] Globals => globals;

        public BoundingBox BoundingBox => boundingBox;

        public MaterialShaderFlags ShaderFlags => shaderFlags;

        public bool Loaded => loaded;

        public void Load()
        {
            for (int i = 0; i < modelFile.Meshes.Count; i++)
            {
                var data = modelFile.GetMesh(i);
                Material material = ResourceManager.Shared.LoadMaterial(data, materialAssets.GetMaterial(data), true);
                Mesh mesh = ResourceManager.Shared.LoadMesh(data, true);

                materials[i] = material;
                meshes[i] = mesh;
                boundingBox = BoundingBox.CreateMerged(boundingBox, mesh.BoundingBox);
            }

            shaderFlags = MaterialShaderFlags.None;
            for (int i = 0; i < materials.Length; i++)
            {
                shaderFlags |= materials[i].Shader.Value.Flags;
            }

            loaded = true;
        }

        public async Task LoadAsync()
        {
            for (int i = 0; i < modelFile.Meshes.Count; i++)
            {
                var data = modelFile.GetMesh(i);
                Material material = await ResourceManager.Shared.LoadMaterialAsync(data, materialAssets.GetMaterial(data), true);
                Mesh mesh = await ResourceManager.Shared.LoadMeshAsync(data, true);

                materials[i] = material;
                meshes[i] = mesh;
                boundingBox = BoundingBox.CreateMerged(boundingBox, mesh.BoundingBox);
            }

            shaderFlags = MaterialShaderFlags.None;
            for (int i = 0; i < materials.Length; i++)
            {
                shaderFlags |= materials[i].Shader.Value.Flags;
            }

            loaded = true;
        }

        public void Unload()
        {
            shaderFlags = MaterialShaderFlags.None;
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Unload();
                disposedValue = true;
            }
        }

        ~Model()
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