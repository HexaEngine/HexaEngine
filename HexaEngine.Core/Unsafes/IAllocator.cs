namespace HexaEngine.Core.Unsafes
{
    /// <summary>
    /// Represents an allocator for memory management.
    /// </summary>
    public unsafe interface IAllocator
    {
        /// <summary>
        /// Allocates a block of memory of the specified width.
        /// </summary>
        /// <param name="width">The width of the memory block to allocate.</param>
        /// <returns>A pointer to the allocated memory block.</returns>
        void* Allocate(nint width);

        /// <summary>
        /// Allocates a block of memory for a single value of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to allocate memory for.</typeparam>
        /// <returns>A pointer to the allocated memory block.</returns>
        T* Allocate<T>() where T : unmanaged;

        /// <summary>
        /// Allocates a block of memory of the specified width and height.
        /// </summary>
        /// <param name="width">The width of the memory block to allocate.</param>
        /// <param name="height">The height of the memory block to allocate.</param>
        /// <returns>A double pointer to the allocated memory block.</returns>
        void** Allocate(uint width, uint height);

        /// <summary>
        /// Allocates a block of memory for an array of values of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the values to allocate memory for.</typeparam>
        /// <param name="count">The number of elements in the array.</param>
        /// <returns>A pointer to the allocated memory block.</returns>
        T* Allocate<T>(uint count) where T : unmanaged;

        /// <summary>
        /// Allocates a block of memory for an array of pointers to values of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the values to allocate memory for.</typeparam>
        /// <param name="count">The number of elements in the array.</param>
        /// <returns>A double pointer to the allocated memory block.</returns>
        T** AllocateArray<T>(uint count) where T : unmanaged;

        /// <summary>
        /// Frees the memory block pointed to by the specified pointer.
        /// </summary>
        /// <param name="ptr">A pointer to the memory block to free.</param>
        void Free(void* ptr);

        /// <summary>
        /// Frees the memory block pointed to by the specified pointer and releases any resources associated with it.
        /// </summary>
        /// <typeparam name="T">The type of the value stored in the memory block.</typeparam>
        /// <param name="ptr">A pointer to the memory block to free.</param>
        void Free<T>(T* ptr) where T : unmanaged, IFreeable;
    }
}