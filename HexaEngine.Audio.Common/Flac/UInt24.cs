namespace HexaEngine.Audio.Common.Flac
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct UInt24 : IEquatable<UInt24>, IComparable<UInt24>, IComparable
    {
        private readonly byte byte1;
        private readonly byte byte2;
        private readonly byte byte3;

        public UInt24(uint value)
        {
            if (value > MaxValue)
            {
                throw new OverflowException("Value is out of range for UInt24.");
            }

            byte1 = (byte)((value >> 0) & 0xFF);
            byte2 = (byte)((value >> 8) & 0xFF);
            byte3 = (byte)((value >> 16) & 0xFF);
        }

        public static readonly uint MaxValue = 0x00FFFFFF;
        public static readonly uint MinValue = 0;

        public readonly uint ToUInt32()
        {
            return (uint)(byte1 | (byte2 << 8) | (byte3 << 16));
        }

        public override bool Equals(object? obj)
        {
            return obj is UInt24 @int &&
                   byte1 == @int.byte1 &&
                   byte2 == @int.byte2 &&
                   byte3 == @int.byte3;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(byte1, byte2, byte3);
        }

        public int CompareTo(object? value)
        {
            if (value is UInt24 other)
            {
                return CompareTo(other);
            }
            else if (value is null)
            {
                return 1;
            }
            else
            {
                throw new ArgumentException("Must be UInt24");
            }
        }

        public int CompareTo(UInt24 value)
        {
            if (this < value)
            {
                return -1;
            }
            else if (this > value)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public bool Equals(UInt24 other)
        {
            return byte1 == other.byte1 &&
                   byte2 == other.byte2 &&
                   byte3 == other.byte3;
        }

        public static UInt24 operator +(UInt24 a, UInt24 b)
        {
            return new UInt24(a.ToUInt32() + b.ToUInt32());
        }

        public static UInt24 operator -(UInt24 a, UInt24 b)
        {
            return new UInt24(a.ToUInt32() - b.ToUInt32());
        }

        public static UInt24 operator *(UInt24 a, UInt24 b)
        {
            return new UInt24(a.ToUInt32() * b.ToUInt32());
        }

        public static UInt24 operator /(UInt24 a, UInt24 b)
        {
            if (b.ToUInt32() == 0)
            {
                throw new DivideByZeroException("Division by zero in UInt24.");
            }
            return new UInt24(a.ToUInt32() / b.ToUInt32());
        }

        public static UInt24 operator %(UInt24 a, UInt24 b)
        {
            if (b.ToUInt32() == 0)
            {
                throw new DivideByZeroException("Division by zero in UInt24.");
            }
            return new UInt24(a.ToUInt32() % b.ToUInt32());
        }

        public static UInt24 operator &(UInt24 a, UInt24 b)
        {
            return new UInt24(a.ToUInt32() & b.ToUInt32());
        }

        public static UInt24 operator |(UInt24 a, UInt24 b)
        {
            return new UInt24(a.ToUInt32() | b.ToUInt32());
        }

        public static UInt24 operator ^(UInt24 a, UInt24 b)
        {
            return new UInt24(a.ToUInt32() ^ b.ToUInt32());
        }

        public static UInt24 operator >>(UInt24 a, Int24 b)
        {
            return new UInt24(a.ToUInt32() >> b.ToInt32());
        }

        public static UInt24 operator <<(UInt24 a, Int24 b)
        {
            return new UInt24(a.ToUInt32() << b.ToInt32());
        }

        public static UInt24 operator >>>(UInt24 a, Int24 b)
        {
            return new UInt24(a.ToUInt32() >>> b.ToInt32());
        }

        public static uint operator +(UInt24 a, uint b)
        {
            return a.ToUInt32() + b;
        }

        public static uint operator -(UInt24 a, uint b)
        {
            return a.ToUInt32() - b;
        }

        public static uint operator *(UInt24 a, uint b)
        {
            return a.ToUInt32() * b;
        }

        public static uint operator /(UInt24 a, uint b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException("Division by zero in UInt24.");
            }
            return a.ToUInt32() / b;
        }

        public static uint operator %(UInt24 a, uint b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException("Division by zero in UInt24.");
            }
            return a.ToUInt32() % b;
        }

        public static uint operator &(UInt24 a, uint b)
        {
            return a.ToUInt32() & b;
        }

        public static uint operator |(UInt24 a, uint b)
        {
            return a.ToUInt32() | b;
        }

        public static uint operator ^(UInt24 a, uint b)
        {
            return a.ToUInt32() ^ b;
        }

        public static uint operator >>(UInt24 a, int b)
        {
            return a.ToUInt32() >> b;
        }

        public static uint operator <<(UInt24 a, int b)
        {
            return a.ToUInt32() << b;
        }

        public static uint operator >>>(UInt24 a, int b)
        {
            return a.ToUInt32() >>> b;
        }

        public static ulong operator +(UInt24 a, ulong b)
        {
            return a.ToUInt32() + b;
        }

        public static ulong operator -(UInt24 a, ulong b)
        {
            return a.ToUInt32() - b;
        }

        public static ulong operator *(UInt24 a, ulong b)
        {
            return a.ToUInt32() * b;
        }

        public static ulong operator /(UInt24 a, ulong b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException("Division by zero in UInt24.");
            }
            return a.ToUInt32() / b;
        }

        public static ulong operator %(UInt24 a, ulong b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException("Division by zero in UInt24.");
            }
            return a.ToUInt32() % b;
        }

        public static ulong operator &(UInt24 a, ulong b)
        {
            return a.ToUInt32() & b;
        }

        public static ulong operator |(UInt24 a, ulong b)
        {
            return a.ToUInt32() | b;
        }

        public static ulong operator ^(UInt24 a, ulong b)
        {
            return a.ToUInt32() ^ b;
        }

        public static UInt24 operator ++(UInt24 a)
        {
            return new UInt24(a.ToUInt32() + 1);
        }

        public static UInt24 operator --(UInt24 a)
        {
            return new UInt24(a.ToUInt32() - 1);
        }

        public static explicit operator UInt24(uint value)
        {
            return new UInt24(value);
        }

        public static explicit operator UInt24(int value)
        {
            return new UInt24((uint)value);
        }

        public static implicit operator uint(UInt24 value)
        {
            return value.ToUInt32();
        }

        public static implicit operator ulong(UInt24 value)
        {
            return value.ToUInt32();
        }

        public static bool operator ==(UInt24 left, UInt24 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UInt24 left, UInt24 right)
        {
            return !(left == right);
        }

        public static void WriteLittleEndian(Span<byte> destination, UInt24 value)
        {
            destination[0] = value.byte1;
            destination[1] = value.byte2;
            destination[2] = value.byte3;
        }

        public static UInt24 ReadLittleEndian(ReadOnlySpan<byte> source)
        {
            return new UInt24((uint)(source[0] | (source[1] << 8) | (source[2] << 16)));
        }

        public static void WriteBigEndian(Span<byte> destination, UInt24 value)
        {
            destination[0] = value.byte3;
            destination[1] = value.byte2;
            destination[2] = value.byte1;
        }

        public static UInt24 ReadBigEndian(ReadOnlySpan<byte> source)
        {
            return new UInt24((uint)(source[2] | (source[1] << 8) | (source[0] << 16)));
        }
    }
}