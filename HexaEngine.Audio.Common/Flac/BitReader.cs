namespace HexaEngine.Audio.Common.Flac
{
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;
    using System;
    using System.Buffers.Binary;
    using System.IO;

    public class BitReader : IDisposable
    {
        private readonly bool leaveOpen;
        private readonly Stream baseStream;
        private readonly byte[] buffer;
        private int bufferLength;
        private int bufferPosition;
        private bool dirty = true;
        private long bitPosition;
        private bool disposedValue;
        private int readLimit;

        public BitReader(Stream baseStream)
        {
            this.baseStream = baseStream;
            leaveOpen = false;
            buffer = new byte[8192];
            bitPosition = baseStream.Position << 3;
        }

        public BitReader(Stream baseStream, bool leaveOpen)
        {
            this.baseStream = baseStream;
            this.leaveOpen = leaveOpen;
            buffer = new byte[8192];
            bitPosition = baseStream.Position << 3;
        }

        public long BytePosition
        {
            get => bitPosition / 8;
            set
            {
                baseStream.Position = value;
                bitPosition = value * 8;
                dirty = true;
            }
        }

        public long BitPosition
        {
            get => bitPosition;
            set
            {
                bitPosition = value;
                var bytePos = value >> 3;
                if (baseStream.Position == bytePos)
                {
                    return;
                }
                baseStream.Position = bytePos;
                dirty = true;
            }
        }

        public long ByteLength => baseStream.Length;

        public long BitLength => baseStream.Length * 8;

        public int ReadLimit { get => readLimit; set => readLimit = value; }

        public Stream BaseStream => baseStream;

        public bool EndOfStream => ByteLength == BytePosition;

        public void Skip(int n)
        {
            BufferData(n);
            bufferPosition += n;
            bitPosition += n;
        }

        private void BufferData(int bits)
        {
            if (dirty)
            {
                bufferLength = baseStream.Read(buffer) * 8;
                bufferPosition = 0;
                dirty = false;
                return;
            }

            if (bufferPosition + bits > bufferLength)
            {
                int start = bufferPosition >> 3;
                int end = bufferLength >> 3;
                int left = end - start;

                for (int i = start, j = 0; i < end; i++, j++)
                {
                    buffer[j] = buffer[i];
                }

                bufferLength = baseStream.Read(buffer, left, buffer.Length - left) << 3;
                bufferPosition &= 7;
                return;
            }
        }

        public void Read(Span<byte> span)
        {
            if ((bufferPosition & 7) == 0)
            {
                // If the current bit position is aligned with byte boundary, we can optimize the reading.
                int byteIndex = bufferPosition >> 3;
                int buffLen = (bufferLength >> 3) - byteIndex;
                int delta = span.Length - buffLen;
                int min = Math.Min(span.Length, buffLen);
                for (int i = 0; i < min; i++)
                {
                    span[i] = buffer[byteIndex++];
                }
                if (delta > 0)
                {
                    var sp = span[min..];
                    baseStream.Read(sp);
                }
                bufferPosition += min << 3;
                bitPosition += span.Length << 3;
                return;
            }

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = ReadByte();
            }
        }

        public byte ReadRawUInt8(int bits)
        {
            if (bits > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(bits), $"Bits must be smaller equals to 8 but was {bits}");
            }

            BufferData(bits);
            bitPosition += bits;
            byte result = 0;

            if (bits == 8 && (bufferPosition & 7) == 0)
            {
                int byteIndex = bufferPosition >> 3;
                result = buffer[byteIndex];
                bufferPosition += 8;
                return result;
            }

            for (int i = 0; i < bits; i++, bufferPosition++)
            {
                int byteIndex = bufferPosition >> 3;
                int bitIndex = bufferPosition & 7;
                result |= (byte)((byte)((byte)(buffer[byteIndex] & (byte)(1 << (7 - bitIndex))) >> (byte)(7 - bitIndex)) << (bits - 1 - i));
            }

            return result;
        }

        public ushort ReadRawUInt16(int bits)
        {
            if (bits > 16)
            {
                throw new ArgumentOutOfRangeException(nameof(bits), $"Bits must be smaller equals to 16 but was {bits}");
            }

            BufferData(bits);
            bitPosition += bits;
            ushort result = 0;

            if ((bits & 7) == 0 && (bufferPosition & 7) == 0)
            {
                int byteIndex = bufferPosition >> 3;
                int byteCount = bits >> 3;
                for (int i = 0; i < byteCount; i++)
                {
                    result |= (ushort)(buffer[byteIndex++] << (byte)(8 * (byteCount - i)));
                }
                bufferPosition += bits;
                return result;
            }

            for (int i = 0; i < bits; i++, bufferPosition++)
            {
                int byteIndex = bufferPosition >> 3;
                int bitIndex = bufferPosition & 7;
                result |= (ushort)((ushort)((byte)(buffer[byteIndex] & (byte)(1 << (7 - bitIndex))) >> (byte)(7 - bitIndex)) << (bits - 1 - i));
            }

            return result;
        }

        public uint ReadRawUInt32(int bits)
        {
            if (bits > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(bits), $"Bits must be smaller equals to 32 but was {bits}");
            }

            BufferData(bits);
            bitPosition += bits;
            uint result = 0;

            if ((bits & 7) == 0 && (bufferPosition & 7) == 0)
            {
                int byteIndex = bufferPosition >> 3;
                int byteCount = bits >> 3;
                for (int i = 0; i < byteCount; i++)
                {
                    result |= (uint)buffer[byteIndex++] << (byte)(8 * ((byteCount - i) - 1));
                }
                bufferPosition += bits;
                return result;
            }

            for (int i = 0; i < bits; i++, bufferPosition++)
            {
                int byteIndex = bufferPosition >> 3;
                byte bitIndex = (byte)(bufferPosition & 7);
                result |= (uint)((byte)(buffer[byteIndex] & (byte)(1 << (7 - bitIndex))) >> (byte)(7 - bitIndex)) << (bits - 1 - i);
            }

            return result;
        }

        public ulong ReadRawUInt64(int bits)
        {
            if (bits > 64)
            {
                throw new ArgumentOutOfRangeException(nameof(bits), $"Bits must be smaller equals to 64 but was {bits}");
            }

            BufferData(bits);
            bitPosition += bits;
            ulong result = 0;

            if ((bits & 7) == 0 && (bufferPosition & 7) == 0)
            {
                int byteIndex = bufferPosition >> 3;
                int byteCount = bits >> 3;
                for (int i = 0; i < byteCount; i++)
                {
                    result |= (ulong)buffer[byteIndex++] << (byte)((byteCount - i) << 3);
                }
                bufferPosition += bits;
                return result;
            }

            for (int i = 0; i < bits; i++, bufferPosition++)
            {
                int byteIndex = bufferPosition >> 3;
                int bitIndex = bufferPosition & 7;
                result |= (ulong)((byte)(buffer[byteIndex] & (byte)(1 << (7 - bitIndex))) >> (byte)(7 - bitIndex)) << (bits - 1 - i);
            }

            return result;
        }

        public bool ReadRawUInt8(ref byte result, int bits)
        {
            if (bits == 0)
            {
                result = 0;
                return true;
            }

            if (readLimit != -1)
            {
                if (readLimit < bits)
                {
                    readLimit = -1;
                    return false;
                }
                else
                {
                    readLimit -= bits;
                }
            }

            if (bits + bitPosition > BitLength)
            {
                return false;
            }

            result = ReadRawUInt8(bits);

            return true;
        }

        public bool ReadRawUInt16(ref ushort result, int bits)
        {
            if (bits == 0)
            {
                result = 0;
                return true;
            }

            if (readLimit != -1)
            {
                if (readLimit < bits)
                {
                    readLimit = -1;
                    return false;
                }
                else
                {
                    readLimit -= bits;
                }
            }

            if (bits + bitPosition > BitLength)
            {
                return false;
            }

            result = ReadRawUInt16(bits);

            return true;
        }

        public bool ReadRawUInt32(ref uint result, int bits)
        {
            if (bits == 0)
            {
                result = 0;
                return true;
            }

            if (readLimit != -1)
            {
                if (readLimit < bits)
                {
                    readLimit = -1;
                    return false;
                }
                else
                {
                    readLimit -= bits;
                }
            }

            if (bits + bitPosition > BitLength)
            {
                return false;
            }

            result = ReadRawUInt32(bits);

            return true;
        }

        public bool ReadRawUInt64(ref ulong result, int bits)
        {
            if (bits == 0)
            {
                result = 0;
                return true;
            }

            if (readLimit != -1)
            {
                if (readLimit < bits)
                {
                    readLimit = -1;
                    return false;
                }
                else
                {
                    readLimit -= bits;
                }
            }

            if (bits + bitPosition > BitLength)
            {
                return false;
            }

            result = ReadRawUInt64(bits);

            return true;
        }

        public bool ReadBool()
        {
            return ReadRawUInt8(1) != 0;
        }

        public byte ReadByte()
        {
            return ReadRawUInt8(8);
        }

        public ushort ReadUInt16BigEndian()
        {
            Span<byte> stackBuffer = stackalloc byte[2];
            Read(stackBuffer);
            return BinaryPrimitives.ReadUInt16BigEndian(stackBuffer);
        }

        public ushort ReadUInt16LittleEndian()
        {
            Span<byte> stackBuffer = stackalloc byte[2];
            Read(stackBuffer);
            return BinaryPrimitives.ReadUInt16LittleEndian(stackBuffer);
        }

        public UInt24 ReadUInt24BigEndian()
        {
            Span<byte> stackBuffer = stackalloc byte[3];
            Read(stackBuffer);
            return UInt24.ReadBigEndian(stackBuffer);
        }

        public UInt24 ReadUInt24LittleEndian()
        {
            Span<byte> stackBuffer = stackalloc byte[3];
            Read(stackBuffer);
            return UInt24.ReadLittleEndian(stackBuffer);
        }

        public uint ReadUInt32BigEndian()
        {
            Span<byte> stackBuffer = stackalloc byte[4];
            Read(stackBuffer);
            return BinaryPrimitives.ReadUInt32BigEndian(stackBuffer);
        }

        public uint ReadUInt32LittleEndian()
        {
            Span<byte> stackBuffer = stackalloc byte[4];
            Read(stackBuffer);
            return BinaryPrimitives.ReadUInt32LittleEndian(stackBuffer);
        }

        public ulong ReadUInt64BigEndian()
        {
            Span<byte> stackBuffer = stackalloc byte[8];
            Read(stackBuffer);
            return BinaryPrimitives.ReadUInt64BigEndian(stackBuffer);
        }

        public ulong ReadUInt64LittleEndian()
        {
            Span<byte> stackBuffer = stackalloc byte[8];
            Read(stackBuffer);
            return BinaryPrimitives.ReadUInt64LittleEndian(stackBuffer);
        }

        public UInt128 ReadUInt128BigEndian()
        {
            Span<byte> stackBuffer = stackalloc byte[16];
            Read(stackBuffer);
            return BinaryPrimitives.ReadUInt128BigEndian(stackBuffer);
        }

        public UInt128 ReadUInt128LittleEndian()
        {
            Span<byte> stackBuffer = stackalloc byte[16];
            Read(stackBuffer);
            return BinaryPrimitives.ReadUInt128LittleEndian(stackBuffer);
        }

        public short ReadInt16BigEndian()
        {
            Span<byte> stackBuffer = stackalloc byte[2];
            Read(stackBuffer);
            return BinaryPrimitives.ReadInt16BigEndian(stackBuffer);
        }

        public short ReadInt16LittleEndian()
        {
            Span<byte> stackBuffer = stackalloc byte[2];
            Read(stackBuffer);
            return BinaryPrimitives.ReadInt16LittleEndian(stackBuffer);
        }

        public Int24 ReadInt24BigEndian()
        {
            Span<byte> stackBuffer = stackalloc byte[3];
            Read(stackBuffer);
            return Int24.ReadBigEndian(stackBuffer);
        }

        public Int24 ReadInt24LittleEndian()
        {
            Span<byte> stackBuffer = stackalloc byte[3];
            Read(stackBuffer);
            return Int24.ReadLittleEndian(stackBuffer);
        }

        public int ReadInt32BigEndian()
        {
            Span<byte> stackBuffer = stackalloc byte[4];
            Read(stackBuffer);
            return BinaryPrimitives.ReadInt32BigEndian(stackBuffer);
        }

        public int ReadInt32LittleEndian()
        {
            Span<byte> stackBuffer = stackalloc byte[4];
            Read(stackBuffer);
            return BinaryPrimitives.ReadInt32LittleEndian(stackBuffer);
        }

        public long ReadInt64BigEndian()
        {
            Span<byte> stackBuffer = stackalloc byte[8];
            Read(stackBuffer);
            return BinaryPrimitives.ReadInt64BigEndian(stackBuffer);
        }

        public long ReadInt64LittleEndian()
        {
            Span<byte> stackBuffer = stackalloc byte[8];
            Read(stackBuffer);
            return BinaryPrimitives.ReadInt64LittleEndian(stackBuffer);
        }

        public Int128 ReadInt128BigEndian()
        {
            Span<byte> stackBuffer = stackalloc byte[16];
            Read(stackBuffer);
            return BinaryPrimitives.ReadInt128BigEndian(stackBuffer);
        }

        public Int128 ReadInt128LittleEndian()
        {
            Span<byte> stackBuffer = stackalloc byte[16];
            Read(stackBuffer);
            return BinaryPrimitives.ReadInt128LittleEndian(stackBuffer);
        }

        public StdString ReadStdString(Endianness endianness)
        {
            uint length;
            if (endianness == Endianness.LittleEndian)
            {
                length = ReadUInt32LittleEndian();
            }
            else
            {
                length = ReadUInt32BigEndian();
            }

            StdString str = default;
            str.Resize((int)length);
            Read(str.AsSpan());
            return str;
        }

        public void Close()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (!leaveOpen)
                {
                    baseStream.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }
    }
}