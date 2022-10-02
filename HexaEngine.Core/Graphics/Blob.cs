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

        public IntPtr BufferPointer { get; }

        public PointerSize PointerSize { get; }

        public string AsString()
        {
            return Marshal.PtrToStringAnsi(BufferPointer) ?? string.Empty;
        }

        public byte[] AsBytes()
        {
            byte[] result = new byte[PointerSize];
            fixed (byte* pResult = result)
            {
                Unsafe.CopyBlockUnaligned(pResult, (void*)BufferPointer, (uint)result.Length);
            }

            return result;
        }

        public Span<byte> AsSpan()
        {
            Span<byte> result = new byte[PointerSize];
            fixed (byte* pResult = result)
            {
                Unsafe.CopyBlockUnaligned(pResult, (void*)BufferPointer, (uint)result.Length);
            }

            return result;
        }

        public void Dispose()
        {
            if (_native)
                Marshal.FreeHGlobal(BufferPointer);
        }
    }
}