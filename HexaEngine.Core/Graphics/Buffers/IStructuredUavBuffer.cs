namespace HexaEngine.Core.Graphics.Buffers
{
    /// <summary>
    /// Represents a structured unordered access view buffer in graphics programming.
    /// </summary>
    public interface IStructuredUavBuffer : IBuffer
    {
        /// <summary>
        /// Gets a value indicating whether the buffer can be read.
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        /// Gets a value indicating whether the buffer can be written to.
        /// </summary>
        bool CanWrite { get; }

        /// <summary>
        /// Gets or sets the capacity of the structured UAV buffer.
        /// </summary>
        uint Capacity { get; set; }

        /// <summary>
        /// Gets the copy buffer associated with the structured UAV buffer.
        /// </summary>
        IBuffer? CopyBuffer { get; }

        /// <summary>
        /// Gets the current count of elements in the structured UAV buffer.
        /// </summary>
        uint Count { get; }

        /// <summary>
        /// Gets the shader resource view associated with the structured UAV buffer.
        /// </summary>
        IShaderResourceView SRV { get; }

        /// <summary>
        /// Gets the unordered access view associated with the structured UAV buffer.
        /// </summary>
        IUnorderedAccessView UAV { get; }

        /// <summary>
        /// Resets the counter of elements to zero.
        /// </summary>
        void ResetCounter();

        /// <summary>
        /// Clears the buffer, setting the counter to zero and marking the buffer as dirty.
        /// </summary>
        void Clear();

        /// <summary>
        /// Erases all items in the buffer, setting the counter to zero and marking the buffer as dirty.
        /// </summary>
        void Erase();

        /// <summary>
        /// Clears the structured UAV buffer in the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context to use for clearing the buffer.</param>
        void Clear(IGraphicsContext context);

        /// <summary>
        /// Copies the contents of the structured UAV buffer to the specified buffer in the graphics context.
        /// </summary>
        /// <param name="context">The graphics context to use for copying the data.</param>
        /// <param name="buffer">The destination buffer.</param>
        void CopyTo(IGraphicsContext context, IBuffer buffer);

        /// <summary>
        /// Ensures that the structured UAV buffer has at least the specified capacity.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        void EnsureCapacity(uint capacity);

        /// <summary>
        /// Reads the structured UAV buffer in the graphics context.
        /// </summary>
        /// <param name="context">The graphics context to use for reading the buffer.</param>
        void Read(IGraphicsContext context);

        /// <summary>
        /// Removes the element at the specified index in the structured UAV buffer.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        void RemoveAt(int index);

        /// <summary>
        /// Removes the element at the specified index in the structured UAV buffer.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        void RemoveAt(uint index);



        /// <summary>
        /// Updates the structured UAV buffer in the graphics context.
        /// </summary>
        /// <param name="context">The graphics context used for the update.</param>
        /// <returns>True if the structured UAV buffer was updated; otherwise, false.</returns>
        bool Update(IGraphicsContext context);
    }

    /// <summary>
    /// Represents a typed structured unordered access view buffer in graphics programming.
    /// </summary>
    /// <typeparam name="T">The type of elements in the structured UAV buffer.</typeparam>
    public interface IStructuredUavBuffer<T> : IStructuredUavBuffer where T : unmanaged
    {
        /// <summary>
        /// Gets or sets the value at the specified index in the structured UAV buffer.
        /// </summary>
        /// <param name="index">The index of the value to get or set.</param>
        T this[int index] { get; set; }

        /// <summary>
        /// Gets or sets the value at the specified index in the structured UAV buffer.
        /// </summary>
        /// <param name="index">The index of the value to get or set.</param>
        T this[uint index] { get; set; }

        /// <summary>
        /// Adds an element to the structured UAV buffer and returns a reference to the added element.
        /// </summary>
        /// <param name="item">The element to add to the structured UAV buffer.</param>
        /// <returns>A reference to the added element.</returns>
        ref T Add(T item);

        /// <summary>
        /// Removes the specified item from the buffer.
        /// </summary>
        /// <param name="item">The item to remove from the buffer.</param>
        /// <returns>True if the item was successfully removed; otherwise, false.</returns>
        bool Remove(T item);

        /// <summary>
        /// Determines whether the buffer contains the specified item.
        /// </summary>
        /// <param name="item">The item to locate in the buffer.</param>
        /// <returns>True if the item is found in the buffer; otherwise, false.</returns>
        bool Contains(T item);

        /// <summary>
        /// Returns the index of the first occurrence of the specified item in the buffer.
        /// </summary>
        /// <param name="item">The item to locate in the buffer.</param>
        /// <returns>The index of the first occurrence of the item in the buffer, or -1 if the item is not found.</returns>
        int IndexOf(T item);
    }
}