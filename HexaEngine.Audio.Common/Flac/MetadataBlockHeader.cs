namespace HexaEngine.Audio.Common.Flac
{
    public struct MetadataBlockHeader
    {
        public bool Flag;
        public BlockType Type;
        public uint Length;

        public void Read(BitReader br)
        {
            Flag = br.ReadBool();
            Type = (BlockType)br.ReadRawUInt8(7);
            Length = br.ReadRawUInt32(24);
        }

        public readonly void Write(BitWriter bw)
        {
            bw.WriteBool(Flag);
            bw.WriteRawUInt8((byte)Type, 7);
            bw.WriteRawUInt32(Length, 24);
        }
    }
}