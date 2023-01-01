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
        protected InstanceBuffer? instanceBuffer;
        private bool disposedValue;

        public Primitive(IGraphicsDevice device)
        {
            (vertexBuffer, indexBuffer, instanceBuffer) = InitializeMesh(device);
        }

        protected abstract (VertexBuffer<T>, IndexBuffer?, InstanceBuffer?) InitializeMesh(IGraphicsDevice device);

        public void DrawAuto(IGraphicsContext context, GraphicsPipeline pipeline, Viewport viewport)
        {
            context.SetVertexBuffer(vertexBuffer, vertexBuffer.Stride);
            if (indexBuffer != null)
            {
                context.SetIndexBuffer(indexBuffer, Format.R32UInt, 0);
                if (instanceBuffer != null)
                {
                    instanceBuffer.Bind(context, 1);
                    pipeline.DrawIndexedInstanced(context, viewport, indexBuffer.Count, (uint)instanceBuffer.Count, 0, 0, 0);
                }
                else
                {
                    pipeline.DrawIndexedInstanced(context, viewport, indexBuffer.Count, 1, 0, 0, 0);
                }
            }
            else
            {
                if (instanceBuffer != null)
                {
                    instanceBuffer.Bind(context, 1);
                    pipeline.DrawInstanced(context, viewport, (uint)vertexBuffer.Count, (uint)instanceBuffer.Count, 0, 0);
                }
                else
                {
                    pipeline.DrawInstanced(context, viewport, (uint)vertexBuffer.Count, 1, 0, 0);
                }
            }
        }

        public void Bind(IGraphicsContext context, out uint vertexCount, out uint indexCount, out int instanceCount)
        {
            context.SetVertexBuffer(vertexBuffer, vertexBuffer.Stride);
            vertexCount = vertexBuffer.Count;
            context.SetIndexBuffer(indexBuffer, Format.R32UInt, 0);
            indexCount = indexBuffer?.Count ?? 0;
            instanceBuffer?.Bind(context, 1);
            instanceCount = instanceBuffer?.Count ?? 0;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                vertexBuffer?.Dispose();
                indexBuffer?.Dispose();
                instanceBuffer?.Dispose();
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