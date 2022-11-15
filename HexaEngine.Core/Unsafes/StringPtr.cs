namespace HexaEngine.Core.Unsafes
{
    using System;
    using System.Buffers.Binary;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public enum Endianness : byte
    {
        LittleEndian = byte.MinValue,
        BigEndian = byte.MaxValue,
    }

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

        public static int Write(UnsafeString* str, Endianness endianness, Span<byte> dest)
        {
            Span<char> srcChars = new(str->Ptr, str->Length + 1);
            Span<byte> src = MemoryMarshal.AsBytes(srcChars);
            if (endianness == Endianness.LittleEndian)
                BinaryPrimitives.WriteInt32LittleEndian(dest, src.Length);
            if (endianness == Endianness.BigEndian)
                BinaryPrimitives.WriteInt32BigEndian(dest, src.Length);
            src.CopyTo(dest[4..]);
            return src.Length + 4;
        }

        public static int Read(UnsafeString** ppStr, Endianness endianness, Span<byte> src)
        {
            int length = endianness == Endianness.LittleEndian ? BinaryPrimitives.ReadInt32LittleEndian(src) : BinaryPrimitives.ReadInt32BigEndian(src);
            UnsafeString* pStr = Utilities.Alloc<UnsafeString>();

            *ppStr = pStr;
            pStr->Ptr = (char*)Utilities.Alloc(length);
            fixed (byte* srcPtr = src.Slice(4, length))
            {
                Unsafe.CopyBlock(pStr->Ptr, srcPtr, (uint)length);
            }
            pStr->Length = length;
            return length + 4;
        }



        public int Sizeof()
        {
            return (Length + 1) * sizeof(char) + 4;
        }

        public static implicit operator string(UnsafeString ptr)
        {
            return new(ptr.Ptr);
        }

        public static implicit operator UnsafeString(string str)
        {
            return new(str);
        }

        public override string ToString()
        {
            return this;
        }
    }

    public unsafe struct UnsafeArray<T> where T : unmanaged
    {
        public UnsafeArray(T[] array)
        {
            fixed (T* ptr = array)
            {
                Ptr = ptr;
            }
            Length = array.Length;
        }

        public UnsafeArray(T* ptr, int length)
        {
            Ptr = ptr;
            Length = length;
        }

        public T* Ptr;
        public int Length;

        public static int Write(UnsafeArray<T>* str, Endianness endianness, Span<byte> dest)
        {
            Span<T> srcChars = new(str->Ptr, str->Length);
            Span<byte> src = MemoryMarshal.AsBytes(srcChars);
            if (endianness == Endianness.LittleEndian)
                BinaryPrimitives.WriteInt32LittleEndian(dest, src.Length);
            if (endianness == Endianness.BigEndian)
                BinaryPrimitives.WriteInt32BigEndian(dest, src.Length);
            src.CopyTo(dest[4..]);
            return src.Length + 4;
        }

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

        public int Sizeof()
        {
            return Length * sizeof(T) + 4;
        }

        public static implicit operator Span<T>(UnsafeArray<T> ptr)
        {
            return new Span<T>(ptr.Ptr, ptr.Length);
        }

        public static implicit operator UnsafeArray<T>(T[] str)
        {
            return new(str);
        }
    }
}