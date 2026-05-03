namespace HexaEngine.Core.Graphics.Buffers
{
    /// <summary>
    /// Represents a vertex buffer in graphics programming.
    /// </summary>
    public interface IVertexBuffer : IBuffer
    {
        /// <summary>
        /// Gets or sets the capacity of the vertex buffer.
        /// </summary>
        uint Count { get; }

        /// <summary>
        /// Gets the stride (size of each vertex in bytes) of the vertex buffer.
        /// </summary>
        uint Stride { get; }

        /// <summary>
        /// Binds the vertex buffer to the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context to bind the vertex buffer to.</param>
        void Bind(IGraphicsContext context);

        /// <summary>
        /// Unbinds the vertex buffer from the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context to unbind the vertex buffer from.</param>
        void Unbind(IGraphicsContext context);

        /// <summary>
        /// Copies the contents of the vertex buffer to the specified buffer in the graphics context.
        /// </summary>
        /// <param name="context">The graphics context to use for copying the data.</param>
        /// <param name="buffer">The destination buffer.</param>
        void CopyTo(IGraphicsContext context, IBuffer buffer);

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="data"></param>
        /// <param name="size"></param>
        unsafe void Update(IGraphicsContext context, void* data, uint size);

        unsafe void Resize(void* items, uint newCapacity);
    }

    /// <summary>
    /// Represents a generic vertex buffer in graphics programming.
    /// </summary>
    /// <typeparam name="T">The type of vertices in the buffer.</typeparam>
    public interface IVertexBuffer<T> : IVertexBuffer where T : unmanaged
    {
        unsafe void Resize(T* items, uint newCapacity);

        /// <summary>
        /// Updates the vertex buffer in the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context to use for updating the vertex buffer.</param>
        /// <param name="items"></param>
        /// <param name="count"></param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        unsafe void Update(IGraphicsContext context, T* items, uint count);
    }
}