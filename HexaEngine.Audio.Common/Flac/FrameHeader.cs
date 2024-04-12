namespace HexaEngine.Audio.Common.Flac
{
    using System;

    public enum FrameNumberType
    {
        FrameNumber,
        SampleNumber,
    }

    public struct FrameHeader
    {
        internal const ushort SyncCodeConst = 0b11111111111110;

        public ushort SyncCode;
        public BlockingStrategy BlockingStrategy;
        public byte BlockSize;
        public SampleRate SampleRate;
        public ChannelAssignment ChannelAssignment;
        public ushort SampleSizeInBits;
        public FrameNumberType NumberType;
        public FrameHeaderNumber Number;
        public byte CRC8;

        public struct FrameHeaderNumber
        {
            public uint FrameNumber;
            public ulong SampleNumber;
        }

        public unsafe void Read(BitReader br, StreamInfo info)
        {
            SyncCode = br.ReadRawUInt16(14);

            if (SyncCode != SyncCodeConst)
            {
                throw new FormatException($"Sync code must be {SyncCodeConst} but was {SyncCode}");
            }

            br.Skip(1);

            BlockingStrategy = (BlockingStrategy)br.ReadRawUInt8(1);

            BlockSize = br.ReadRawUInt8(4);
            SampleRate = (SampleRate)br.ReadRawUInt8(4);
            ChannelAssignment = (ChannelAssignment)br.ReadRawUInt8(4);
            var sampleSizeInBits = (SampleSize)br.ReadRawUInt8(3);

            SampleSizeInBits = sampleSizeInBits switch
            {
                SampleSize.GetFromStreamInfo => info.BitsPerSample,
                SampleSize.Sample8Bit => 8,
                SampleSize.Sample12Bit => 12,
                SampleSize.Reserved => throw new NotImplementedException(),
                SampleSize.Sample16Bit => 16,
                SampleSize.Sample20Bit => 20,
                SampleSize.Sample24Bit => 24,
                SampleSize.Sample32Bit => 32,
                _ => throw new NotImplementedException(),
            };

            if (BlockingStrategy == BlockingStrategy.VariableBlocksizeStream)
            {
                // <8-56>:"UTF-8" coded sample number (decoded number is 36 bits)
            }
            else
            {
                FLAC__bitreader_read_utf8_uint32(br, ref Number.FrameNumber, null, null);
                // <8-48>:"UTF-8" coded frame number (decoded number is 31 bits)
            }

            if ((BlockSize & 0x6) != 0)
            {
                // 8/16 bit (blocksize-1)
            }

            if (((byte)SampleRate & 0xC) != 0)
            {
                // 8/16 bit sample rate
            }

            CRC8 = br.ReadByte();
        }

        private static unsafe bool FLAC__bitreader_read_utf8_uint32(BitReader br, ref uint val, byte* raw, uint* rawlen)
        {
            uint v = 0;
            uint x = 0;
            uint i;

            if (br.ReadRawUInt32(ref x, 8))
            {
                return false;
            }

            if (raw != null)
            {
                raw[(*rawlen)++] = (byte)x;
            }

            if ((x & 0x80) == 0)
            { /* 0xxxxxxx */
                v = x;
                i = 0;
            }
            else if ((x & 0xC0) != 0 && (x & 0x20) == 0)
            { /* 110xxxxx */
                v = x & 0x1F;
                i = 1;
            }
            else if ((x & 0xE0) != 0 && (x & 0x10) == 0)
            { /* 1110xxxx */
                v = x & 0x0F;
                i = 2;
            }
            else if ((x & 0xF0) != 0 && (x & 0x08) == 0)
            { /* 11110xxx */
                v = x & 0x07;
                i = 3;
            }
            else if ((x & 0xF8) != 0 && (x & 0x04) == 0)
            { /* 111110xx */
                v = x & 0x03;
                i = 4;
            }
            else if ((x & 0xFC) != 0 && (x & 0x02) == 0)
            { /* 1111110x */
                v = x & 0x01;
                i = 5;
            }
            else
            {
                val = 0xffffffff;
                return true;
            }
            for (; i != 0; i--)
            {
                if (!br.ReadRawUInt32(ref x, 8))
                {
                    return false;
                }

                if (raw != null)
                {
                    raw[(*rawlen)++] = (byte)x;
                }

                if ((x & 0x80) == 0 || (x & 0x40) != 0)
                { /* 10xxxxxx */
                    val = 0xffffffff;
                    return true;
                }
                v <<= 6;
                v |= (x & 0x3F);
            }
            val = v;
            return true;
        }

        private unsafe bool FLAC__bitreader_read_utf8_uint64(BitReader br, ref ulong val, byte* raw, uint* rawlen)
        {
            ulong v = 0;
            uint x = 0;
            uint i;

            if (!br.ReadRawUInt32(ref x, 8))
            {
                return false;
            }

            if (raw != null)
            {
                raw[(*rawlen)++] = (byte)x;
            }

            if ((x & 0x80) != 0)
            { /* 0xxxxxxx */
                v = x;
                i = 0;
            }
            else if ((x & 0xC0) != 0 && (x & 0x20) == 0)
            { /* 110xxxxx */
                v = x & 0x1F;
                i = 1;
            }
            else if ((x & 0xE0) != 0 && (x & 0x10) == 0)
            { /* 1110xxxx */
                v = x & 0x0F;
                i = 2;
            }
            else if ((x & 0xF0) != 0 && (x & 0x08) == 0)
            { /* 11110xxx */
                v = x & 0x07;
                i = 3;
            }
            else if ((x & 0xF8) != 0 && (x & 0x04) == 0)
            { /* 111110xx */
                v = x & 0x03;
                i = 4;
            }
            else if ((x & 0xFC) != 0 && (x & 0x02) == 0)
            { /* 1111110x */
                v = x & 0x01;
                i = 5;
            }
            else if ((x & 0xFE) != 0 && (x & 0x01) == 0)
            { /* 11111110 */
                v = 0;
                i = 6;
            }
            else
            {
                val = 0xffffffffffffffff;
                return true;
            }
            for (; i != 0; i--)
            {
                if (!br.ReadRawUInt32(ref x, 8))
                {
                    return false;
                }

                if (raw != null)
                {
                    raw[(*rawlen)++] = (byte)x;
                }

                if ((x & 0x80) == 0 || (x & 0x40) != 0)
                { /* 10xxxxxx */
                    val = 0xffffffffffffffff;
                    return true;
                }
                v <<= 6;
                v |= (x & 0x3F);
            }
            val = v;
            return true;
        }
    }
}