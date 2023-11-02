namespace HexaEngine.Audio.Common.Flac
{
    public struct Frame
    {
        public FrameHeader Header;
        public Subframe Subframe;
        public FrameFooter Footer;

        public void Read(BitReader br, StreamInfo info)
        {
            Header.Read(br, info);
            Subframe.Read(br, Header);
            Footer.Read(br);
        }
    }

    public struct FrameFooter
    {
        public ushort CRC16;

        public void Read(BitReader br)
        {
            CRC16 = br.ReadUInt16BigEndian();
        }
    }

    public struct Subframe
    {
        public SubframeHeader Header;

        public void Read(BitReader br, FrameHeader frameHeader)
        {
            Header.Read(br);
        }
    }

    public unsafe struct SubframeConstant
    {
        public ulong Data;

        public void Read(BitReader reader, FrameHeader frameHeader)
        {
            int bytes = frameHeader.SampleSizeInBits >> 3;

            Span<byte> buffer = stackalloc byte[bytes];
            reader.Read(buffer);
            for (int i = 0; i < bytes; i++)
            {
                Data |= (ulong)buffer[i] << (bytes - i - 1);
            }
        }
    }

    public struct SubframeFixed
    {
        public void Read(BitReader reader, FrameHeader frameHeader)
        {
            int bytes = frameHeader.SampleSizeInBits >> 3;

            Span<byte> buffer = stackalloc byte[bytes];
            reader.Read(buffer);
            for (int i = 0; i < bytes; i++)
            {
                //  Data |= (ulong)buffer[i] << (bytes - i - 1);
            }
        }
    }

    public enum ResidualCodingMethod
    {
        PartitionedRice,
        PartitionedRice2
    }

    public struct Residual
    {
        public ResidualCodingMethod CodingMethod;
    }

    public struct ResidualCodingMethodPartitionedRice
    {
    }

    public struct RicePartition
    {
        public ushort RiceParameter;
    }

    public struct ResidualCodingMethodPartitionedRice2
    {
    }

    public enum SubframeType
    {
        Reserved = -1,
        Constant = 0,
        Verbatim = 1,
        Fixed,
        LPC,
    }

    public struct SubframeHeader
    {
        public SubframeType Type;
        public byte Order;
        public bool WastedBitsPerSampleFlag;
        public int WastedBitsPerSample;

        public void Read(BitReader br)
        {
            br.Skip(1);
            byte type = br.ReadByte();

            if (type == 0)
            {
                Type = SubframeType.Constant;
            }

            if (type == 1)
            {
                Type = SubframeType.Verbatim;
            }

            if ((type & 0x8) != 0 && (type & 0x30) == 0 && (type & 0x7) >= 4)
            {
                Type = SubframeType.Fixed;
                Order = (byte)(type & 0x7);
            }
            else
            {
                Type = SubframeType.Reserved;
            }

            if ((type & 0x20) != 0)
            {
                Type = SubframeType.LPC;
                Order = (byte)((type & 0x1F) + 1);
            }

            WastedBitsPerSampleFlag = br.ReadBool();

            if (WastedBitsPerSampleFlag)
            {
                WastedBitsPerSample = 1;
                while (!br.ReadBool())
                {
                    WastedBitsPerSample++;
                }
            }
        }
    }
}