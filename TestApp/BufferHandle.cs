namespace TestApp
{
    using System;

    public unsafe struct BufferHandle
    {
        public void* Buffer;
        public int Start;
        public int Length;

        public readonly Span<byte> Span => new(Buffer, Length);

        public readonly int End => Start + Length;
    }
}