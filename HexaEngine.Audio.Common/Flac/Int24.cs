namespace HexaEngine.Audio.Common.Flac
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Int24
    {
        private readonly byte byte1;
        private readonly byte byte2;
        private readonly byte byte3;

        public Int24(int value)
        {
            if (value < MinValue || value > MaxValue)
            {
                throw new OverflowException("Value is out of range for Int24.");
            }

            byte1 = (byte)((value >> 0) & 0xFF);
            byte2 = (byte)((value >> 8) & 0xFF);
            byte3 = (byte)((value >> 16) & 0xFF);
        }

        public static readonly int MaxValue = 0x007FFFFF;
        public static readonly int MinValue = unchecked((int)0xFF800000);

        public readonly int ToInt32()
        {
            int value = byte1 | (byte2 << 8) | (byte3 << 16);
            if ((value & 0x00800000) != 0)
            {
                value |= unchecked((int)0xFF000000);
            }
            return value;
        }

        public uint ToUInt32()
        {
            return (uint)(byte1 | (byte2 << 8) | (byte3 << 16));
        }

        public static Int24 operator +(Int24 a, byte b)
        {
            return (Int24)(a.ToInt32() + b);
        }

        public static Int24 operator -(Int24 a, byte b)
        {
            return (Int24)(a.ToInt32() - b);
        }

        public static Int24 operator *(Int24 a, byte b)
        {
            return (Int24)(a.ToInt32() * b);
        }

        public static Int24 operator /(Int24 a, byte b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException("Division by zero in Int24.");
            }
            return (Int24)(a.ToInt32() / b);
        }

        public static Int24 operator %(Int24 a, byte b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException("Division by zero in Int24.");
            }
            return (Int24)(a.ToInt32() % b);
        }

        public static Int24 operator &(Int24 a, byte b)
        {
            return (Int24)(a.ToInt32() & b);
        }

        public static Int24 operator |(Int24 a, byte b)
        {
            return (Int24)(a.ToInt32() | b);
        }

        public static Int24 operator ^(Int24 a, byte b)
        {
            return (Int24)(a.ToInt32() ^ b);
        }

        public static Int24 operator >>(Int24 a, byte b)
        {
            return (Int24)(a.ToInt32() >> b);
        }

        public static Int24 operator <<(Int24 a, byte b)
        {
            return (Int24)(a.ToInt32() << b);
        }

        public static Int24 operator >>>(Int24 a, byte b)
        {
            return (Int24)(a.ToInt32() >>> b);
        }

        public static Int24 operator +(Int24 a, short b)
        {
            return (Int24)(a.ToInt32() + b);
        }

        public static Int24 operator -(Int24 a, short b)
        {
            return (Int24)(a.ToInt32() - b);
        }

        public static Int24 operator *(Int24 a, short b)
        {
            return (Int24)(a.ToInt32() * b);
        }

        public static Int24 operator /(Int24 a, short b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException("Division by zero in Int24.");
            }
            return (Int24)(a.ToInt32() / b);
        }

        public static Int24 operator %(Int24 a, short b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException("Division by zero in Int24.");
            }
            return (Int24)(a.ToInt32() % b);
        }

        public static Int24 operator &(Int24 a, short b)
        {
            return (Int24)(a.ToInt32() & b);
        }

        public static Int24 operator |(Int24 a, short b)
        {
            return (Int24)(a.ToInt32() | b);
        }

        public static Int24 operator ^(Int24 a, short b)
        {
            return (Int24)(a.ToInt32() ^ b);
        }

        public static Int24 operator >>(Int24 a, short b)
        {
            return (Int24)(a.ToInt32() >> b);
        }

        public static Int24 operator <<(Int24 a, short b)
        {
            return (Int24)(a.ToInt32() << b);
        }

        public static Int24 operator >>>(Int24 a, short b)
        {
            return (Int24)(a.ToInt32() >>> b);
        }

        public static Int24 operator +(Int24 a, Int24 b)
        {
            return new Int24(a.ToInt32() + b.ToInt32());
        }

        public static Int24 operator -(Int24 a, Int24 b)
        {
            return new Int24(a.ToInt32() - b.ToInt32());
        }

        public static Int24 operator *(Int24 a, Int24 b)
        {
            return new Int24(a.ToInt32() * b.ToInt32());
        }

        public static Int24 operator /(Int24 a, Int24 b)
        {
            if (b.ToInt32() == 0)
            {
                throw new DivideByZeroException("Division by zero in Int24.");
            }
            return new Int24(a.ToInt32() / b.ToInt32());
        }

        public static Int24 operator %(Int24 a, Int24 b)
        {
            if (b.ToInt32() == 0)
            {
                throw new DivideByZeroException("Division by zero in Int24.");
            }
            return new Int24(a.ToInt32() % b.ToInt32());
        }

        public static Int24 operator &(Int24 a, Int24 b)
        {
            return new Int24(a.ToInt32() & b.ToInt32());
        }

        public static Int24 operator |(Int24 a, Int24 b)
        {
            return new Int24(a.ToInt32() | b.ToInt32());
        }

        public static Int24 operator ^(Int24 a, Int24 b)
        {
            return new Int24(a.ToInt32() ^ b.ToInt32());
        }

        public static Int24 operator >>(Int24 a, Int24 b)
        {
            return new Int24(a.ToInt32() >> b.ToInt32());
        }

        public static Int24 operator <<(Int24 a, Int24 b)
        {
            return new Int24(a.ToInt32() << b.ToInt32());
        }

        public static Int24 operator >>>(Int24 a, Int24 b)
        {
            return new Int24(a.ToInt32() >>> b.ToInt32());
        }

        public static int operator +(Int24 a, int b)
        {
            return a.ToInt32() + b;
        }

        public static int operator -(Int24 a, int b)
        {
            return a.ToInt32() - b;
        }

        public static int operator *(Int24 a, int b)
        {
            return a.ToInt32() * b;
        }

        public static int operator /(Int24 a, int b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException("Division by zero in Int24.");
            }
            return a.ToInt32() / b;
        }

        public static int operator %(Int24 a, int b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException("Division by zero in Int24.");
            }
            return a.ToInt32() % b;
        }

        public static int operator &(Int24 a, int b)
        {
            return a.ToInt32() & b;
        }

        public static int operator |(Int24 a, int b)
        {
            return a.ToInt32() | b;
        }

        public static int operator ^(Int24 a, int b)
        {
            return a.ToInt32() ^ b;
        }

        public static int operator >>(Int24 a, int b)
        {
            return a.ToInt32() >> b;
        }

        public static int operator <<(Int24 a, int b)
        {
            return a.ToInt32() << b;
        }

        public static int operator >>>(Int24 a, int b)
        {
            return a.ToInt32() >>> b;
        }

        public static long operator +(Int24 a, long b)
        {
            return a.ToInt32() + b;
        }

        public static long operator -(Int24 a, long b)
        {
            return a.ToInt32() - b;
        }

        public static long operator *(Int24 a, long b)
        {
            return a.ToInt32() * b;
        }

        public static long operator /(Int24 a, long b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException("Division by zero in Int24.");
            }
            return a.ToInt32() / b;
        }

        public static long operator %(Int24 a, long b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException("Division by zero in Int24.");
            }
            return a.ToInt32() % b;
        }

        public static long operator &(Int24 a, long b)
        {
            return a.ToInt32() & b;
        }

        public static long operator |(Int24 a, long b)
        {
            return a.ToInt32() | b;
        }

        public static long operator ^(Int24 a, long b)
        {
            return a.ToInt32() ^ b;
        }

        public static Int24 operator ++(Int24 a)
        {
            return new Int24(a.ToInt32() + 1);
        }

        public static Int24 operator --(Int24 a)
        {
            return new Int24(a.ToInt32() - 1);
        }

        public static explicit operator Int24(int value)
        {
            return new Int24(value);
        }

        public static implicit operator int(Int24 value)
        {
            return value.ToInt32();
        }

        public static implicit operator uint(Int24 value)
        {
            return (uint)value.ToInt32();
        }

        public static explicit operator Int24(uint value)
        {
            if (value > MaxValue)
            {
                throw new OverflowException("Value is out of range for Int24.");
            }
            return new Int24((int)value);
        }

        public static void WriteLittleEndian(Span<byte> destination, Int24 value)
        {
            destination[0] = value.byte1;
            destination[1] = value.byte2;
            destination[2] = value.byte3;
        }

        public static Int24 ReadLittleEndian(ReadOnlySpan<byte> source)
        {
            return new Int24((int)(source[0] | (source[1] << 8) | (source[2] << 16)));
        }

        public static void WriteBigEndian(Span<byte> destination, Int24 value)
        {
            destination[0] = value.byte3;
            destination[1] = value.byte2;
            destination[2] = value.byte1;
        }

        public static Int24 ReadBigEndian(ReadOnlySpan<byte> source)
        {
            return new Int24((int)(source[2] | (source[1] << 8) | (source[0] << 16)));
        }
    }
}