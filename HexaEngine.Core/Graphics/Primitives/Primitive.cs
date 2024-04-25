namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using System;

    /// <summary>
    /// Represents an abstract base class for 3D primitives.
    /// </summary>
    /// <typeparam name="Tvertex">The type of vertices in the primitive.</typeparam>
    /// <typeparam name="Tindex">The type of indices in the primitive.</typeparam>
    public abstract class Primitive<Tvertex, Tindex> : IPrimitive where Tvertex : unmanaged where Tindex : unmanaged
    {
        /// <summary>
        /// The vertex buffer of the primitive.
        /// </summary>
        protected VertexBuffer<Tvertex> vertexBuffer;

        /// <summary>
        /// The index buffer of the primitive.
        /// </summary>
        protected IndexBuffer<Tindex>? indexBuffer;

        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Primitive{Tvertex, Tindex}"/> class.
        /// </summary>
        public Primitive()
        {
            (vertexBuffer, indexBuffer) = InitializeMesh();
        }

        /// <summary>
        /// Initializes the mesh by creating vertex and index buffers.
        /// </summary>
        /// <returns>A tuple containing the vertex buffer and an optional index buffer.</returns>
        protected abstract (VertexBuffer<Tvertex>, IndexBuffer<Tindex>?) InitializeMesh();

        /// <inheritdoc/>
        public void DrawAuto(IGraphicsContext context, IGraphicsPipelineState pipeline)
        {
            context.SetPipelineState(pipeline);
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
            context.SetPipelineState(null);
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
        }

        /// <summary>
        /// Draws the primitive with instancing support.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        /// <param name="instanceCount">The number of instances to draw.</param>
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

        /// <inheritdoc/>
        public void BeginDraw(IGraphicsContext context, out uint vertexCount, out uint indexCount, out int instanceCount)
        {
            context.SetVertexBuffer(vertexBuffer, vertexBuffer.Stride);
            vertexCount = vertexBuffer.Count;
            indexBuffer?.Bind(context);
            indexCount = indexBuffer?.Count ?? 0;
            instanceCount = 1;
        }

        /// <inheritdoc/>
        public void EndDraw(IGraphicsContext context)
        {
            context.SetVertexBuffer(null, 0);
            context.SetIndexBuffer(null, 0, 0);
        }

        /// <summary>
        /// Cleans up resources used by the primitive.
        /// </summary>
        /// <param name="disposing">True if called from Dispose; false if called from the finalizer.</param>
        protected virtual void Dispose(bool disposing)
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