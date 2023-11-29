namespace HexaEngine.Editor.MeshEditor.Rendering
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Mathematics;

    public unsafe class MeshEditorMesh : IDisposable
    {
        public MeshData Data;
        public readonly VertexBuffer<MeshVertex> VertexBuffer;
        public readonly IndexBuffer<uint> IndexBuffer;
        public readonly uint Stride;
        public readonly uint VertexCount;
        public readonly uint IndexCount;
        public readonly BoundingBox BoundingBox;
        public readonly BoundingSphere BoundingSphere;
        private bool disposedValue;

        public MeshEditorMesh(IGraphicsDevice device, MeshData data)
        {
            Data = data;
            VertexBuffer = data.CreateVertexBuffer(device, CpuAccessFlags.Write);
            IndexBuffer = data.CreateIndexBuffer(device, CpuAccessFlags.Write);
            Stride = (uint)sizeof(MeshVertex);

            VertexCount = data.VerticesCount;
            IndexCount = data.IndicesCount;

            BoundingBox = data.Box;
            BoundingSphere = data.Sphere;
        }

        public void Update(IGraphicsContext context, bool ib, bool vb)
        {
            if (ib)
            {
                Data.WriteIndexBuffer(context, IndexBuffer);
            }

            if (vb)
            {
                Data.WriteVertexBuffer(context, VertexBuffer);
            }
        }

        public void Draw(IGraphicsContext context, uint instanceCount)
        {
            context.SetIndexBuffer(IndexBuffer, Format.R32UInt, 0);
            context.SetVertexBuffer(VertexBuffer, Stride);
            context.DrawIndexedInstanced(IndexCount, instanceCount, 0, 0, 0);
            context.SetVertexBuffer(null, 0);
            context.SetIndexBuffer(null, Format.Unknown, 0);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                VertexBuffer.Dispose();
                IndexBuffer.Dispose();
                disposedValue = true;
            }
        }

        ~MeshEditorMesh()
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