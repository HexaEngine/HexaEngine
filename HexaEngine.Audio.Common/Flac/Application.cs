namespace HexaEngine.Audio.Common.Flac
{
    public unsafe struct Application
    {
        public ApplicationID ID;
        public byte* Data;

        public void Read(BitReader br, MetadataBlockHeader header)
        {
            ID = (ApplicationID)br.ReadUInt32BigEndian();
            uint len = header.Length - 4;
            Data = AllocT<byte>(len);
            br.Read(new Span<byte>(Data, (int)len));
        }

        public readonly void Write(BitWriter bw, MetadataBlockHeader header)
        {
            bw.WriteUInt32BigEndian((uint)ID);
            uint len = header.Length - 4;
            bw.Write(new Span<byte>(Data, (int)len));
        }

        public void Release()
        {
            Free(Data);
            Data = null;
            this = default;
        }
    }
}