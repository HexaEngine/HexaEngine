namespace HexaEngine.Audio.Common.Flac
{
    using HexaEngine.Core.Unsafes;

    public unsafe struct SeekTable
    {
        public UnsafeList<SeekPoint> SeekPoints;

        public void Read(BitReader br, MetadataBlockHeader header)
        {
            var seekPointLength = header.Length / sizeof(SeekPoint);
            SeekPoints.Capacity = (uint)seekPointLength;
            for (int i = 0; i < seekPointLength; i++)
            {
                SeekPoint point = default;
                point.Read(br);
                SeekPoints.Add(point);
            }
        }

        public readonly void Write(BitWriter bw)
        {
            for (int i = 0; i < SeekPoints.Size; i++)
            {
                SeekPoints[i].Write(bw);
            }
        }

        public void Release()
        {
            SeekPoints.Release();
            this = default;
        }
    }
}