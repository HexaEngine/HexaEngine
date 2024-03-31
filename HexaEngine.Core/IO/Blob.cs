namespace HexaEngine.Core.IO
{
    using HexaEngine.Core.Graphics;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents a blob of data that can be manipulated using pointer operations.
    /// </summary>
    public unsafe class Blob : IDisposable
    {
        private nint bufferPointer;
        private PointerSize pointerSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="Blob"/> class.
        /// </summary>
        public Blob()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Blob"/> class with a pointer to a buffer and its size.
        /// </summary>
        /// <param name="bufferPointer">A pointer to the data buffer.</param>
        /// <param name="pointerSize">The size of the data buffer.</param>
        public Blob(nint bufferPointer, PointerSize pointerSize)
        {
            this.bufferPointer = bufferPointer;
            this.pointerSize = pointerSize;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Blob"/> class with a pointer to a buffer and its size.
        /// </summary>
        /// <param name="bufferPointer">A pointer to the data buffer.</param>
        /// <param name="pointerSize">The size of the data buffer.</param>
        public Blob(void* bufferPointer, nuint pointerSize)
        {
            this.bufferPointer = new(bufferPointer);
            this.pointerSize = (int)pointerSize;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Blob"/> class from a byte array.
        /// </summary>
        /// <param name="data">The byte array containing data for the blob.</param>
        public Blob(byte[] data)
        {
            bufferPointer = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, bufferPointer, data.Length);
            pointerSize = data.Length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Blob"/> class from a span of bytes.
        /// </summary>
        /// <param name="data">The span of bytes containing data for the blob.</param>
        public Blob(Span<byte> data)
        {
            bufferPointer = Marshal.AllocHGlobal(data.Length);
            pointerSize = data.Length;
            fixed (byte* ptr = data)
            {
                Buffer.MemoryCopy(ptr, (void*)bufferPointer, pointerSize, pointerSize);
            }
        }

        /// <summary>
        /// Gets the pointer to the data buffer.
        /// </summary>
        public nint BufferPointer => bufferPointer;

        /// <summary>
        /// Gets the size of the data buffer.
        /// </summary>
        public PointerSize PointerSize => pointerSize;

        /// <summary>
        /// Converts the blob's data to a string (assumes ANSI encoding).
        /// </summary>
        /// <returns>The data as a string.</returns>
        public string AsString()
        {
            return Marshal.PtrToStringAnsi(bufferPointer) ?? string.Empty;
        }

        /// <summary>
        /// Converts the blob's data to a byte array.
        /// </summary>
        /// <returns>The data as a byte array.</returns>
        public byte[] AsBytes()
        {
            return new Span<byte>((void*)bufferPointer, pointerSize).ToArray();
        }

        /// <summary>
        /// Converts the blob's data to a span of bytes.
        /// </summary>
        /// <returns>The data as a span of bytes.</returns>
        public Span<byte> AsSpan()
        {
            return new Span<byte>((void*)bufferPointer, pointerSize);
        }

        public void CopyFrom(Span<byte> source)
        {
            if (bufferPointer != 0)
            {
                Marshal.FreeHGlobal(bufferPointer);
            }
            bufferPointer = Marshal.AllocHGlobal(source.Length);
            pointerSize = source.Length;
            fixed (byte* ptr = source)
            {
                Buffer.MemoryCopy(ptr, (void*)bufferPointer, pointerSize, pointerSize);
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the blob.
        /// </summary>
        public void Dispose()
        {
            Marshal.FreeHGlobal(bufferPointer);
            bufferPointer = IntPtr.Zero;
            pointerSize = 0;
            GC.SuppressFinalize(this);
        }
    }
}