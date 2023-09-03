﻿namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using System;

    public abstract class Primitive<Tvertex, Tindex> : IPrimitive where Tvertex : unmanaged where Tindex : unmanaged
    {
        protected VertexBuffer<Tvertex> vertexBuffer;
        protected IndexBuffer<Tindex>? indexBuffer;
        private bool disposedValue;

        public Primitive(IGraphicsDevice device)
        {
            (vertexBuffer, indexBuffer) = InitializeMesh(device);
        }

        protected abstract (VertexBuffer<Tvertex>, IndexBuffer<Tindex>?) InitializeMesh(IGraphicsDevice device);

        public void DrawAuto(IGraphicsContext context, IGraphicsPipeline pipeline)
        {
            context.SetGraphicsPipeline(pipeline);
            context.SetVertexBuffer(vertexBuffer, vertexBuffer.Stride);
            if (indexBuffer != null)
            {
                indexBuffer.Bind(context);
                context.DrawIndexedInstanced(indexBuffer.Count, 1, 0, 0, 0);
                context.SetIndexBuffer(null, 0, 0);
            }
            else
            {
                context.DrawInstanced(vertexBuffer.Count, 1, 0, 0);
            }
            context.SetVertexBuffer(null, 0);
            context.SetGraphicsPipeline(null);
        }

        public void DrawAuto(IGraphicsContext context)
        {
            context.SetVertexBuffer(vertexBuffer, vertexBuffer.Stride);
            if (indexBuffer != null)
            {
                indexBuffer.Bind(context);
                context.DrawIndexedInstanced(indexBuffer.Count, 1, 0, 0, 0);
                context.SetIndexBuffer(null, 0, 0);
            }
            else
            {
                context.DrawInstanced(vertexBuffer.Count, 1, 0, 0);
            }
            context.SetVertexBuffer(null, 0);
        }

        public void DrawAutoInstanced(IGraphicsContext context, uint instanceCount)
        {
            context.SetVertexBuffer(vertexBuffer, vertexBuffer.Stride);
            if (indexBuffer != null)
            {
                indexBuffer.Bind(context);
                context.DrawIndexedInstanced(indexBuffer.Count, instanceCount, 0, 0, 0);
                context.SetIndexBuffer(null, 0, 0);
            }
            else
            {
                context.DrawInstanced(vertexBuffer.Count, instanceCount, 0, 0);
            }
            context.SetVertexBuffer(null, 0);
        }

        public void Bind(IGraphicsContext context, out uint vertexCount, out uint indexCount, out int instanceCount)
        {
            context.SetVertexBuffer(vertexBuffer, vertexBuffer.Stride);
            vertexCount = vertexBuffer.Count;
            indexBuffer?.Bind(context);
            indexCount = indexBuffer?.Count ?? 0;
            instanceCount = 1;
        }

        public void Unbind(IGraphicsContext context)
        {
            context.SetVertexBuffer(null, 0);
            context.SetIndexBuffer(null, 0, 0);
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