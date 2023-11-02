namespace HexaEngine.Audio.Common.Flac
{
    public unsafe struct CueSheetTrackIndex
    {
        public ulong OffsetInSamples;
        public byte IndexPointNumber;
        // Reserved 3*8;

        public void Read(BitReader br)
        {
            OffsetInSamples = br.ReadUInt64BigEndian();
            IndexPointNumber = br.ReadByte();
            br.BitPosition += 3;  // Reserved 3*8;
        }

        public readonly void Write(BitWriter bw)
        {
            bw.WriteUInt64BigEndian(OffsetInSamples);
            bw.WriteByte(IndexPointNumber);
            bw.BytePosition += 3; // Reserved 3*8;
        }
    }
}