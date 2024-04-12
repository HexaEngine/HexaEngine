namespace HexaEngine.Audio.Common.Flac
{
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;
    using System;
    using System.Buffers.Binary;
    using System.IO;

    public class BitWriter : IDisposable
    {
        private readonly bool leaveOpen;
        private readonly Stream baseStream;
        private readonly byte[] buffer;
        private readonly int bufferLength;
        private int bufferPosition;
        private long bitPosition;
        private bool disposedValue;

        public BitWriter(Stream baseStream)
        {
            this.baseStream = baseStream;
            leaveOpen = false;
            buffer = new byte[8192];
            bufferLength = buffer.Length << 3;
            bitPosition = baseStream.Position << 3;
        }

        public BitWriter(Stream baseStream, bool leaveOpen)
        {
            this.baseStream = baseStream;
            this.leaveOpen = leaveOpen;
            buffer = new byte[8192];
            bufferLength = buffer.Length << 3;
            bitPosition = baseStream.Position << 3;
        }

        /// <summary>
        /// Gets or sets the byte position of the stream. <see langword="Warning"/> setting this will force a flush.
        /// </summary>
        public long BytePosition
        {
            get => bitPosition >> 3;
            set
            {
                Flush();
                baseStream.Position = value;
                bitPosition = value << 3;
            }
        }

        /// <summary>
        /// Gets or sets the bit position of the stream. <see langword="Warning"/> setting this will force a flush, if it's not the same byte in the streams position.
        /// </summary>
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
                Flush();
                baseStream.Position = bytePos;
            }
        }

        public long ByteLength => baseStream.Length;

        public long BitLength => baseStream.Length << 3;

        public Stream BaseStream => baseStream;

        private void BufferData(int bits)
        {
            if (bufferPosition + bits >= bufferLength)
            {
                var byteCount = bufferPosition >> 3;
                var bitCount = bufferPosition & 7;
                baseStream.Write(buffer, 0, byteCount);
                bufferPosition = bitCount;
                buffer[0] = buffer[byteCount + 1];
            }
        }

        /// <summary>
        /// Flush will add padding to unfinished bytes.
        /// </summary>
        public void Flush()
        {
            if ((bufferPosition & 7) == 0)
            {
                baseStream.Write(buffer, 0, bufferPosition >> 3);
            }
            else
            {
                baseStream.Write(buffer, 0, (bufferPosition >> 3) + 1);
            }

            bufferPosition = 0;
        }

        public void Write(ReadOnlySpan<byte> span)
        {
            if ((bufferPosition & 7) == 0)
            {
                int byteIndex = bufferPosition >> 3;
                int buffLen = (bufferLength >> 3) - byteIndex;
                int delta = buffer.Length - buffLen;
                int min = Math.Min(buffer.Length, buffLen);

                for (int i = 0; i < min; i++)
                {
                    buffer[byteIndex++] = span[i];
                }
                bufferPosition += min << 3;

                if (delta > 0)
                {
                    Flush();
                    baseStream.Write(span.Slice(min, delta));
                }

                bitPosition += span.Length << 3;
                return;
            }

            for (int i = 0; i < span.Length; i++)
            {
                WriteByte(span[i]);
            }
        }

        public void WriteRawUInt8(byte value, int bits)
        {
            if (bits > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(bits), $"Bits must be smaller equals to 8 but was {bits}");
            }

            BufferData(bits);

            if (bits == 8 && (bufferPosition & 7) == 0)
            {
                buffer[bufferPosition >> 3] = value;
                bufferPosition += 8;
                return;
            }

            for (int i = 0; i < bits; i++, bufferPosition++)
            {
                var byteIndex = bufferPosition >> 3;
                var bitIndex = bufferPosition & 7;
                var ix = bits - 1 - i;
                buffer[byteIndex] |= (byte)(((value & (1 << ix)) >> ix) << (7 - bitIndex));
            }

            bitPosition += bits;
        }

        public void WriteRawUInt16(ushort value, int bits)
        {
            if (bits > 16)
            {
                throw new ArgumentOutOfRangeException(nameof(bits), $"Bits must be smaller equals to 16 but was {bits}");
            }

            BufferData(bits);

            if (bits == 16 && (bufferPosition & 7) == 0)
            {
                var byteIndex = bufferPosition >> 3;
                buffer[byteIndex] = (byte)(value >> 8);
                buffer[byteIndex + 1] = (byte)(value & 0xFF);
                bufferPosition += 16;
                return;
            }

            for (int i = 0; i < bits; i++, bufferPosition++)
            {
                var byteIndex = bufferPosition >> 3;
                var bitIndex = bufferPosition & 7;
                var ix = bits - 1 - i;
                buffer[byteIndex] |= (byte)(((value & (1 << ix)) >> ix) << (7 - bitIndex));
            }

            bitPosition += bits;
        }

        public void WriteRawUInt32(uint value, int bits)
        {
            if (bits > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(bits), $"Bits must be smaller equals to 32 but was {bits}");
            }

            BufferData(bits);

            if (bits == 32 && (bufferPosition & 7) == 0)
            {
                var byteIndex = bufferPosition >> 3;
                buffer[byteIndex] = (byte)(value >> 24);
                buffer[byteIndex + 1] = (byte)((value >> 16) & 0xFF);
                buffer[byteIndex + 2] = (byte)((value >> 8) & 0xFF);
                buffer[byteIndex + 3] = (byte)(value & 0xFF);
                bufferPosition += 32;
                return;
            }

            for (int i = 0; i < bits; i++, bufferPosition++)
            {
                var byteIndex = bufferPosition >> 3;
                var bitIndex = bufferPosition & 7;
                var ix = bits - 1 - i;
                buffer[byteIndex] |= (byte)(((value & (1 << ix)) >> ix) << (7 - bitIndex));
            }

            bitPosition += bits;
        }

        public void WriteRawUInt64(ulong value, int bits)
        {
            if (bits > 64)
            {
                throw new ArgumentOutOfRangeException(nameof(bits), $"Bits must be smaller equals to 64 but was {bits}");
            }

            BufferData(bits);

            if (bits == 64 && (bufferPosition & 7) == 0)
            {
                var byteIndex = bufferPosition >> 3;
                buffer[byteIndex] = (byte)(value >> 56);
                buffer[byteIndex + 1] = (byte)((value >> 48) & 0xFF);
                buffer[byteIndex + 2] = (byte)((value >> 40) & 0xFF);
                buffer[byteIndex + 3] = (byte)((value >> 32) & 0xFF);
                buffer[byteIndex + 4] = (byte)((value >> 24) & 0xFF);
                buffer[byteIndex + 5] = (byte)((value >> 16) & 0xFF);
                buffer[byteIndex + 6] = (byte)((value >> 8) & 0xFF);
                buffer[byteIndex + 7] = (byte)(value & 0xFF);
                bufferPosition += 64;
                return;
            }

            for (int i = 0; i < bits; i++, bufferPosition++)
            {
                var byteIndex = bufferPosition >> 3;
                var bitIndex = bufferPosition & 7;
                var ix = bits - 1 - i;
                buffer[byteIndex] |= (byte)(((value & (1UL << ix)) >> ix) << (7 - bitIndex));
            }

            bitPosition += bits;
        }

        public void WriteBool(bool value)
        {
            WriteRawUInt8((byte)(value ? 1 : 0), 1);
        }

        public void WriteByte(byte value)
        {
            WriteRawUInt8(value, 8);
        }

        public void WriteUInt16BigEndian(ushort value)
        {
            Span<byte> buffer = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
            Write(buffer);
        }

        public void WriteUInt16LittleEndian(ushort value)
        {
            Span<byte> buffer = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
            Write(buffer);
        }

        public void WriteUInt24BigEndian(UInt24 value)
        {
            Span<byte> buffer = stackalloc byte[3];
            UInt24.WriteBigEndian(buffer, value);
            Write(buffer);
        }

        public void WriteUInt24LittleEndian(UInt24 value)
        {
            Span<byte> buffer = stackalloc byte[3];
            UInt24.WriteLittleEndian(buffer, value);
            Write(buffer);
        }

        public void WriteUInt32BigEndian(uint value)
        {
            Span<byte> buffer = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
            Write(buffer);
        }

        public void WriteUInt32LittleEndian(uint value)
        {
            Span<byte> buffer = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
            Write(buffer);
        }

        public void WriteUInt64BigEndian(ulong value)
        {
            Span<byte> buffer = stackalloc byte[8];
            BinaryPrimitives.WriteUInt64BigEndian(buffer, value);
            Write(buffer);
        }

        public void WriteUInt64LittleEndian(ulong value)
        {
            Span<byte> buffer = stackalloc byte[8];
            BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
            Write(buffer);
        }

        public void WriteUInt128BigEndian(UInt128 value)
        {
            Span<byte> buffer = stackalloc byte[16];
            BinaryPrimitives.WriteUInt128BigEndian(buffer, value);
            Write(buffer);
        }

        public void WriteUInt128LittleEndian(UInt128 value)
        {
            Span<byte> buffer = stackalloc byte[16];
            BinaryPrimitives.WriteUInt128LittleEndian(buffer, value);
            Write(buffer);
        }

        public void WriteInt16BigEndian(short value)
        {
            Span<byte> buffer = stackalloc byte[2];
            BinaryPrimitives.WriteInt16BigEndian(buffer, value);
            Write(buffer);
        }

        public void WriteInt16LittleEndian(short value)
        {
            Span<byte> buffer = stackalloc byte[2];
            BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
            Write(buffer);
        }

        public void WriteInt24BigEndian(Int24 value)
        {
            Span<byte> buffer = stackalloc byte[3];
            Int24.WriteBigEndian(buffer, value);
            Write(buffer);
        }

        public void WriteInt24LittleEndian(Int24 value)
        {
            Span<byte> buffer = stackalloc byte[3];
            Int24.WriteLittleEndian(buffer, value);
            Write(buffer);
        }

        public void WriteInt32BigEndian(int value)
        {
            Span<byte> buffer = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(buffer, value);
            Write(buffer);
        }

        public void WriteInt32LittleEndian(int value)
        {
            Span<byte> buffer = stackalloc byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
            Write(buffer);
        }

        public void WriteInt64BigEndian(long value)
        {
            Span<byte> buffer = stackalloc byte[8];
            BinaryPrimitives.WriteInt64BigEndian(buffer, value);
            Write(buffer);
        }

        public void WriteInt64LittleEndian(long value)
        {
            Span<byte> buffer = stackalloc byte[8];
            BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
            Write(buffer);
        }

        public void WriteInt128BigEndian(Int128 value)
        {
            Span<byte> buffer = stackalloc byte[16];
            BinaryPrimitives.WriteInt128BigEndian(buffer, value);
            Write(buffer);
        }

        public void WriteInt128LittleEndian(Int128 value)
        {
            Span<byte> buffer = stackalloc byte[16];
            BinaryPrimitives.WriteInt128LittleEndian(buffer, value);
            Write(buffer);
        }

        public void WriteStdString(StdString str, Endianness endianness)
        {
            if (endianness == Endianness.LittleEndian)
            {
                WriteUInt32LittleEndian((uint)str.Size);
            }
            else
            {
                WriteUInt32BigEndian((uint)str.Size);
            }

            Write(str);
        }

        public void Close()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Flush();
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