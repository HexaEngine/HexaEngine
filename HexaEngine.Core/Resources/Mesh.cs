namespace HexaEngine.Core.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public class Mesh : ResourceInstance
    {
        private readonly string name;
        private bool disposedValue;
        public readonly MeshData Data;
        public IBuffer? VertexBuffer;
        public IBuffer? IndexBuffer;
        public int VertexCount;
        public int IndexCount;
        public BoundingBox BoundingBox;
        public BoundingSphere BoundingSphere;
        public uint Stride;

        public unsafe Mesh(IGraphicsDevice device, MeshData data) : base(data.Name, 1)
        {
            name = data.Name;
            Data = data;
            BoundingBox = data.Box;
            BoundingSphere = data.Sphere;
            IndexBuffer = data.CreateIndexBuffer(device);
            VertexBuffer = data.CreateVertexBuffer(device);
            IndexCount = (int)data.IndicesCount;
            VertexCount = (int)data.VerticesCount;
            Stride = data.GetStride();
        }

        public void BeginDraw(IGraphicsContext context)
        {
            context.SetIndexBuffer(IndexBuffer, Format.R32UInt, 0);
            context.SetVertexBuffer(VertexBuffer, Stride);
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