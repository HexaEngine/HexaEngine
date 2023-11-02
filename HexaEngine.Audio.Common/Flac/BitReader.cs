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
            byte b0 = ReadRawUInt8(8);
            byte b1 = ReadRawUInt8(8);
            Span<byte> buffer = [b0, b1];
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }

        public ushort ReadUInt16LittleEndian()
        {
            byte b0 = ReadRawUInt8(8);
            byte b1 = ReadRawUInt8(8);
            Span<byte> buffer = [b0, b1];
            return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
        }

        public UInt24 ReadUInt24BigEndian()
        {
            byte b0 = ReadRawUInt8(8);
            byte b1 = ReadRawUInt8(8);
            byte b2 = ReadRawUInt8(8);
            Span<byte> buffer = [b0, b1, b2];
            return UInt24.ReadBigEndian(buffer);
        }

        public UInt24 ReadUInt24LittleEndian()
        {
            byte b0 = ReadRawUInt8(8);
            byte b1 = ReadRawUInt8(8);
            byte b2 = ReadRawUInt8(8);
            Span<byte> buffer = [b0, b1, b2];
            return UInt24.ReadLittleEndian(buffer);
        }

        public uint ReadUInt32BigEndian()
        {
            byte b0 = ReadRawUInt8(8);
            byte b1 = ReadRawUInt8(8);
            byte b2 = ReadRawUInt8(8);
            byte b3 = ReadRawUInt8(8);
            Span<byte> buffer = [b0, b1, b2, b3];
            return BinaryPrimitives.ReadUInt32BigEndian(buffer);
        }

        public uint ReadUInt32LittleEndian()
        {
            byte b0 = ReadRawUInt8(8);
            byte b1 = ReadRawUInt8(8);
            byte b2 = ReadRawUInt8(8);
            byte b3 = ReadRawUInt8(8);
            Span<byte> buffer = [b0, b1, b2, b3];
            return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
        }

        public ulong ReadUInt64BigEndian()
        {
            byte b0 = ReadRawUInt8(8);
            byte b1 = ReadRawUInt8(8);
            byte b2 = ReadRawUInt8(8);
            byte b3 = ReadRawUInt8(8);
            byte b4 = ReadRawUInt8(8);
            byte b5 = ReadRawUInt8(8);
            byte b6 = ReadRawUInt8(8);
            byte b7 = ReadRawUInt8(8);
            Span<byte> buffer = [b0, b1, b2, b3, b4, b5, b6, b7];
            return BinaryPrimitives.ReadUInt64BigEndian(buffer);
        }

        public ulong ReadUInt64LittleEndian()
        {
            byte b0 = ReadRawUInt8(8);
            byte b1 = ReadRawUInt8(8);
            byte b2 = ReadRawUInt8(8);
            byte b3 = ReadRawUInt8(8);
            byte b4 = ReadRawUInt8(8);
            byte b5 = ReadRawUInt8(8);
            byte b6 = ReadRawUInt8(8);
            byte b7 = ReadRawUInt8(8);
            Span<byte> buffer = [b0, b1, b2, b3, b4, b5, b6, b7];
            return BinaryPrimitives.ReadUInt64LittleEndian(buffer);
        }

        public UInt128 ReadUInt128BigEndian()
        {
            byte b0 = ReadRawUInt8(8);
            byte b1 = ReadRawUInt8(8);
            byte b2 = ReadRawUInt8(8);
            byte b3 = ReadRawUInt8(8);
            byte b4 = ReadRawUInt8(8);
            byte b5 = ReadRawUInt8(8);
            byte b6 = ReadRawUInt8(8);
            byte b7 = ReadRawUInt8(8);

            byte b8 = ReadRawUInt8(8);
            byte b9 = ReadRawUInt8(8);
            byte b10 = ReadRawUInt8(8);
            byte b11 = ReadRawUInt8(8);
            byte b12 = ReadRawUInt8(8);
            byte b13 = ReadRawUInt8(8);
            byte b14 = ReadRawUInt8(8);
            byte b15 = ReadRawUInt8(8);

            Span<byte> buffer = [b0, b1, b2, b3, b4, b5, b6, b7, b8, b9, b10, b11, b12, b13, b14, b15];
            return BinaryPrimitives.ReadUInt128BigEndian(buffer);
        }

        public UInt128 ReadUInt128LittleEndian()
        {
            byte b0 = ReadRawUInt8(8);
            byte b1 = ReadRawUInt8(8);
            byte b2 = ReadRawUInt8(8);
            byte b3 = ReadRawUInt8(8);
            byte b4 = ReadRawUInt8(8);
            byte b5 = ReadRawUInt8(8);
            byte b6 = ReadRawUInt8(8);
            byte b7 = ReadRawUInt8(8);

            byte b8 = ReadRawUInt8(8);
            byte b9 = ReadRawUInt8(8);
            byte b10 = ReadRawUInt8(8);
            byte b11 = ReadRawUInt8(8);
            byte b12 = ReadRawUInt8(8);
            byte b13 = ReadRawUInt8(8);
            byte b14 = ReadRawUInt8(8);
            byte b15 = ReadRawUInt8(8);

            Span<byte> buffer = [b0, b1, b2, b3, b4, b5, b6, b7, b8, b9, b10, b11, b12, b13, b14, b15];
            return BinaryPrimitives.ReadUInt128LittleEndian(buffer);
        }

        public short ReadInt16BigEndian()
        {
            byte b0 = ReadRawUInt8(8);
            byte b1 = ReadRawUInt8(8);
            Span<byte> buffer = [b0, b1];
            return BinaryPrimitives.ReadInt16BigEndian(buffer);
        }

        public short ReadInt16LittleEndian()
        {
            byte b0 = ReadRawUInt8(8);
            byte b1 = ReadRawUInt8(8);
            Span<byte> buffer = [b0, b1];
            return BinaryPrimitives.ReadInt16LittleEndian(buffer);
        }

        public Int24 ReadInt24BigEndian()
        {
            byte b0 = ReadRawUInt8(8);
            byte b1 = ReadRawUInt8(8);
            byte b2 = ReadRawUInt8(8);
            Span<byte> buffer = [b0, b1, b2];
            return Int24.ReadBigEndian(buffer);
        }

        public Int24 ReadInt24LittleEndian()
        {
            byte b0 = ReadRawUInt8(8);
            byte b1 = ReadRawUInt8(8);
            byte b2 = ReadRawUInt8(8);
            Span<byte> buffer = [b0, b1, b2];
            return Int24.ReadLittleEndian(buffer);
        }

        public int ReadInt32BigEndian()
        {
            byte b0 = ReadRawUInt8(8);
            byte b1 = ReadRawUInt8(8);
            byte b2 = ReadRawUInt8(8);
            byte b3 = ReadRawUInt8(8);
            Span<byte> buffer = [b0, b1, b2, b3];
            return BinaryPrimitives.ReadInt32BigEndian(buffer);
        }

        public int ReadInt32LittleEndian()
        {
            byte b0 = ReadRawUInt8(8);
            byte b1 = ReadRawUInt8(8);
            byte b2 = ReadRawUInt8(8);
            byte b3 = ReadRawUInt8(8);
            Span<byte> buffer = [b0, b1, b2, b3];
            return BinaryPrimitives.ReadInt32LittleEndian(buffer);
        }

        public long ReadInt64BigEndian()
        {
            byte b0 = ReadRawUInt8(8);
            byte b1 = ReadRawUInt8(8);
            byte b2 = ReadRawUInt8(8);
            byte b3 = ReadRawUInt8(8);
            byte b4 = ReadRawUInt8(8);
            byte b5 = ReadRawUInt8(8);
            byte b6 = ReadRawUInt8(8);
            byte b7 = ReadRawUInt8(8);
            Span<byte> buffer = [b0, b1, b2, b3, b4, b5, b6, b7];
            return BinaryPrimitives.ReadInt64BigEndian(buffer);
        }

        public long ReadInt64LittleEndian()
        {
            byte b0 = ReadRawUInt8(8);
            byte b1 = ReadRawUInt8(8);
            byte b2 = ReadRawUInt8(8);
            byte b3 = ReadRawUInt8(8);
            byte b4 = ReadRawUInt8(8);
            byte b5 = ReadRawUInt8(8);
            byte b6 = ReadRawUInt8(8);
            byte b7 = ReadRawUInt8(8);
            Span<byte> buffer = [b0, b1, b2, b3, b4, b5, b6, b7];
            return BinaryPrimitives.ReadInt64LittleEndian(buffer);
        }

        public Int128 ReadInt128BigEndian()
        {
            byte b0 = ReadRawUInt8(8);
            byte b1 = ReadRawUInt8(8);
            byte b2 = ReadRawUInt8(8);
            byte b3 = ReadRawUInt8(8);
            byte b4 = ReadRawUInt8(8);
            byte b5 = ReadRawUInt8(8);
            byte b6 = ReadRawUInt8(8);
            byte b7 = ReadRawUInt8(8);

            byte b8 = ReadRawUInt8(8);
            byte b9 = ReadRawUInt8(8);
            byte b10 = ReadRawUInt8(8);
            byte b11 = ReadRawUInt8(8);
            byte b12 = ReadRawUInt8(8);
            byte b13 = ReadRawUInt8(8);
            byte b14 = ReadRawUInt8(8);
            byte b15 = ReadRawUInt8(8);

            Span<byte> buffer = [b0, b1, b2, b3, b4, b5, b6, b7, b8, b9, b10, b11, b12, b13, b14, b15];
            return BinaryPrimitives.ReadInt128BigEndian(buffer);
        }

        public Int128 ReadInt128LittleEndian()
        {
            byte b0 = ReadRawUInt8(8);
            byte b1 = ReadRawUInt8(8);
            byte b2 = ReadRawUInt8(8);
            byte b3 = ReadRawUInt8(8);
            byte b4 = ReadRawUInt8(8);
            byte b5 = ReadRawUInt8(8);
            byte b6 = ReadRawUInt8(8);
            byte b7 = ReadRawUInt8(8);

            byte b8 = ReadRawUInt8(8);
            byte b9 = ReadRawUInt8(8);
            byte b10 = ReadRawUInt8(8);
            byte b11 = ReadRawUInt8(8);
            byte b12 = ReadRawUInt8(8);
            byte b13 = ReadRawUInt8(8);
            byte b14 = ReadRawUInt8(8);
            byte b15 = ReadRawUInt8(8);

            Span<byte> buffer = [b0, b1, b2, b3, b4, b5, b6, b7, b8, b9, b10, b11, b12, b13, b14, b15];
            return BinaryPrimitives.ReadInt128LittleEndian(buffer);
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