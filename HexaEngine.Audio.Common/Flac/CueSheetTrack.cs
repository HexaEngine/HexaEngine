namespace HexaEngine.Audio.Common.Flac
{
    using HexaEngine.Core.Unsafes;

    public unsafe struct CueSheetTrack
    {
        public ulong TrackOffsetInSamples;
        public byte TrackNumber;
        public byte* ISRC;
        public bool TrackType;
        public bool PreEmphasisFlag;

        // Reserved <6+13*8>

        public UnsafeList<CueSheetTrackIndex> TrackIndexPoints;

        public static readonly byte[] EmptyISRC = new byte[12];

        public void Read(BitReader br)
        {
            TrackOffsetInSamples = br.ReadUInt64BigEndian();
            TrackNumber = br.ReadByte();
            Span<byte> buffer = stackalloc byte[12];
            br.Read(buffer);
            if (!buffer.SequenceEqual(EmptyISRC))
            {
                ISRC = AllocCopy(buffer);
            }
            TrackType = br.ReadBool();
            PreEmphasisFlag = br.ReadBool();
            br.BytePosition += 13; // Reserved <6+13*8>
            byte numberOfTrackIndexPoints = br.ReadByte();
            TrackIndexPoints.Capacity = numberOfTrackIndexPoints;
            for (int i = 0; i < numberOfTrackIndexPoints; i++)
            {
                CueSheetTrackIndex trackIndex = default;
                trackIndex.Read(br);
                TrackIndexPoints.PushBack(trackIndex);
            }
        }

        public readonly void Write(BitWriter bw)
        {
            bw.WriteUInt64BigEndian(TrackOffsetInSamples);
            bw.WriteByte(TrackNumber);
            if (ISRC == null)
            {
                bw.BytePosition += 12;
            }
            else
            {
                bw.Write(new Span<byte>(ISRC, 12));
            }
            bw.WriteBool(TrackType);
            bw.WriteBool(PreEmphasisFlag);
            bw.BytePosition += 13; // Reserved <6+13*8>

            bw.WriteByte((byte)TrackIndexPoints.Size);
            for (int i = 0; i < TrackIndexPoints.Size; i++)
            {
                TrackIndexPoints[i].Write(bw);
            }
        }

        public void Release()
        {
            if (ISRC != null)
            {
                Free(ISRC);
            }
            ISRC = null;

            TrackIndexPoints.Release();
            this = default;
        }
    }
}