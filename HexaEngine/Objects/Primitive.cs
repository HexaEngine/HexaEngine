namespace HexaEngine.Objects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Buffers;
    using HexaEngine.Mathematics;
    using System;

    public abstract class Primitive<T> : IPrimitive where T : unmanaged
    {
        protected VertexBuffer<T> vertexBuffer;
        protected IndexBuffer? indexBuffer;
        private bool disposedValue;

        public Primitive(IGraphicsDevice device)
        {
            (vertexBuffer, indexBuffer) = InitializeMesh(device);
        }

        protected abstract (VertexBuffer<T>, IndexBuffer?) InitializeMesh(IGraphicsDevice device);

        public void DrawAuto(IGraphicsContext context, GraphicsPipeline pipeline, Viewport viewport)
        {
            context.SetVertexBuffer(vertexBuffer, vertexBuffer.Stride);
            if (indexBuffer != null)
            {
                context.SetIndexBuffer(indexBuffer, Format.R32UInt, 0);

                pipeline.DrawIndexedInstanced(context, viewport, indexBuffer.Count, 1, 0, 0, 0);
            }
            else
            {
                pipeline.DrawInstanced(context, viewport, (uint)vertexBuffer.Count, 1, 0, 0);
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

        ~Primitive()
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