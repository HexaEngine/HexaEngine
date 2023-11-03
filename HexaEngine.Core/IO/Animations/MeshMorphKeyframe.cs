namespace HexaEngine.Core.IO.Animations
{
    using HexaEngine.Mathematics;

    /// <summary>
    /// Represents a keyframe for mesh morphing animations.
    /// </summary>
    public struct MeshMorphKeyframe
    {
        /// <summary>
        /// Gets or sets the time associated with the keyframe.
        /// </summary>
        public double Time;

        /// <summary>
        /// Gets or sets an array of values related to mesh morphing.
        /// </summary>
        public uint[] Values;

        /// <summary>
        /// Gets or sets an array of weights associated with mesh morphing.
        /// </summary>
        public double[] Weights;

        /// <summary>
        /// Writes the MeshMorphKeyframe to a binary stream with the specified endianness.
        /// </summary>
        /// <param name="stream">The binary stream to write the keyframe to.</param>
        /// <param name="endianness">The endianness of binary data.</param>
        public readonly void Write(Stream stream, Endianness endianness)
        {
            stream.WriteDouble(Time, endianness);
            stream.WriteInt32(Values.Length, endianness);
            for (int i = 0; i < Values.Length; i++)
            {
                stream.WriteUInt32(Values[i], endianness);
            }
            stream.WriteInt32(Weights.Length, endianness);
            for (int i = 0; i < Weights.Length; i++)
            {
                stream.WriteDouble(Weights[i], endianness);
            }
        }

        /// <summary>
        /// Reads a MeshMorphKeyframe from a binary stream with the specified endianness.
        /// </summary>
        /// <param name="stream">The binary stream to read the keyframe from.</param>
        /// <param name="endianness">The endianness of binary data.</param>
        public void Read(Stream stream, Endianness endianness)
        {
            Time = stream.ReadDouble(endianness);
            int valueCount = stream.ReadInt32(endianness);
            Values = new uint[valueCount];
            for (int i = 0; i < valueCount; i++)
            {
                Values[i] = stream.ReadUInt32(endianness);
            }
            int weightCount = stream.ReadInt32(endianness);
            Weights = new double[weightCount];
            for (int i = 0; i < weightCount; i++)
            {
                Weights[i] = stream.ReadDouble(endianness);
            }
        }

        /// <summary>
        /// Reads a MeshMorphKeyframe from a binary stream with the specified endianness.
        /// </summary>
        /// <param name="stream">The binary stream to read the keyframe from.</param>
        /// <param name="endianness">The endianness of binary data.</param>
        /// <returns>The MeshMorphKeyframe read from the stream.</returns>
        public static MeshMorphKeyframe ReadFrom(Stream stream, Endianness endianness)
        {
            MeshMorphKeyframe keyframe = default;
            keyframe.Read(stream, endianness);
            return keyframe;
        }
    }
}