namespace HexaEngine.Core.IO.Binary.Animations
{
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Metadata;
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

        private Guid guid;
        private string name;
        private double duration;
        private double ticksPerSecond;
        private readonly List<NodeChannel> nodeChannels = new();
        private readonly List<MeshChannel> meshChannels = new();
        private readonly List<MorphMeshChannel> morphMeshChannels = new();
        private readonly Metadata metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationClip"/> class.
        /// </summary>
        public AnimationClip()
        {
            guid = Guid.NewGuid();
            name = string.Empty;
            duration = 0;
            ticksPerSecond = 0;
            nodeChannels = new();
            meshChannels = new();
            morphMeshChannels = new();
            metadata = new();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationClip"/> class with specified parameters.
        /// </summary>
        /// <param name="guid">The GUID of the animation clip.</param>
        /// <param name="name">The name of the animation clip.</param>
        /// <param name="duration">The duration of the animation clip.</param>
        /// <param name="ticksPerSecond">The number of ticks per second for the animation clip.</param>
        /// <param name="metadata">The metadata of the animation clip.</param>
        public AnimationClip(Guid guid, string name, double duration, double ticksPerSecond, Metadata? metadata)
        {
            this.guid = guid;
            this.name = name;
            this.duration = duration;
            this.ticksPerSecond = ticksPerSecond;
            nodeChannels = new();
            meshChannels = new();
            morphMeshChannels = new();
            this.metadata = metadata ?? new();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationClip"/> class with specified parameters.
        /// </summary>
        /// <param name="guid">The GUID of the animation clip.</param>
        /// <param name="name">The name of the animation clip.</param>
        /// <param name="duration">The duration of the animation clip.</param>
        /// <param name="ticksPerSecond">The number of ticks per second for the animation clip.</param>
        /// <param name="nodeChannels">The list of node channels for the animation clip.</param>
        /// <param name="meshChannels">The list of mesh channels for the animation clip.</param>
        /// <param name="morphMeshChannels">The list of morph mesh channels for the animation clip.</param>
        /// <param name="metadata">The metadata of the animation clip.</param>
        public AnimationClip(Guid guid, string name, double duration, double ticksPerSecond, IList<NodeChannel> nodeChannels, IList<MeshChannel> meshChannels, IList<MorphMeshChannel> morphMeshChannels, Metadata? metadata)
        {
            this.guid = guid;
            this.name = name;
            this.duration = duration;
            this.ticksPerSecond = ticksPerSecond;
            this.nodeChannels = new(nodeChannels);
            this.meshChannels = new(meshChannels);
            this.morphMeshChannels = new(morphMeshChannels);
            this.metadata = metadata ?? new();
        }

        /// <summary>
        /// Gets or sets the GUID of the animation clip. (Do not use, unless you know what you are doing. Used for internal resource management.)
        /// </summary>
        public Guid Guid { get => guid; set => guid = value; }

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
        /// Gets the metadata of the animation clip.
        /// </summary>
        public Metadata Metadata => metadata;

        /// <summary>
        /// Writes the animation clip data to a stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="encoding">The encoding to use for string writing.</param>
        /// <param name="endianness">The endianness to use for binary writing.</param>
        public virtual void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteGuid(guid, endianness);
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
            metadata.Write(stream, encoding, endianness);
        }

        /// <summary>
        /// Reads an <see cref="AnimationClip"/> data from a stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding to use for string reading.</param>
        /// <param name="endianness">The endianness to use for binary reading.</param>
        public static AnimationClip Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            var guid = stream.ReadGuid(endianness);
            var name = stream.ReadString(encoding, endianness) ?? string.Empty;
            var duration = stream.ReadDouble(endianness);
            var ticksPerSecond = stream.ReadDouble(endianness);

            var nodeChannelCount = stream.ReadInt32(endianness);
            List<NodeChannel> nodeChannels = new(nodeChannelCount);
            for (int i = 0; i < nodeChannelCount; i++)
            {
                nodeChannels.Add(NodeChannel.ReadFrom(stream, encoding, endianness));
            }

            var meshChannelCount = stream.ReadInt32(endianness);
            List<MeshChannel> meshChannels = new(meshChannelCount);
            for (int i = 0; i < meshChannelCount; i++)
            {
                meshChannels.Add(MeshChannel.ReadFrom(stream, encoding, endianness));
            }

            var morphMeshChannelCount = stream.ReadInt32(endianness);
            List<MorphMeshChannel> morphMeshChannels = new(morphMeshChannelCount);
            for (int i = 0; i < morphMeshChannelCount; i++)
            {
                morphMeshChannels.Add(MorphMeshChannel.ReadFrom(stream, encoding, endianness));
            }

            var metadata = Metadata.ReadFrom(stream, encoding, endianness);

            return new AnimationClip(guid, name, duration, ticksPerSecond, nodeChannels, meshChannels, morphMeshChannels, metadata);
        }

        /// <summary>
        /// Deep clones a <see cref="AnimationFile"/> instance.
        /// </summary>
        /// <returns>The deep cloned <see cref="AnimationFile"/> instance.</returns>
        public AnimationClip Clone()
        {
            return new AnimationClip(Guid.NewGuid(), (string)Name.Clone(), duration, ticksPerSecond, nodeChannels.Select(x => x.Clone()).ToList(), meshChannels.Select(x => x.Clone()).ToList(), morphMeshChannels.Select(x => x.Clone()).ToList(), Metadata.Clone());
        }
    }
}