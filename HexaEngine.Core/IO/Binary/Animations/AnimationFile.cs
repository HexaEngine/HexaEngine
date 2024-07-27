namespace HexaEngine.Core.IO.Binary.Animations
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.IO.Binary.Metadata;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class AnimationFile : AnimationClip
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationFile"/> class with default values.
        /// </summary>
        public AnimationFile()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationFile"/> class from a <see cref="AnimationClip"/>.
        /// </summary>
        public AnimationFile(AnimationClip clip) : this(clip.Guid, clip.Name, clip.Duration, clip.TicksPerSecond, clip.NodeChannels, clip.MeshChannels, clip.MorphMeshChannels, clip.Metadata.Clone())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationFile"/> class with specified parameters.
        /// </summary>
        /// <param name="guid">The GUID of the animation clip.</param>
        /// <param name="name">The name of the animation clip.</param>
        /// <param name="duration">The duration of the animation clip.</param>
        /// <param name="ticksPerSecond">The number of ticks per second for the animation clip.</param>
        /// <param name="metadata">The metadata of the animation clip.</param>
        public AnimationFile(Guid guid, string name, double duration, double ticksPerSecond, Metadata? metadata) : base(guid, name, duration, ticksPerSecond, metadata)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationFile"/> class with specified parameters.
        /// </summary>
        /// <param name="guid">The GUID of the animation clip.</param>
        /// <param name="name">The name of the animation clip.</param>
        /// <param name="duration">The duration of the animation clip.</param>
        /// <param name="ticksPerSecond">The number of ticks per second for the animation clip.</param>
        /// <param name="nodeChannels">The list of node channels for the animation clip.</param>
        /// <param name="meshChannels">The list of mesh channels for the animation clip.</param>
        /// <param name="morphMeshChannels">The list of morph mesh channels for the animation clip.</param>
        /// <param name="metadata">The metadata of the animation clip.</param>
        public AnimationFile(Guid guid, string name, double duration, double ticksPerSecond, IList<NodeChannel> nodeChannels, IList<MeshChannel> meshChannels, IList<MorphMeshChannel> morphMeshChannels, Metadata? metadata) : base(guid, name, duration, ticksPerSecond, nodeChannels, meshChannels, morphMeshChannels, metadata)
        {
        }

        /// <summary>
        /// Reads an <see cref="AnimationClip"/> data from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        public static AnimationFile Read(Stream stream)
        {
            AnimationFileHeader header = default;
            header.Read(stream);
            Encoding encoding = header.Encoding ?? Encoding.UTF8;
            Endianness endianness = header.Endianness;

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

            return new AnimationFile(guid, name, duration, ticksPerSecond, nodeChannels, meshChannels, morphMeshChannels, metadata);
        }

        /// <summary>
        /// Writes the <see cref="AnimationFile"/> instance to the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to write the <see cref="AnimationFile"/> to.</param>
        /// <param name="encoding">The character encoding to use for writing strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        public void Save(string path, Encoding encoding, Endianness endianness = Endianness.LittleEndian)
        {
            FileStream? stream = null;
            try
            {
                stream = File.Create(path);
                Write(stream, encoding, endianness);
            }
            finally
            {
                stream?.Dispose();
            }
        }

        /// <summary>
        /// Writes the <see cref="AnimationFile"/> instance to the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        /// <param name="encoding">The character encoding to use for writing strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        public override void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            AnimationFileHeader header = new()
            {
                Encoding = encoding,
                Endianness = endianness
            };
            header.Write(stream);
            base.Write(stream, encoding, endianness);
        }

        /// <summary>
        /// Deep clones a <see cref="AnimationFile"/> instance.
        /// </summary>
        /// <returns>The deep cloned <see cref="AnimationFile"/> instance.</returns>
        public new AnimationFile Clone()
        {
            return new(this);
        }
    }
}