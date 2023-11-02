namespace HexaEngine.Audio.Common.Flac
{
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;
    using System.IO;

    public unsafe struct FlacHeader
    {
        public static readonly byte[] StreamMarker = [0x66, 0x4C, 0x61, 0x43];
        public UnsafeList<MetadataBlock> MetadataBlocks;
        public long DataBegin;
        public long DataSize;

        public void Read(Stream stream)
        {
            if (!stream.Compare(StreamMarker))
            {
                throw new FormatException("FLAC streams need to start with \"fLaC\"");
            }

            BitReader reader = new(stream, true);

            while (true)
            {
                MetadataBlock block = default;
                block.Read(reader);
                MetadataBlocks.PushBack(block);
                if (block.BlockHeader.Flag)
                {
                    break;
                }
            }

            DataBegin = reader.BaseStream.Position;
            DataSize = reader.BaseStream.Length - DataBegin;

            reader.Close();
        }

        public void Write(Stream stream)
        {
            stream.Write(StreamMarker);

            BitWriter writer = new(stream, true);

            for (int i = 0; i < MetadataBlocks.Count; i++)
            {
                MetadataBlocks[i].Write(writer);
            }

            writer.Close();
        }

        public void Release()
        {
            for (int i = 0; i < MetadataBlocks.Size; i++)
            {
                MetadataBlocks[i].Release();
            }
            MetadataBlocks.Release();
            this = default;
        }
    }
}