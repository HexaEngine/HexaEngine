namespace HexaEngine.Core.IO.Animations
{
    using HexaEngine.Mathematics;
    using System.Text;

    /// <summary>
    /// Represents a keyframe for a mesh animation.
    /// </summary>
    public struct MeshKeyframe
    {
        /// <summary>
        /// The name of the mesh associated with this keyframe.
        /// </summary>
        public string MeshName;

        /// <summary>
        /// The time at which this keyframe occurs in the animation.
        /// </summary>
        public double Time;

        /// <summary>
        /// Writes the mesh keyframe data to a stream.
        /// </summary>
        /// <param name="stream">The stream to write the data to.</param>
        /// <param name="encoding">The encoding used for writing strings.</param>
        /// <param name="endianness">The endianness of binary data.</param>
        public readonly void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteString(MeshName, encoding, endianness);
            stream.WriteDouble(Time, endianness);
        }

        /// <summary>
        /// Reads the mesh keyframe data from a stream.
        /// </summary>
        /// <param name="stream">The stream to read the data from.</param>
        /// <param name="encoding">The encoding used for reading strings.</param>
        /// <param name="endianness">The endianness of binary data.</param>
        public void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            MeshName = stream.ReadString(encoding, endianness) ?? string.Empty;
            Time = stream.ReadDouble(endianness);
        }

        /// <summary>
        /// Reads a <see cref="MeshKeyframe"/> from a stream.
        /// </summary>
        /// <param name="stream">The stream to read the keyframe from.</param>
        /// <param name="encoding">The encoding used for reading strings.</param>
        /// <param name="endianness">The endianness of binary data.</param>
        /// <returns>The <see cref="MeshKeyframe"/> read from the stream.</returns>
        public static MeshKeyframe ReadFrom(Stream stream, Encoding encoding, Endianness endianness)
        {
            MeshKeyframe keyframe = default;
            keyframe.Read(stream, encoding, endianness);
            return keyframe;
        }
    }
}