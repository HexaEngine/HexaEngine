namespace HexaEngine.Audio.Common.Wave
{
    using System.IO;

    public struct WaveHeader
    {
        public string ChunkId;
        public int ChunkSize;
        public string Format;
        public string SubChunkId;
        public int SubChunkSize;
        public WaveFormatEncoding WaveFormat;
        public short NumChannels;
        public int SampleRate;
        public int BytesPerSecond;
        public short BlockAlign;
        public short BitsPerSample;
        public string DataChunkId;
        public long DataBegin;
        public int DataSize;

        public WaveHeader(Stream stream)
        {
            // Open the wave file in binary.
            BinaryReader reader = new(stream);

            // Read in the wave file header.
            ChunkId = new string(reader.ReadChars(4));
            ChunkSize = reader.ReadInt32();
            Format = new string(reader.ReadChars(4));
            SubChunkId = new string(reader.ReadChars(4));
            SubChunkSize = reader.ReadInt32();
            WaveFormat = (WaveFormatEncoding)reader.ReadInt16();
            NumChannels = reader.ReadInt16();
            SampleRate = reader.ReadInt32();
            BytesPerSecond = reader.ReadInt32();
            BlockAlign = reader.ReadInt16();
            BitsPerSample = reader.ReadInt16();
            DataChunkId = new string(reader.ReadChars(4));
            DataSize = reader.ReadInt32();

            // prevent other chunkids to be loaded.
            while (DataChunkId != "data")
            {
                reader.BaseStream.Position += DataSize;
                DataChunkId = new string(reader.ReadChars(4));
                DataSize = reader.ReadInt32();
            }

            DataBegin = (int)reader.BaseStream.Position;
        }

        public void Read(Stream stream)
        {
            // Open the wave file in binary.
            BinaryReader reader = new(stream);

            // Read in the wave file header.
            ChunkId = new string(reader.ReadChars(4));
            ChunkSize = reader.ReadInt32();
            Format = new string(reader.ReadChars(4));
            SubChunkId = new string(reader.ReadChars(4));
            SubChunkSize = reader.ReadInt32();
            WaveFormat = (WaveFormatEncoding)reader.ReadInt16();
            NumChannels = reader.ReadInt16();
            SampleRate = reader.ReadInt32();
            BytesPerSecond = reader.ReadInt32();
            BlockAlign = reader.ReadInt16();
            BitsPerSample = reader.ReadInt16();

            DataChunkId = new string(reader.ReadChars(4));
            DataSize = reader.ReadInt32();

            // prevent other chunkids to be loaded.
            while (DataChunkId != "data")
            {
                reader.BaseStream.Position += DataSize;
                DataChunkId = new string(reader.ReadChars(4));
                DataSize = reader.ReadInt32();
            }

            DataBegin = reader.BaseStream.Position;
        }
    }
}