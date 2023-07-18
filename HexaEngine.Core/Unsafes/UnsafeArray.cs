namespace HexaEngine.Core.Unsafes
{
    using HexaEngine.Mathematics;
    using System;
    using System.Buffers.Binary;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents an unsafe array of elements of type T.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    public unsafe struct UnsafeArray<T> where T : unmanaged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeArray{T}"/> struct with an existing array.
        /// </summary>
        /// <param name="array">The existing array to create an unsafe array from.</param>
        public UnsafeArray(T[] array)
        {
            Ptr = AllocCopy(array);
            Length = array.Length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeArray{T}"/> struct with a pointer and length.
        /// </summary>
        /// <param name="ptr">The pointer to the start of the array.</param>
        /// <param name="length">The length of the array.</param>
        public UnsafeArray(T* ptr, int length)
        {
            Ptr = ptr;
            Length = length;
        }

        /// <summary>
        /// Gets or sets the pointer to the start of the array.
        /// </summary>
        public T* Ptr;

        /// <summary>
        /// Gets or sets the length of the array.
        /// </summary>
        public int Length;

        /// <summary>
        /// Writes the contents of the unsafe array to a span of bytes, using the specified endianness.
        /// </summary>
        /// <param name="str">The unsafe array to write.</param>
        /// <param name="endianness">The endianness to use when writing the length of the array.</param>
        /// <param name="dest">The destination span of bytes to write to.</param>
        /// <returns>The number of bytes written.</returns>
        public static int Write(UnsafeArray<T>* str, Endianness endianness, Span<byte> dest)
        {
            Span<T> srcChars = new(str->Ptr, str->Length);
            Span<byte> src = MemoryMarshal.AsBytes(srcChars);
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteInt32LittleEndian(dest, src.Length);
            }

            if (endianness == Endianness.BigEndian)
            {
                BinaryPrimitives.WriteInt32BigEndian(dest, src.Length);
            }

            src.CopyTo(dest[4..]);
            return src.Length + 4;
        }

        /// <summary>
        /// Reads the contents of a span of bytes into the unsafe array, using the specified endianness.
        /// </summary>
        /// <param name="str">The unsafe array to read into.</param>
        /// <param name="endianness">The endianness to use when reading the length of the array.</param>
        /// <param name="src">The source span of bytes to read from.</param>
        /// <returns>The number of bytes read.</returns>
        public static int Read(UnsafeArray<T>* str, Endianness endianness, Span<byte> src)
        {
            int length = endianness == Endianness.LittleEndian ? BinaryPrimitives.ReadInt32LittleEndian(src) : BinaryPrimitives.ReadInt32BigEndian(src);
            fixed (byte* srcPtr = src.Slice(4, length))
            {
                str->Ptr = (T*)srcPtr;
            }
            str->Length = length;
            return length + 4;
        }

        /// <summary>
        /// Gets the size in bytes of the unsafe array.
        /// </summary>
        /// <returns>The size in bytes of the unsafe array.</returns>
        public int Sizeof()
        {
            return Length * sizeof(T) + 4;
        }

        /// <summary>
        /// Releases the memory associated with the unsafe array.
        /// </summary>
        public void Release()
        {
            Free(Ptr);
        }

        /// <summary>
        /// Implicitly converts the unsafe array to a span of elements of type T.
        /// </summary>
        /// <param name="ptr">The unsafe array to convert.</param>
        /// <returns>A span of elements of type T.</returns>
        public static implicit operator Span<T>(UnsafeArray<T> ptr)
        {
            return new Span<T>(ptr.Ptr, ptr.Length);
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