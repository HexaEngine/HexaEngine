namespace HexaEngine.Audio.Common.Flac
{
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;

    public unsafe struct Picture
    {
        public PictureType Type;
        public StdString MimeType;
        public StdString Description;
        public uint Width;
        public uint Height;
        public uint ColorDepth;
        public uint IndexCount;
        public uint DataLength;
        public byte* Data;

        public void Read(BitReader br)
        {
            Type = (PictureType)br.ReadUInt32BigEndian();
            MimeType = br.ReadStdString(Endianness.BigEndian);
            Description = br.ReadStdString(Endianness.BigEndian);
            Width = br.ReadUInt32BigEndian();
            Height = br.ReadUInt32BigEndian();
            ColorDepth = br.ReadUInt32BigEndian();
            IndexCount = br.ReadUInt32BigEndian();
            DataLength = br.ReadUInt32BigEndian();
            Data = AllocT<byte>(DataLength);
            br.Read(new Span<byte>(Data, (int)DataLength));
        }

        public readonly void Write(BitWriter bw)
        {
            bw.WriteUInt32BigEndian((uint)Type);
            bw.WriteStdString(MimeType, Endianness.BigEndian);
            bw.WriteStdString(Description, Endianness.BigEndian);
            bw.WriteUInt32BigEndian(Width);
            bw.WriteUInt32BigEndian(Height);
            bw.WriteUInt32BigEndian(ColorDepth);
            bw.WriteUInt32BigEndian(IndexCount);
            bw.WriteUInt32BigEndian(DataLength);
            bw.Write(new Span<byte>(Data, (int)DataLength));
        }

        public void Release()
        {
            MimeType.Release();
            Description.Release();

            Free(Data);
            Data = null;
            this = default;
        }
    }
}