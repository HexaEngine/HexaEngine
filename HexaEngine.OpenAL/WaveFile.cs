namespace HexaEngine.OpenAL
{
    using HexaEngine.IO;
    using System.IO;

    public struct WaveHeader
    {
        public string ChunkId;
        public int ChunkSize;
        public string Format;
        public string SubChunkId;
        public int SubChunkSize;
        public WaveFormatEncoding AudioFormat;
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
            AudioFormat = (WaveFormatEncoding)reader.ReadInt16();
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

        /// <summary>
        /// Gets the buffer format.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public BufferFormat GetBufferFormat()
        {
            BufferFormat format;
            if (NumChannels == 1 && BitsPerSample == 8)
                format = BufferFormat.Mono8;
            else if (NumChannels == 1 && BitsPerSample == 16)
                format = BufferFormat.Mono16;
            else if (NumChannels == 2 && BitsPerSample == 8)
                format = BufferFormat.Stereo8;
            else if (NumChannels == 2 && BitsPerSample == 16)
                format = BufferFormat.Stereo16;
            else
            {
                throw new InvalidDataException();
            }

            return format;
        }
    }

    public class WaveFile
    {
        public string ChunkId;
        public int ChunkSize;
        public string Format;
        public string SubChunkId;
        public int SubChunkSize;
        public WaveFormatEncoding AudioFormat;
        public short NumChannels;
        public int SampleRate;
        public int BytesPerSecond;
        public short BlockAlign;
        public short BitsPerSample;
        public string DataChunkId;
        public int DataSize;
        public byte[] WaveData;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaveFile"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <exception cref="System.IO.InvalidDataException"></exception>
        public WaveFile(string path)
        {
            // Open the wave file in binary.
            BinaryReader reader = new(FileSystem.Open(Paths.CurrentSoundPath + path));

            // Read in the wave file header.
            ChunkId = new string(reader.ReadChars(4));
            ChunkSize = reader.ReadInt32();
            Format = new string(reader.ReadChars(4));
            SubChunkId = new string(reader.ReadChars(4));
            SubChunkSize = reader.ReadInt32();
            AudioFormat = (WaveFormatEncoding)reader.ReadInt16();
            NumChannels = reader.ReadInt16();
            SampleRate = reader.ReadInt32();
            BytesPerSecond = reader.ReadInt32();
            BlockAlign = reader.ReadInt16();
            BitsPerSample = reader.ReadInt16();
            DataChunkId = new string(reader.ReadChars(4));
            DataSize = reader.ReadInt32();

            // Check that the chunk ID is the RIFF format
            // and the file format is the WAVE format
            // and sub chunk ID is the fmt format
            // and the audio format is PCM
            // and the wave file was recorded in stereo format
            // and at a sample rate of 44.1 KHz
            // and at 16 bit format
            // and there is the data chunk header.
            // Otherwise return false.
            // modified in Tutorial 31 for 3D Sound loading stereo files in a mono Secondary buffer.
            if (ChunkId != "RIFF" || Format != "WAVE" || SubChunkId.Trim() != "fmt" || AudioFormat != WaveFormatEncoding.Pcm)
                throw new InvalidDataException();

            // prevent other chunkids to be loaded.
            while (DataChunkId != "data")
            {
                reader.BaseStream.Position += DataSize;
                DataChunkId = new string(reader.ReadChars(4));
                DataSize = reader.ReadInt32();
            }

            // Read in the wave file data into the temporary buffer.
            WaveData = reader.ReadBytes(DataSize);

            // Close the reader
            reader.Close();
        }

        /// <summary>
        /// Gets the buffer format.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.IO.InvalidDataException"></exception>
        internal BufferFormat GetBufferFormat()
        {
            BufferFormat format;
            if (NumChannels == 1 && BitsPerSample == 8)
                format = BufferFormat.Mono8;
            else if (NumChannels == 1 && BitsPerSample == 16)
                format = BufferFormat.Mono16;
            else if (NumChannels == 2 && BitsPerSample == 8)
                format = BufferFormat.Stereo8;
            else if (NumChannels == 2 && BitsPerSample == 16)
                format = BufferFormat.Stereo16;
            else
            {
                throw new InvalidDataException();
            }

            return format;
        }
    }
}