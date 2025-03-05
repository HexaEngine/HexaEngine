namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;

    /// <summary>
    /// Represents an abstract base class for 3D primitives.
    /// </summary>
    /// <typeparam name="TDescriptor">The type of the descriptor/config of primitive.</typeparam>
    /// <typeparam name="TIndex">The type of indices in the primitive.</typeparam>
    public abstract class Primitive<TDescriptor, TIndex> : DisposableRefBase, IPrimitive where TIndex : unmanaged
    {
        private readonly TDescriptor descriptor;

        /// <summary>
        /// The vertex buffer of the primitive.
        /// </summary>
        protected VertexBuffer<PrimVertex> vertexBuffer;

        /// <summary>
        /// The index buffer of the primitive.
        /// </summary>
        protected IndexBuffer<TIndex>? indexBuffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Primitive{TDescriptor, TIndex}"/> class.
        /// </summary>
        protected Primitive(TDescriptor descriptor)
        {
            this.descriptor = descriptor;
            (vertexBuffer, indexBuffer) = InitializeMesh(descriptor);
        }

        public TDescriptor Description => descriptor;

        public uint VertexCount => vertexBuffer.Count;

        public uint IndexCount => indexBuffer?.Count ?? 0;

        /// <summary>
        /// Initializes the mesh by creating vertex and index buffers.
        /// </summary>
        /// <returns>A tuple containing the vertex buffer and an optional index buffer.</returns>
        protected abstract (VertexBuffer<PrimVertex>, IndexBuffer<TIndex>?) InitializeMesh(TDescriptor descriptor);

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

        protected override void DisposeCore()
        {
            vertexBuffer?.Dispose();
            indexBuffer?.Dispose();
        }
    }
}