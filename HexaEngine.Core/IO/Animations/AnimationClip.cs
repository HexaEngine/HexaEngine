namespace HexaEngine.Core.IO.Animations
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents an animation clip with channels for node, mesh, and morph mesh animations.
    /// </summary>
    public class AnimationClip
    {
        /// <summary>
        /// Gets an empty animation clip instance.
        /// </summary>
        public static AnimationClip Empty => new();

        private string name;
        private double duration;
        private double ticksPerSecond;
        private readonly List<NodeChannel> nodeChannels = new();
        private readonly List<MeshChannel> meshChannels = new();
        private readonly List<MorphMeshChannel> morphMeshChannels = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationClip"/> class with default values.
        /// </summary>
        public AnimationClip()
        {
            name = string.Empty;
            duration = 0;
            ticksPerSecond = 0;
            nodeChannels = new();
            meshChannels = new();
            morphMeshChannels = new();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationClip"/> class with specified parameters.
        /// </summary>
        /// <param name="name">The name of the animation clip.</param>
        /// <param name="duration">The duration of the animation clip.</param>
        /// <param name="ticksPerSecond">The number of ticks per second for the animation clip.</param>
        public AnimationClip(string name, double duration, double ticksPerSecond)
        {
            this.name = name;
            this.duration = duration;
            this.ticksPerSecond = ticksPerSecond;
            nodeChannels = new();
            meshChannels = new();
            morphMeshChannels = new();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationClip"/> class with specified parameters.
        /// </summary>
        /// <param name="name">The name of the animation clip.</param>
        /// <param name="duration">The duration of the animation clip.</param>
        /// <param name="ticksPerSecond">The number of ticks per second for the animation clip.</param>
        /// <param name="nodeChannels">The list of node channels for the animation clip.</param>
        /// <param name="meshChannels">The list of mesh channels for the animation clip.</param>
        /// <param name="morphMeshChannels">The list of morph mesh channels for the animation clip.</param>
        public AnimationClip(string name, double duration, double ticksPerSecond, IList<NodeChannel> nodeChannels, IList<MeshChannel> meshChannels, IList<MorphMeshChannel> morphMeshChannels)
        {
            this.name = name;
            this.duration = duration;
            this.ticksPerSecond = ticksPerSecond;
            this.nodeChannels = new(nodeChannels);
            this.meshChannels = new(meshChannels);
            this.morphMeshChannels = new(morphMeshChannels);
        }

        /// <summary>
        /// Gets or sets the name of the animation clip.
        /// </summary>
        public string Name { get => name; set => name = value; }

        /// <summary>
        /// Gets or sets the duration of the animation clip.
        /// </summary>
        public double Duration { get => duration; set => duration = value; }

        /// <summary>
        /// Gets or sets the number of ticks per second for the animation clip.
        /// </summary>
        public double TicksPerSecond { get => ticksPerSecond; set => ticksPerSecond = value; }

        /// <summary>
        /// Gets the list of node channels for the animation clip.
        /// </summary>
        public List<NodeChannel> NodeChannels => nodeChannels;

        /// <summary>
        /// Gets the list of mesh channels for the animation clip.
        /// </summary>
        public List<MeshChannel> MeshChannels => meshChannels;

        /// <summary>
        /// Gets the list of morph mesh channels for the animation clip.
        /// </summary>
        public List<MorphMeshChannel> MorphMeshChannels => morphMeshChannels;

        /// <summary>
        /// Writes the animation clip data to a stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="encoding">The encoding to use for string writing.</param>
        /// <param name="endianness">The endianness to use for binary writing.</param>
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

        /// <summary>
        /// Reads the animation clip data from a stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding to use for string reading.</param>
        /// <param name="endianness">The endianness to use for binary reading.</param>
        public static AnimationClip ReadFrom(Stream stream, Encoding encoding, Endianness endianness)
        {
            AnimationClip animation = new();
            animation.Read(stream, encoding, endianness);
            return animation;
        }

        /// <summary>
        /// Reads the animation clip data from a stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding to use for string reading.</param>
        /// <param name="endianness">The endianness to use for binary reading.</param>
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