namespace HexaEngine.Audio.Common.Flac
{
    using HexaEngine.Core.Unsafes;

    public unsafe struct CueSheet
    {
        public UInt128* MediaCatalogNumbers;
        public ushort NumberOfLeadInSamples;
        public bool CompactDisc;

        // Reserved <7+258*8>

        public UnsafeList<CueSheetTrack> Tracks;

        public static readonly byte[] EmptyMCNs = new byte[128];

        public void Read(BitReader br)
        {
            Span<byte> buffer = stackalloc byte[128];
            br.Read(buffer);
            if (!buffer.SequenceEqual(EmptyMCNs))
            {
                MediaCatalogNumbers = (UInt128*)AllocCopy(buffer);
            }

            NumberOfLeadInSamples = br.ReadUInt16BigEndian();
            CompactDisc = br.ReadBool();
            br.BytePosition += 258; // Reserved <7+258*8>

            byte numberOfTracks = br.ReadByte();
            Tracks.Capacity = numberOfTracks;
            for (int i = 0; i < numberOfTracks; i++)
            {
                CueSheetTrack track = default;
                track.Read(br);
                Tracks.Add(track);
            }
        }

        public readonly void Write(BitWriter bw)
        {
            if (MediaCatalogNumbers == null)
            {
                bw.BytePosition += 128;
            }
            else
            {
                bw.Write(new Span<byte>(MediaCatalogNumbers, 128));
            }

            bw.WriteUInt16BigEndian(NumberOfLeadInSamples);
            bw.WriteBool(CompactDisc);
            bw.BytePosition += 258; // Reserved <7+258*8>

            bw.WriteByte((byte)Tracks.Size);
            for (int i = 0; i < Tracks.Size; i++)
            {
                Tracks[i].Write(bw);
            }
        }

        public void Release()
        {
            if (MediaCatalogNumbers != null)
            {
                Free(MediaCatalogNumbers);
            }
            MediaCatalogNumbers = null;

            for (int i = 0; i < Tracks.Size; i++)
            {
                Tracks[i].Release();
            }
            Tracks.Release();
            this = default;
        }
    }
}