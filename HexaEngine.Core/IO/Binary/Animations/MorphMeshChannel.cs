namespace HexaEngine.Core.IO.Binary.Animations
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System;
    using System.Text;

    /// <summary>
    /// Represents a channel for morphing mesh animations.
    /// </summary>
    public struct MorphMeshChannel
    {
        /// <summary>
        /// Gets or sets the name of the mesh associated with this channel.
        /// </summary>
        public string MeshName;

        /// <summary>
        /// Gets a list of keyframes for morphing the mesh.
        /// </summary>
        public List<MeshMorphKeyframe> Keyframes = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="MorphMeshChannel"/> struct with the specified mesh name.
        /// </summary>
        /// <param name="meshName">The name of the mesh associated with this channel.</param>
        public MorphMeshChannel(string meshName) : this()
        {
            MeshName = meshName;
        }

        /// <summary>
        /// Writes the MorphMeshChannel to a binary stream with the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The binary stream to write the channel to.</param>
        /// <param name="encoding">The encoding used for writing strings.</param>
        /// <param name="endianness">The endianness of binary data.</param>
        public readonly void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteString(MeshName, encoding, endianness);
            stream.WriteInt32(Keyframes.Count, endianness);
            for (int i = 0; i < Keyframes.Count; i++)
            {
                Keyframes[i].Write(stream, endianness);
            }
        }

        /// <summary>
        /// Reads a MorphMeshChannel from a binary stream with the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The binary stream to read the channel from.</param>
        /// <param name="encoding">The encoding used for reading strings.</param>
        /// <param name="endianness">The endianness of binary data.</param>
        public void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            MeshName = stream.ReadString(encoding, endianness) ?? string.Empty;
            var keyframeCount = stream.ReadInt32(endianness);
            Keyframes = new(keyframeCount);
            for (int i = 0; i < keyframeCount; i++)
            {
                Keyframes.Add(MeshMorphKeyframe.ReadFrom(stream, endianness));
            }
        }

        /// <summary>
        /// Reads a MorphMeshChannel from a binary stream with the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The binary stream to read the channel from.</param>
        /// <param name="encoding">The encoding used for reading strings.</param>
        /// <param name="endianness">The endianness of binary data.</param>
        /// <returns>The MorphMeshChannel read from the stream.</returns>
        public static MorphMeshChannel ReadFrom(Stream stream, Encoding encoding, Endianness endianness)
        {
            MorphMeshChannel channel = default;
            channel.Read(stream, encoding, endianness);
            return channel;
        }

        /// <summary>
        /// Deep clones a <see cref="MorphMeshChannel"/> instance.
        /// </summary>
        /// <returns>The deep cloned <see cref="MorphMeshChannel"/> instance.</returns>
        public MorphMeshChannel Clone()
        {
            return new MorphMeshChannel() { Keyframes = new(Keyframes), MeshName = MeshName };
        }
    }
}