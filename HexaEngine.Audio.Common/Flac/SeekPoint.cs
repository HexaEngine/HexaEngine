namespace HexaEngine.Audio.Common.Flac
{
    public struct SeekPoint
    {
        public ulong SampleNumber;
        public ulong Offset;
        public ushort NumberOfSamples;

        public void Read(BitReader br)
        {
            SampleNumber = br.ReadUInt64BigEndian();
            Offset = br.ReadUInt64BigEndian();
            NumberOfSamples = br.ReadUInt16BigEndian();
        }

        public readonly void Write(BitWriter bw)
        {
            bw.WriteUInt64BigEndian(SampleNumber);
            bw.WriteUInt64BigEndian(Offset);
            bw.WriteUInt16BigEndian(NumberOfSamples);
        }
    }
}