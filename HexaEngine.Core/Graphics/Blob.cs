namespace HexaEngine.Core.Graphics
{
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public unsafe class Blob : IDisposable
    {
        private bool _native;

        public Blob(IntPtr bufferPointer, PointerSize pointerSize)
        {
            _native = true;
            BufferPointer = bufferPointer;
            PointerSize = pointerSize;
        }

        public Blob(void* bufferPointer, nuint pointerSize)
        {
            _native = false;
            BufferPointer = new(bufferPointer);
            PointerSize = (int)pointerSize;
        }

        public Blob(byte[] data)
        {
            _native = false;
            fixed (byte* ptr = data)
            {
                BufferPointer = new(ptr);
            }
            PointerSize = data.Length;
        }

        public Blob(Span<byte> data)
        {
            _native = false;
            fixed (byte* ptr = data)
            {
                BufferPointer = new(ptr);
            }
            PointerSize = data.Length;
        }

        public IntPtr BufferPointer { get; }

        public PointerSize PointerSize { get; }

        public string AsString()
        {
            return Marshal.PtrToStringAnsi(BufferPointer) ?? string.Empty;
        }

        public byte[] AsBytes()
        {
            return new Span<byte>((void*)BufferPointer, PointerSize).ToArray();
        }

        public Span<byte> AsSpan()
        {
            return new Span<byte>((void*)BufferPointer, PointerSize);
        }

        public void Dispose()
        {
            if (_native)
                Marshal.FreeHGlobal(BufferPointer);
        }
    }
}