namespace HexaEngine.Core.Graphics.Buffers
{
    /// <summary>
    /// Represents an index buffer in graphics programming.
    /// </summary>
    public interface IIndexBuffer : IBuffer
    {
        /// <summary>
        /// Gets or sets the capacity of the index buffer.
        /// </summary>
        uint Capacity { get; set; }

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
        /// Clears the index buffer.
        /// </summary>
        void Clear();

        /// <summary>
        /// Copies the content of the index buffer to another buffer using the graphics context.
        /// </summary>
        /// <param name="context">The graphics context used for the copy operation.</param>
        /// <param name="buffer">The destination buffer.</param>
        void CopyTo(IGraphicsContext context, IBuffer buffer);

        /// <summary>
        /// Ensures that the index buffer has the specified capacity.
        /// </summary>
        /// <param name="capacity">The desired capacity of the index buffer.</param>
        void EnsureCapacity(uint capacity);

        /// <summary>
        /// Flushes the memory of the index buffer.
        /// </summary>
        void FlushMemory();

        /// <summary>
        /// Removes the index at the specified index from the buffer.
        /// </summary>
        /// <param name="index">The index to remove.</param>
        void RemoveAt(int index);

        /// <summary>
        /// Resets the counter of the index buffer.
        /// </summary>
        void ResetCounter();

        /// <summary>
        /// Unbinds the index buffer from the graphics context.
        /// </summary>
        /// <param name="context">The graphics context from which the buffer will be unbound.</param>
        void Unbind(IGraphicsContext context);

        /// <summary>
        /// Updates the index buffer using the graphics context.
        /// </summary>
        /// <param name="context">The graphics context used for the update.</param>
        /// <returns><c>true</c> if the buffer has been updated; otherwise, <c>false</c>.</returns>
        bool Update(IGraphicsContext context);
    }

    /// <summary>
    /// Represents a typed index buffer in graphics programming.
    /// </summary>
    /// <typeparam name="T">The type of indices in the buffer (must be unmanaged).</typeparam>
    public interface IIndexBuffer<T> : IIndexBuffer where T : unmanaged
    {
        /// <summary>
        /// Gets or sets the index at the specified index in the buffer.
        /// </summary>
        /// <param name="index">The index of the element to get or set.</param>
        T this[uint index] { get; set; }

        /// <summary>
        /// Adds multiple indices to the buffer.
        /// </summary>
        /// <param name="indices">The indices to add to the buffer.</param>
        void Add(params T[] indices);

        /// <summary>
        /// Adds a single index to the buffer.
        /// </summary>
        /// <param name="value">The index to add to the buffer.</param>
        void Add(T value);
    }
}