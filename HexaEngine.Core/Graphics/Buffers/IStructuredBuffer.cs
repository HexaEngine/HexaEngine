namespace HexaEngine.Core.Graphics.Buffers
{
    /// <summary>
    /// Represents a structured buffer in graphics programming.
    /// </summary>
    public interface IStructuredBuffer : IBuffer
    {
        /// <summary>
        /// Gets or sets the capacity of the structured buffer.
        /// </summary>
        uint Capacity { get; set; }

        /// <summary>
        /// Gets the current count of elements in the structured buffer.
        /// </summary>
        uint Count { get; }

        /// <summary>
        /// Gets the shader resource view associated with the structured buffer.
        /// </summary>
        IShaderResourceView SRV { get; }

        /// <summary>
        /// Ensures that the structured buffer has at least the specified capacity.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        void EnsureCapacity(uint capacity);

        /// <summary>
        /// Removes the element at the specified index in the structured buffer.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        void RemoveAt(int index);

        /// <summary>
        /// Resets the counter of elements to zero.
        /// </summary>
        void ResetCounter();

        /// <summary>
        /// Clears the buffer by resetting the item counter to zero and marking the buffer as dirty.
        /// </summary>
        void Clear();

        /// <summary>
        /// Erases the contents of the buffer by zeroing out the memory and resetting the item counter to zero and marking the buffer as dirty.
        /// </summary>
        void Erase();

        /// <summary>
        /// Updates the structured buffer in the graphics context.
        /// </summary>
        /// <param name="context">The graphics context used for the update.</param>
        /// <returns>True if the structured buffer was updated; otherwise, false.</returns>
        bool Update(IGraphicsContext context);
    }

    /// <summary>
    /// Represents a typed structured buffer in graphics programming.
    /// </summary>
    /// <typeparam name="T">The type of elements in the structured buffer.</typeparam>
    public interface IStructuredBuffer<T> : IStructuredBuffer where T : unmanaged
    {
        /// <summary>
        /// Gets or sets the value at the specified index in the structured buffer.
        /// </summary>
        /// <param name="index">The index of the value to get or set.</param>
        T this[int index] { get; set; }

        /// <summary>
        /// Gets or sets the element at the specified index in the buffer.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        T this[uint index] { get; set; }

        /// <summary>
        /// Adds an element to the structured buffer.
        /// </summary>
        /// <param name="item">The element to add to the structured buffer.</param>
        void Add(T item);

        /// <summary>
        /// Removes the specified item from the buffer.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns><c>true</c> if the item is successfully removed; otherwise, <c>false</c>.</returns>
        bool Remove(T item);

        /// <summary>
        /// Determines whether the buffer contains the specified item.
        /// </summary>
        /// <param name="item">The item to locate in the buffer.</param>
        /// <returns><c>true</c> if the item is found in the buffer; otherwise, <c>false</c>.</returns>
        bool Contains(T item);

        /// <summary>
        /// Returns the index of the specified item in the buffer.
        /// </summary>
        /// <param name="item">The item to locate in the buffer.</param>
        /// <returns>The index of the item if found; otherwise, -1.</returns>
        int IndexOf(T item);
    }
}