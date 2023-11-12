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
        uint Capacity { get; set; }

        /// <summary>
        /// Gets the number of vertices in the vertex buffer.
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
        /// Resets the item count to zero.
        /// </summary>
        public void ResetCounter();

        /// <summary>
        /// Clears the vertex buffer.
        /// </summary>
        void Clear();

        /// <summary>
        /// Erases the buffer by zeroing out its memory, setting the item count to zero, and marking it as dirty.
        /// </summary>
        public void Erase();

        /// <summary>
        /// Copies the contents of the vertex buffer to the specified buffer in the graphics context.
        /// </summary>
        /// <param name="context">The graphics context to use for copying the data.</param>
        /// <param name="buffer">The destination buffer.</param>
        void CopyTo(IGraphicsContext context, IBuffer buffer);

        /// <summary>
        /// Ensures that the vertex buffer has the specified capacity.
        /// </summary>
        /// <param name="capacity">The desired capacity of the vertex buffer.</param>
        void EnsureCapacity(uint capacity);

        /// <summary>
        /// Flushes the memory associated with the vertex buffer.
        /// </summary>
        void FlushMemory();

        /// <summary>
        /// Removes a vertex at the specified index from the vertex buffer.
        /// </summary>
        /// <param name="index">The index of the vertex to remove.</param>
        void RemoveAt(int index);

        /// <summary>
        /// Unbinds the vertex buffer from the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context to unbind the vertex buffer from.</param>
        void Unbind(IGraphicsContext context);

        /// <summary>
        /// Updates the vertex buffer in the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context to use for updating the vertex buffer.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        bool Update(IGraphicsContext context);
    }

    /// <summary>
    /// Represents a generic vertex buffer in graphics programming.
    /// </summary>
    /// <typeparam name="T">The type of vertices in the buffer.</typeparam>
    public interface IVertexBuffer<T> : IVertexBuffer where T : unmanaged
    {
        /// <summary>
        /// Gets or sets a vertex at the specified index in the vertex buffer.
        /// </summary>
        /// <param name="index">The index of the vertex to get or set.</param>
        /// <returns>The vertex at the specified index.</returns>
        T this[int index] { get; set; }

        /// <summary>
        /// Adds a vertex to the vertex buffer.
        /// </summary>
        /// <param name="vertex">The vertex to add to the buffer.</param>
        void Add(T vertex);

        /// <summary>
        /// Adds vertices to the vertex buffer.
        /// </summary>
        /// <param name="vertices">The vertices to add to the buffer.</param>
        void Add(params T[] vertices);
    }
}