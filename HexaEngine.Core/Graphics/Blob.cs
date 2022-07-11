namespace HexaEngine.Core.Graphics
{
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public unsafe class Blob : IDisposable
    {
        public Blob(IntPtr bufferPointer, PointerSize pointerSize)
        {
            BufferPointer = bufferPointer;
            PointerSize = pointerSize;
        }

        public Blob(void* bufferPointer, nuint pointerSize)
        {
            BufferPointer = new(bufferPointer);
            PointerSize = (int)pointerSize;
        }

        public Blob(byte[] data)
        {
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
            return Marshal.PtrToStringAnsi(BufferPointer);
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
            Marshal.FreeHGlobal(BufferPointer);
        }
    }
}