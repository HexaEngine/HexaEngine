namespace HexaEngine.Core.IO.Animations
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using K4os.Compression.LZ4;
    using K4os.Compression.LZ4.Streams;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Text;

    public class Animation
    {
        public AnimationHeader Header;
        public string Name;
        public double Duration;
        public double TicksPerSecond;
        public readonly List<NodeChannel> NodeChannels = new();
        public readonly List<MeshChannel> MeshChannels = new();
        public readonly List<MorphMeshChannel> MorphMeshChannels = new();

        private Animation(string path, Stream fs)
        {
            Name = path;

            Header.Read(fs);

            var stream = fs;
            if (Header.Compression == Compression.Deflate)
            {
                stream = new DeflateStream(fs, CompressionMode.Decompress, true);
            }

            if (Header.Compression == Compression.LZ4)
            {
                stream = LZ4Stream.Decode(fs, 0, true);
            }

            Name = stream.ReadString(Header.Encoding, Header.Endianness) ?? string.Empty;
            Duration = stream.ReadDouble(Header.Endianness);
            TicksPerSecond = stream.ReadDouble(Header.Endianness);

            var nodeChannelCount = stream.ReadInt32(Header.Endianness);
            NodeChannels = new(nodeChannelCount);
            for (int i = 0; i < nodeChannelCount; i++)
            {
                NodeChannels.Add(NodeChannel.ReadFrom(stream, Header.Encoding, Header.Endianness));
            }

            var meshChannelCount = stream.ReadInt32(Header.Endianness);
            MeshChannels = new(meshChannelCount);
            for (int i = 0; i < meshChannelCount; i++)
            {
                MeshChannels.Add(MeshChannel.ReadFrom(stream, Header.Encoding, Header.Endianness));
            }

            var morphMeshChannelCount = stream.ReadInt32(Header.Endianness);
            MorphMeshChannels = new(morphMeshChannelCount);
            for (int i = 0; i < morphMeshChannelCount; i++)
            {
                MorphMeshChannels.Add(MorphMeshChannel.ReadFrom(stream, Header.Encoding, Header.Endianness));
            }

            fs.Close();
        }

        private Animation(string path) : this(path, FileSystem.Open(path))
        {
        }

        public Animation(string name, double duration, double ticksPerSecond)
        {
            Name = name;
            Duration = duration;
            TicksPerSecond = ticksPerSecond;
            NodeChannels = new();
            MeshChannels = new();
            MorphMeshChannels = new();
        }

        public void Save(string path, Encoding encoding, Endianness endianness = Endianness.LittleEndian, Compression compression = Compression.LZ4)
        {
            Stream fs = File.Create(path);

            Header.Encoding = encoding;
            Header.Endianness = endianness;
            Header.Compression = compression;
            Header.Write(fs);

            var stream = fs;
            if (compression == Compression.Deflate)
            {
                stream = new DeflateStream(fs, CompressionLevel.SmallestSize, true);
            }

            if (compression == Compression.LZ4)
            {
                stream = LZ4Stream.Encode(fs, LZ4Level.L12_MAX, 0, true);
            }

            stream.WriteString(Name, encoding, endianness);
            stream.WriteDouble(Duration, endianness);
            stream.WriteDouble(TicksPerSecond, endianness);
            stream.WriteInt32(NodeChannels.Count, endianness);
            for (int i = 0; i < NodeChannels.Count; i++)
            {
                NodeChannels[i].Write(stream, encoding, endianness);
            }
            stream.WriteInt32(MeshChannels.Count, endianness);
            for (int i = 0; i < MeshChannels.Count; i++)
            {
                MeshChannels[i].Write(stream, encoding, endianness);
            }
            stream.WriteInt32(MorphMeshChannels.Count, endianness);
            for (int i = 0; i < MorphMeshChannels.Count; i++)
            {
                MorphMeshChannels[i].Write(stream, encoding, endianness);
            }

            stream.Close();
            fs.Close();
        }

        public static Animation Load(string path)
        {
            return new Animation(path);
        }

        public static Animation LoadExternal(string path)
        {
            return new Animation(path, File.OpenRead(path));
        }
    }
}