namespace HexaEngine.Core.IO.Binary.Animations
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.Numerics;

    /// <summary>
    /// Represents a keyframe for storing 3D vector values.
    /// </summary>
    public struct VectorKeyframe
    {
        /// <summary>
        /// Gets or sets the 3D vector value at this keyframe.
        /// </summary>
        public Vector3 Value;

        /// <summary>
        /// Gets or sets the time at which this keyframe occurs.
        /// </summary>
        public double Time;

        /// <summary>
        /// Writes the VectorKeyframe data to a binary stream.
        /// </summary>
        /// <param name="stream">The binary stream to write to.</param>
        /// <param name="endianness">The endianness of binary data.</param>
        public readonly void Write(Stream stream, Endianness endianness)
        {
            stream.WriteVector3(Value, endianness);
            stream.WriteDouble(Time, endianness);
        }

        /// <summary>
        /// Reads a VectorKeyframe from a binary stream with the specified endianness.
        /// </summary>
        /// <param name="stream">The binary stream to read the keyframe from.</param>
        /// <param name="endianness">The endianness of binary data.</param>
        public void Read(Stream stream, Endianness endianness)
        {
            Value = stream.ReadVector3(endianness);
            Time = stream.ReadDouble(endianness);
        }

        /// <summary>
        /// Reads a VectorKeyframe from a binary stream with the specified endianness.
        /// </summary>
        /// <param name="stream">The binary stream to read the keyframe from.</param>
        /// <param name="endianness">The endianness of binary data.</param>
        /// <returns>The VectorKeyframe read from the stream.</returns>
        public static VectorKeyframe ReadFrom(Stream stream, Endianness endianness)
        {
            VectorKeyframe keyframe = default;
            keyframe.Read(stream, endianness);
            return keyframe;
        }
    }
}