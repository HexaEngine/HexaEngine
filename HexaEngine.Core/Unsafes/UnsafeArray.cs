namespace HexaEngine.Core.Unsafes
{
    using System;

    /// <summary>
    /// Represents an unsafe array of elements of type T.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    public unsafe struct UnsafeArray<T> where T : unmanaged
    {
        private T* pointer;
        private uint length;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeArray{T}"/> struct with an existing array.
        /// </summary>
        /// <param name="array">The existing array to create an unsafe array from.</param>
        public UnsafeArray(T[] array)
        {
            pointer = AllocCopyT(array);
            length = (uint)array.Length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeArray{T}"/> struct with a pointer and length.
        /// </summary>
        /// <param name="ptr">The pointer to the start of the array.</param>
        /// <param name="length">The length of the array.</param>
        public UnsafeArray(T* ptr, uint length)
        {
            pointer = ptr;
            this.length = length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeArray{T}"/> struct with a pointer and length.
        /// </summary>
        /// <param name="ptr">The pointer to the start of the array.</param>
        /// <param name="length">The length of the array.</param>
        public UnsafeArray(uint length)
        {
            pointer = AllocT<T>(length);
            ZeroMemoryT(pointer, length);
            this.length = length;
        }

        public readonly T* Data => pointer;

        public readonly uint Length => length;

        public T this[int index]
        {
            get { return pointer[index]; }
            set { pointer[index] = value; }
        }

        public T this[uint index]
        {
            get { return pointer[index]; }
            set { pointer[index] = value; }
        }

        public T* GetPointer(uint index)
        {
            return &pointer[index];
        }

        public T* GetPointer(int index)
        {
            return &pointer[index];
        }

        /// <summary>
        /// Gets the size in bytes of the unsafe array.
        /// </summary>
        /// <returns>The size in bytes of the unsafe array.</returns>
        public readonly uint Sizeof()
        {
            return length * (uint)sizeof(T) + 4;
        }

        /// <summary>
        /// Releases the memory associated with the unsafe array.
        /// </summary>
        public void Release()
        {
            Free(pointer);
            pointer = null;
            length = 0;
        }

        /// <summary>
        /// Implicitly converts the unsafe array to a span of elements of type T.
        /// </summary>
        /// <param name="ptr">The unsafe array to convert.</param>
        /// <returns>A span of elements of type T.</returns>
        public static implicit operator Span<T>(UnsafeArray<T> ptr)
        {
            return new Span<T>(ptr.pointer, (int)ptr.Length);
        }

        /// <summary>
        /// Implicitly converts an array of elements of type T to an unsafe array.
        /// </summary>
        /// <param name="str">The array to convert.</param>
        /// <returns>An unsafe array of elements of type T.</returns>
        public static implicit operator UnsafeArray<T>(T[] str)
        {
            return new UnsafeArray<T>(str);
        }
    }
}