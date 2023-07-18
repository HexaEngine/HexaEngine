namespace HexaEngine.Core.Unsafes
{
    using HexaEngine.Mathematics;
    using System;
    using System.Buffers.Binary;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public unsafe struct UnsafeString : IFreeable, IEquatable<UnsafeString>
    {
        public UnsafeString(string str)
        {
            nint len = str.Length * sizeof(char);
            Ptr = (char*)Marshal.AllocHGlobal(len);
            fixed (char* strPtr = str)
            {
                Memcpy(strPtr, Ptr, len, len);
            }
            Length = str.Length;
        }

        public UnsafeString(char* ptr, int length)
        {
            Ptr = ptr;
            Length = length;
        }

        public UnsafeString(int length)
        {
            Ptr = (char*)Marshal.AllocHGlobal(length * sizeof(char));
            Length = length;
        }

        public char* Ptr;
        public int Length;

        public bool Compare(UnsafeString* other)
        {
            if (Length != other->Length)
            {
                return false;
            }

            for (uint i = 0; i < Length; i++)
            {
                if (Ptr[i] != other->Ptr[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static int Write(UnsafeString* str, Endianness endianness, Span<byte> dest)
        {
            Span<char> srcChars = new(str->Ptr, str->Length + 1);
            Span<byte> src = MemoryMarshal.AsBytes(srcChars);
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteInt32LittleEndian(dest, src.Length);
            }

            if (endianness == Endianness.BigEndian)
            {
                BinaryPrimitives.WriteInt32BigEndian(dest, src.Length);
            }

            src.CopyTo(dest[4..]);
            return src.Length + 4;
        }

        public static int Read(UnsafeString** ppStr, Endianness endianness, Span<byte> src)
        {
            int length = endianness == Endianness.LittleEndian ? BinaryPrimitives.ReadInt32LittleEndian(src) : BinaryPrimitives.ReadInt32BigEndian(src);
            UnsafeString* pStr = Alloc<UnsafeString>();

            *ppStr = pStr;
            pStr->Ptr = Alloc<char>(length);
            fixed (byte* srcPtr = src.Slice(4, length))
            {
                Unsafe.CopyBlock(pStr->Ptr, srcPtr, (uint)length);
            }
            pStr->Length = length;
            return length + 4;
        }

        public void Release()
        {
            Free(Ptr);
            Ptr = null;
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

        public static implicit operator ReadOnlySpan<char>(UnsafeString ptr)
        {
            return new(ptr.Ptr, ptr.Length);
        }

        public static implicit operator Span<char>(UnsafeString ptr)
        {
            return new(ptr.Ptr, ptr.Length);
        }

        public static implicit operator char*(UnsafeString ptr)
        {
            return ptr.Ptr;
        }

        public static bool operator ==(UnsafeString left, UnsafeString right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UnsafeString left, UnsafeString right)
        {
            return !(left == right);
        }

        public static bool operator ==(UnsafeString left, string right)
        {
            if (left.Length != right.Length)
                return false;

            ReadOnlySpan<char> chars1 = new(left.Ptr, left.Length);
            ReadOnlySpan<char> chars2 = right;

            return chars1.SequenceEqual(chars2);
        }

        public static bool operator !=(UnsafeString left, string right)
        {
            return !(left == right);
        }

        public static bool operator ==(string left, UnsafeString right)
        {
            if (left.Length != right.Length)
                return false;

            ReadOnlySpan<char> chars1 = left;
            ReadOnlySpan<char> chars2 = new(right.Ptr, right.Length);

            return chars1.SequenceEqual(chars2);
        }

        public static bool operator !=(string left, UnsafeString right)
        {
            return !(left == right);
        }

        public override readonly string ToString()
        {
            return this;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is UnsafeString @string && Equals(@string);
        }

        public readonly bool Equals(UnsafeString other)
        {
            if (Length != other.Length)
                return false;

            ReadOnlySpan<char> chars1 = new(Ptr, Length);
            ReadOnlySpan<char> chars2 = new(other.Ptr, Length);

            return chars1.SequenceEqual(chars2);
        }

        public override readonly int GetHashCode()
        {
            return string.GetHashCode(this);
        }

        public readonly int GetHashCode(StringComparison comparison)
        {
            return string.GetHashCode(this, comparison);
        }
    }
}