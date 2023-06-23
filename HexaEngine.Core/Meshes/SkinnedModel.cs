﻿namespace HexaEngine.Core.Meshes
{
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Resources;
    using HexaEngine.Mathematics;
    using System.Numerics;
    using System.Threading.Tasks;

    public class SkinnedModel : IDisposable
    {
        private readonly ModelFile modelFile;
        private readonly MaterialLibrary materialLibrary;

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

        private BoundingBox boundingBox;

        private bool loaded;
        private bool disposedValue;

        public SkinnedModel(ModelFile modelFile, MaterialLibrary materialLibrary)
        {
            this.modelFile = modelFile;
            this.materialLibrary = materialLibrary;

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

        public MaterialLibrary MaterialLibrary => materialLibrary;

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

        public BoundingBox BoundingBox => boundingBox;

        public void Load()
        {
            for (uint i = 0; i < modelFile.Header.MeshCount; i++)
            {
                var data = modelFile.GetMesh(i);
                Material material = ResourceManager.LoadMaterial(data, materialLibrary.GetMaterial(data.MaterialName));
                Mesh mesh = ResourceManager.LoadMesh(data, false);

                materials[i] = material;
                meshes[i] = mesh;
                boundingBox = BoundingBox.CreateMerged(boundingBox, mesh.BoundingBox);
            }
            loaded = true;
        }

        public async Task LoadAsync()
        {
            for (uint i = 0; i < modelFile.Header.MeshCount; i++)
            {
                var data = modelFile.GetMesh(i);
                Material material = await ResourceManager.LoadMaterialAsync(data, materialLibrary.GetMaterial(data.MaterialName));
                Mesh mesh = await ResourceManager.LoadMeshAsync(data);

                materials[i] = material;
                meshes[i] = mesh;
                boundingBox = BoundingBox.CreateMerged(boundingBox, mesh.BoundingBox);
            }
            loaded = true;
        }

        public void Unload()
        {
            for (int i = 0; i < meshes.Length; i++)
            {
                ResourceManager.UnloadMesh(meshes[i]);
                ResourceManager.UnloadMaterial(materials[i]);
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