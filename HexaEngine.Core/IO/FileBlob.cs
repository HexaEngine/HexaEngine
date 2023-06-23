namespace HexaEngine.Core.IO
{
    using System;

    public unsafe struct FileBlob
    {
        public byte* Data;
        public nint Length;

        public FileBlob(byte* data, nint length)
        {
            Data = data;
            Length = length;
        }

        public FileBlob(nint length)
        {
            Data = (byte*)Malloc(length);
            Length = length;
        }

        public readonly Span<byte> AsSpan()
        {
            return new Span<byte>(Data, (int)Length);
        }

        public readonly byte[] ToArray()
        {
            return AsSpan().ToArray();
        }

        public void Release()
        {
            Free(Data);
            Data = null;
            Length = 0;
        }
    }
}