namespace HexaEngine.Objects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
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
            if (vertexBuffer.GetVertices() is Vertex[] v)
            {
                BoundingBox = BoundingBoxHelper.Compute(v);
            }
        }

        protected abstract (VertexBuffer<T>, IndexBuffer?, InstanceBuffer?) InitializeMesh(IGraphicsDevice device);

        public BoundingBox BoundingBox;

        public void DrawAuto(IGraphicsContext context, GraphicsPipeline pipeline, Viewport viewport)
        {
            vertexBuffer.Bind(context);
            if (indexBuffer != null)
            {
                indexBuffer.Bind(context);
                if (instanceBuffer != null)
                {
                    instanceBuffer.Bind(context, 1);
                    pipeline.DrawIndexedInstanced(context, viewport, indexBuffer.Count, instanceBuffer.Count, 0, 0, 0);
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
                    pipeline.DrawInstanced(context, viewport, vertexBuffer.Count, instanceBuffer.Count, 0, 0);
                }
                else
                {
                    pipeline.DrawInstanced(context, viewport, vertexBuffer.Count, 1, 0, 0);
                }
            }
        }

        public void Bind(IGraphicsContext context, out int vertexCount, out int indexCount, out int instanceCount)
        {
            vertexBuffer.Bind(context);
            vertexCount = vertexBuffer.Count;
            indexBuffer?.Bind(context);
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