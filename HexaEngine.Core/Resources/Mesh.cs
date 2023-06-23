namespace HexaEngine.Core.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Mathematics;

    public class Mesh : ResourceInstance
    {
        private readonly string name;
        private bool disposedValue;
        public readonly MeshData Data;
        public IBuffer? VertexBuffer;
        public IBuffer? IndexBuffer;
        public uint VertexCount;
        public uint IndexCount;
        public BoundingBox BoundingBox;
        public BoundingSphere BoundingSphere;
        public uint Stride;

        public unsafe Mesh(IGraphicsDevice device, MeshData data, bool debone = true) : base(data.Name, 1)
        {
            name = data.Name;
            Data = data;
            BoundingBox = data.Box;
            BoundingSphere = data.Sphere;
            IndexBuffer = data.CreateIndexBuffer(device);
            if (!debone && (data.Flags & VertexFlags.Skinned) != 0)
            {
                VertexBuffer = data.CreateSkinnedVertexBuffer(device);
                Stride = (uint)sizeof(SkinnedMeshVertex);
            }
            else
            {
                VertexBuffer = data.CreateVertexBuffer(device);
                Stride = (uint)sizeof(MeshVertex);
            }
            IndexCount = data.IndicesCount;
            VertexCount = data.VerticesCount;
        }

        public void BeginDraw(IGraphicsContext context)
        {
            context.SetIndexBuffer(IndexBuffer, Format.R32UInt, 0);
            context.SetVertexBuffer(VertexBuffer, Stride);
        }

        public void EndDraw(IGraphicsContext context)
        {
            context.SetIndexBuffer(null, Format.Unknown, 0);
            context.SetVertexBuffer(null, 0);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                VertexBuffer?.Dispose();
                IndexBuffer?.Dispose();

                disposedValue = true;
            }
        }

        public override string ToString()
        {
            return name;
        }
    }
}