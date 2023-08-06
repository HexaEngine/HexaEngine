namespace HexaEngine.Core.IO
{
    using System;

    /// <summary>
    /// Represents a blob of file data.
    /// </summary>
    public unsafe struct FileBlob
    {
        /// <summary>
        /// Pointer to the data of the file blob.
        /// </summary>
        public byte* Data;

        /// <summary>
        /// Length of the file blob data.
        /// </summary>
        public nint Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileBlob"/> struct with the specified data pointer and length.
        /// </summary>
        /// <param name="data">Pointer to the data of the file blob.</param>
        /// <param name="length">Length of the file blob data.</param>
        public FileBlob(byte* data, nint length)
        {
            Data = data;
            Length = length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileBlob"/> struct with the specified length and allocates memory for the data.
        /// </summary>
        /// <param name="length">Length of the file blob data to allocate.</param>
        public FileBlob(nint length)
        {
            Data = (byte*)Alloc(length);
            Length = length;
        }

        /// <summary>
        /// Returns a readonly span that represents the file blob data.
        /// </summary>
        /// <returns>A readonly span representing the file blob data.</returns>
        public readonly Span<byte> AsSpan()
        {
            return new Span<byte>(Data, (int)Length);
        }

        /// <summary>
        /// Converts the file blob data to a byte array.
        /// </summary>
        /// <returns>A byte array representing the file blob data.</returns>
        public readonly byte[] ToArray()
        {
            return AsSpan().ToArray();
        }

        /// <summary>
        /// Releases the memory allocated for the file blob data.
        /// </summary>
        public void Release()
        {
            Free(Data);
            Data = null;
            Length = 0;
        }
    }
}