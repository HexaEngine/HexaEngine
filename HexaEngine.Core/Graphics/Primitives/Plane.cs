namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Mathematics;

    public class Plane : IPrimitive
    {
#nullable disable
        private VertexBuffer<OrthoVertex> vertexBuffer;
        private IndexBuffer<ushort> indexBuffer;
        private bool disposedValue;
        private readonly int size;
#nullable enable

        public Plane(IGraphicsDevice device, int size)
        {
            this.size = size;
            InitializeMesh(device);
        }

        protected void InitializeMesh(IGraphicsDevice device)
        {
            vertexBuffer = new(device, new OrthoVertex[]
            {
                    new OrthoVertex(new(-1 * size, 1 * size, 0), new(0, 0)),
                    new OrthoVertex(new(-1 * size, -1 * size, 0), new(0, 1)),
                    new OrthoVertex(new(1 * size, 1 * size, 0), new(1, 0)),
                    new OrthoVertex(new(1 * size, -1 * size, 0), new(1, 1))
            }, CpuAccessFlags.None);
            indexBuffer = new(device, new ushort[] { 0, 3, 1, 0, 2, 3 }, CpuAccessFlags.None);
        }

        public void DrawAuto(IGraphicsContext context, IGraphicsPipeline pipeline)
        {
            context.SetVertexBuffer(vertexBuffer, vertexBuffer.Stride);
            if (indexBuffer != null)
            {
                indexBuffer.Bind(context);
                context.SetGraphicsPipeline(pipeline);
                context.DrawIndexedInstanced(indexBuffer.Count, 1, 0, 0, 0);
            }
            else
            {
                context.SetGraphicsPipeline(pipeline);
                context.DrawInstanced(vertexBuffer.Count, 1, 0, 0);
            }
        }

        public void Bind(IGraphicsContext context, out uint vertexCount, out uint indexCount, out int instanceCount)
        {
            context.SetVertexBuffer(vertexBuffer, vertexBuffer.Stride);
            vertexCount = vertexBuffer.Count;
            context.SetIndexBuffer(indexBuffer, Format.R32UInt, 0);
            indexCount = indexBuffer?.Count ?? 0;
            instanceCount = 1;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                vertexBuffer?.Dispose();
                indexBuffer?.Dispose();
                disposedValue = true;
            }
        }

        ~Plane()
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