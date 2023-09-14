namespace HexaEngine.Core.IO.Animations
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class Animation
    {
        public static Animation Empty => new();

        private string name;
        private double duration;
        private double ticksPerSecond;
        private readonly List<NodeChannel> nodeChannels = new();
        private readonly List<MeshChannel> meshChannels = new();
        private readonly List<MorphMeshChannel> morphMeshChannels = new();

        public Animation()
        {
            name = string.Empty;
            duration = 0;
            ticksPerSecond = 0;
            nodeChannels = new();
            meshChannels = new();
            morphMeshChannels = new();
        }

        public Animation(string name, double duration, double ticksPerSecond)
        {
            this.name = name;
            this.duration = duration;
            this.ticksPerSecond = ticksPerSecond;
            nodeChannels = new();
            meshChannels = new();
            morphMeshChannels = new();
        }

        public Animation(string name, double duration, double ticksPerSecond, IList<NodeChannel> nodeChannels, IList<MeshChannel> meshChannels, IList<MorphMeshChannel> morphMeshChannels)
        {
            this.name = name;
            this.duration = duration;
            this.ticksPerSecond = ticksPerSecond;
            this.nodeChannels = new(nodeChannels);
            this.meshChannels = new(meshChannels);
            this.morphMeshChannels = new(morphMeshChannels);
        }

        public string Name { get => name; set => name = value; }

        public double Duration { get => duration; set => duration = value; }

        public double TicksPerSecond { get => ticksPerSecond; set => ticksPerSecond = value; }

        public List<NodeChannel> NodeChannels => nodeChannels;

        public List<MeshChannel> MeshChannels => meshChannels;

        public List<MorphMeshChannel> MorphMeshChannels => morphMeshChannels;

        public void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteString(name, encoding, endianness);
            stream.WriteDouble(duration, endianness);
            stream.WriteDouble(ticksPerSecond, endianness);
            stream.WriteInt32(nodeChannels.Count, endianness);
            for (int i = 0; i < nodeChannels.Count; i++)
            {
                nodeChannels[i].Write(stream, encoding, endianness);
            }
            stream.WriteInt32(meshChannels.Count, endianness);
            for (int i = 0; i < meshChannels.Count; i++)
            {
                meshChannels[i].Write(stream, encoding, endianness);
            }
            stream.WriteInt32(morphMeshChannels.Count, endianness);
            for (int i = 0; i < morphMeshChannels.Count; i++)
            {
                morphMeshChannels[i].Write(stream, encoding, endianness);
            }
        }

        public static Animation ReadFrom(Stream stream, Encoding encoding, Endianness endianness)
        {
            Animation animation = new();
            animation.Read(stream, encoding, endianness);
            return animation;
        }

        public void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            name = stream.ReadString(encoding, endianness) ?? string.Empty;
            duration = stream.ReadDouble(endianness);
            ticksPerSecond = stream.ReadDouble(endianness);

            var nodeChannelCount = stream.ReadInt32(endianness);
            nodeChannels.Clear();
            nodeChannels.Capacity = nodeChannelCount;
            for (int i = 0; i < nodeChannelCount; i++)
            {
                nodeChannels.Add(NodeChannel.ReadFrom(stream, encoding, endianness));
            }

            var meshChannelCount = stream.ReadInt32(endianness);
            meshChannels.Clear();
            meshChannels.Capacity = meshChannelCount;
            for (int i = 0; i < meshChannelCount; i++)
            {
                meshChannels.Add(MeshChannel.ReadFrom(stream, encoding, endianness));
            }

            var morphMeshChannelCount = stream.ReadInt32(endianness);
            morphMeshChannels.Clear();
            morphMeshChannels.Capacity = morphMeshChannelCount;
            for (int i = 0; i < morphMeshChannelCount; i++)
            {
                morphMeshChannels.Add(MorphMeshChannel.ReadFrom(stream, encoding, endianness));
            }
        }
    }
}