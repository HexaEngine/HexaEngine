namespace HexaEngine.Core.Graphics.Buffers
{
    /// <summary>
    /// Represents an index buffer in graphics programming.
    /// </summary>
    public interface IIndexBuffer : IBuffer
    {
        /// <summary>
        /// Gets the number of indices in the buffer.
        /// </summary>
        uint Count { get; }

        /// <summary>
        /// Gets the index buffer format.
        /// </summary>
        IndexFormat Format { get; }

        /// <summary>
        /// Binds the index buffer to the graphics context.
        /// </summary>
        /// <param name="context">The graphics context to which the buffer will be bound.</param>
        void Bind(IGraphicsContext context);

        /// <summary>
        /// Binds the index buffer to the graphics context with an offset.
        /// </summary>
        /// <param name="context">The graphics context to which the buffer will be bound.</param>
        /// <param name="offset">The offset within the buffer.</param>
        void Bind(IGraphicsContext context, int offset);

        /// <summary>
        /// Copies the content of the index buffer to another buffer using the graphics context.
        /// </summary>
        /// <param name="context">The graphics context used for the copy operation.</param>
        /// <param name="buffer">The destination buffer.</param>
        void CopyTo(IGraphicsContext context, IBuffer buffer);

        /// <summary>
        /// Unbinds the index buffer from the graphics context.
        /// </summary>
        /// <param name="context">The graphics context from which the buffer will be unbound.</param>
        void Unbind(IGraphicsContext context);

        /// <summary>
        /// Updates the index buffer using the graphics context.
        /// </summary>
        /// <param name="context">The graphics context used for the update.</param>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <returns><c>true</c> if the buffer has been updated; otherwise, <c>false</c>.</returns>
        unsafe void Update(IGraphicsContext context, void* data, uint size);
    }

    /// <summary>
    /// Represents a typed index buffer in graphics programming.
    /// </summary>
    /// <typeparam name="T">The type of indices in the buffer (must be unmanaged).</typeparam>
    public interface IIndexBuffer<T> : IIndexBuffer where T : unmanaged
    {
        unsafe void Resize(T* items, uint capacity);

        unsafe void Update(IGraphicsContext context, T* items, uint size);
    }
}