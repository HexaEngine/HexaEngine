namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Graphics.Batching;
    using HexaEngine.Resources;
    using System.Numerics;

    public class MeshBatchInstance : IBatchInstance
    {
        public Mesh Mesh { get; }

        public Material Material { get; }

        public Matrix4x4 Transform;
        public bool Visible;
        public uint BufferOffset;

        public MeshBatchInstance(Mesh mesh, Material material)
        {
            Mesh = mesh;
            Material = material;
        }

        public bool CanInstantiate(IBatch batch, IBatchInstance other)
        {
            return other is MeshBatchInstance instance && instance.Mesh == Mesh && instance.Material == Material;
        }

        public bool CanMerge(IBatch batch, IBatchInstance other)
        {
            if (batch.Count >= 8)
                return false;
            return false;
        }

        public IBatchInstance Merge(IBatch batch, IBatchInstance other)
        {
            throw new NotSupportedException();
        }
    }
}