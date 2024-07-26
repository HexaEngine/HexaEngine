namespace HexaEngine.Core.IO.Binary.Animations
{
    using HexaEngine.Core.IO;
    using Hexa.NET.Mathematics;
    using System.Numerics;

    /// <summary>
    /// Represents a keyframe for storing quaternion rotation values.
    /// </summary>
    public struct QuatKeyframe
    {
        /// <summary>
        /// Gets or sets the quaternion rotation value at this keyframe.
        /// </summary>
        public Quaternion Value;

        /// <summary>
        /// Gets or sets the time at which this keyframe occurs.
        /// </summary>
        public double Time;

        /// <summary>
        /// Writes the QuatKeyframe data to a binary stream.
        /// </summary>
        /// <param name="stream">The binary stream to write to.</param>
        /// <param name="endianness">The endianness of binary data.</param>
        public readonly void Write(Stream stream, Endianness endianness)
        {
            stream.WriteQuaternion(Value, endianness);
            stream.WriteDouble(Time, endianness);
        }

        /// <summary>
        /// Reads a QuatKeyframe from a binary stream with the specified endianness.
        /// </summary>
        /// <param name="stream">The binary stream to read the keyframe from.</param>
        /// <param name="endianness">The endianness of binary data.</param>
        public void Read(Stream stream, Endianness endianness)
        {
            Value = stream.ReadQuaternion(endianness);
            Time = stream.ReadDouble(endianness);
        }

        /// <summary>
        /// Reads a QuatKeyframe from a binary stream with the specified endianness.
        /// </summary>
        /// <param name="stream">The binary stream to read the keyframe from.</param>
        /// <param name="endianness">The endianness of binary data.</param>
        /// <returns>The QuatKeyframe read from the stream.</returns>
        public static QuatKeyframe ReadFrom(Stream stream, Endianness endianness)
        {
            QuatKeyframe keyframe = default;
            keyframe.Read(stream, endianness);
            return keyframe;
        }
    }
}