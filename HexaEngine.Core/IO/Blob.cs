namespace HexaEngine.Core.IO
{
    using System.Runtime.InteropServices;
    using HexaEngine.Core.Graphics;

    /// <summary>
    /// Represents a blob of data that can be manipulated using pointer operations.
    /// </summary>
    public unsafe class Blob : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Blob"/> class with a pointer to a buffer and its size.
        /// </summary>
        /// <param name="bufferPointer">A pointer to the data buffer.</param>
        /// <param name="pointerSize">The size of the data buffer.</param>
        public Blob(nint bufferPointer, PointerSize pointerSize)
        {
            BufferPointer = bufferPointer;
            PointerSize = pointerSize;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Blob"/> class with a pointer to a buffer and its size.
        /// </summary>
        /// <param name="bufferPointer">A pointer to the data buffer.</param>
        /// <param name="pointerSize">The size of the data buffer.</param>
        public Blob(void* bufferPointer, nuint pointerSize)
        {
            BufferPointer = new(bufferPointer);
            PointerSize = (int)pointerSize;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Blob"/> class from a byte array.
        /// </summary>
        /// <param name="data">The byte array containing data for the blob.</param>
        public Blob(byte[] data)
        {
            BufferPointer = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, BufferPointer, data.Length);
            PointerSize = data.Length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Blob"/> class from a span of bytes.
        /// </summary>
        /// <param name="data">The span of bytes containing data for the blob.</param>
        public Blob(Span<byte> data)
        {
            BufferPointer = Marshal.AllocHGlobal(data.Length);
            PointerSize = data.Length;
            fixed (byte* ptr = data)
            {
                Buffer.MemoryCopy(ptr, (void*)BufferPointer, PointerSize, PointerSize);
            }
        }

        /// <summary>
        /// Gets the pointer to the data buffer.
        /// </summary>
        public nint BufferPointer { get; }

        /// <summary>
        /// Gets the size of the data buffer.
        /// </summary>
        public PointerSize PointerSize { get; }

        /// <summary>
        /// Converts the blob's data to a string (assumes ANSI encoding).
        /// </summary>
        /// <returns>The data as a string.</returns>
        public string AsString()
        {
            return Marshal.PtrToStringAnsi(BufferPointer) ?? string.Empty;
        }

        /// <summary>
        /// Converts the blob's data to a byte array.
        /// </summary>
        /// <returns>The data as a byte array.</returns>
        public byte[] AsBytes()
        {
            return new Span<byte>((void*)BufferPointer, PointerSize).ToArray();
        }

        /// <summary>
        /// Converts the blob's data to a span of bytes.
        /// </summary>
        /// <returns>The data as a span of bytes.</returns>
        public Span<byte> AsSpan()
        {
            return new Span<byte>((void*)BufferPointer, PointerSize);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the blob.
        /// </summary>
        public void Dispose()
        {
            Marshal.FreeHGlobal(BufferPointer);
            GC.SuppressFinalize(this);
        }
    }
}