namespace HexaEngine.Core.Unsafes
{
    using System;
    using System.Buffers.Binary;
    using System.Runtime.InteropServices;

    public unsafe struct UnsafeString
    {
        public UnsafeString(string str)
        {
            fixed (char* strPtr = str)
            {
                Ptr = strPtr;
            }
            Length = str.Length;
        }

        public UnsafeString(char* ptr, int length)
        {
            Ptr = ptr;
            Length = length;
        }

        public char* Ptr;
        public int Length;

        public static int Write(UnsafeString* str, Span<byte> dest)
        {
            Span<char> srcChars = new(str->Ptr, str->Length);
            Span<byte> src = MemoryMarshal.AsBytes(srcChars);
            BinaryPrimitives.WriteInt32LittleEndian(dest, src.Length);
            src.CopyTo(dest[4..]);
            return src.Length + 4;
        }

        public static int Read(UnsafeString* str, Span<byte> src)
        {
            int length = BinaryPrimitives.ReadInt32LittleEndian(src);
            fixed (byte* srcPtr = src.Slice(4, length))
            {
                str->Ptr = (char*)srcPtr;
            }
            str->Length = length;
            return length + 4;
        }

        public static implicit operator string(UnsafeString ptr)
        {
            return new(new Span<char>(ptr.Ptr, ptr.Length));
        }

        public static implicit operator UnsafeString(string str)
        {
            return new(str);
        }
    }
}