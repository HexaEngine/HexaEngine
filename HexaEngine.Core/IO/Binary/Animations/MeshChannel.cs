namespace HexaEngine.Core.IO.Binary.Animations
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents a channel containing keyframes for a mesh animation.
    /// </summary>
    public struct MeshChannel
    {
        /// <summary>
        /// The name of the mesh associated with this channel.
        /// </summary>
        public string MeshName;

        /// <summary>
        /// The list of keyframes for the mesh animation.
        /// </summary>
        public List<MeshKeyframe> Keyframes = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="MeshChannel"/> struct with the specified mesh name.
        /// </summary>
        /// <param name="meshName">The name of the mesh associated with this channel.</param>
        public MeshChannel(string meshName) : this()
        {
            MeshName = meshName;
        }

        /// <summary>
        /// Writes the mesh channel data to a stream.
        /// </summary>
        /// <param name="stream">The stream to write the data to.</param>
        /// <param name="encoding">The encoding used for writing strings.</param>
        /// <param name="endianness">The endianness of binary data.</param>
        public readonly void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteString(MeshName, encoding, endianness);
            stream.WriteInt32(Keyframes.Count, endianness);
            for (int i = 0; i < Keyframes.Count; i++)
            {
                Keyframes[i].Write(stream, encoding, endianness);
            }
        }

        /// <summary>
        /// Reads the mesh channel data from a stream.
        /// </summary>
        /// <param name="stream">The stream to read the data from.</param>
        /// <param name="encoding">The encoding used for reading strings.</param>
        /// <param name="endianness">The endianness of binary data.</param>
        public void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            MeshName = stream.ReadString(encoding, endianness) ?? string.Empty;
            var keyframeCount = stream.ReadInt32(endianness);
            Keyframes = new(keyframeCount);
            for (int i = 0; i < keyframeCount; i++)
            {
                Keyframes.Add(MeshKeyframe.ReadFrom(stream, encoding, endianness));
            }
        }

        /// <summary>
        /// Reads a <see cref="MeshChannel"/> from a stream.
        /// </summary>
        /// <param name="stream">The stream to read the channel from.</param>
        /// <param name="encoding">The encoding used for reading strings.</param>
        /// <param name="endianness">The endianness of binary data.</param>
        /// <returns>The <see cref="MeshChannel"/> read from the stream.</returns>
        public static MeshChannel ReadFrom(Stream stream, Encoding encoding, Endianness endianness)
        {
            MeshChannel channel = default;
            channel.Read(stream, encoding, endianness);
            return channel;
        }
    }
}