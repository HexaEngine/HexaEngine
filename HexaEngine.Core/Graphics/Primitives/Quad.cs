﻿namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO;
    using System.Numerics;

    /// <summary>
    /// Represents a simple quad in 3D space.
    /// </summary>
    public sealed class Quad : IPrimitive
    {
        private readonly VertexBuffer<MeshVertex> vertexBuffer;
        private readonly IndexBuffer<ushort> indexBuffer;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Quad"/> class.
        /// </summary>
        /// <param name="size">The size of the plane.</param>
        public Quad(float size)
        {
            CreateQuad(out vertexBuffer, out indexBuffer, size);
        }

        /// <summary>
        /// Creates a plane mesh.
        /// </summary>
        /// <param name="vertexBuffer">The vertex buffer.</param>
        /// <param name="indexBuffer">The index buffer.</param>
        /// <param name="size">The size of the plane.</param>
        public static void CreateQuad(out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer<ushort> indexBuffer, float size = 1)
        {
            vertexBuffer = new(
            [
                new(new(-1 * size, 1 * size, 0), new Vector2(0, 0), new(0, 0, -1), new(1, 0, 0)),
                new(new(-1 * size, -1 * size, 0), new Vector2(0, 1), new(0, 0, -1), new(1, 0, 0)),
                new(new(1 * size, 1 * size, 0), new Vector2(1, 0), new(0, 0, -1), new(1, 0, 0)),
                new(new(1 * size, -1 * size, 0), new Vector2(1, 1), new(0, 0, -1), new(1, 0, 0))
            ], CpuAccessFlags.None);

            indexBuffer = new(
            [
                0,
                3,
                1,
                0,
                2,
                3
            ], CpuAccessFlags.None);
        }

        /// <inheritdoc/>
        public void DrawAuto(IGraphicsContext context, IGraphicsPipelineState pipeline)
        {
            context.SetGraphicsPipelineState(pipeline);
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
            context.SetGraphicsPipelineState(null);
        }

        /// <inheritdoc/>
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
            context.SetGraphicsPipelineState(null);
        }

        /// <inheritdoc/>
        public void BeginDraw(IGraphicsContext context, out uint vertexCount, out uint indexCount, out int instanceCount)
        {
            context.SetVertexBuffer(vertexBuffer, vertexBuffer.Stride);
            vertexCount = vertexBuffer.Count;
            indexBuffer.Bind(context);
            indexCount = indexBuffer?.Count ?? 0;
            instanceCount = 1;
        }

        /// <inheritdoc/>
        public void EndDraw(IGraphicsContext context)
        {
            context.SetVertexBuffer(null, 0);
            context.SetIndexBuffer(null, 0, 0);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                vertexBuffer?.Dispose();
                indexBuffer?.Dispose();
                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}