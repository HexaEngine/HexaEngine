namespace HexaEngine.Core.IO.Binary.Meshes
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;

    /// <summary>
    /// Represents a vertex weight, consisting of a vertex ID and a weight.
    /// </summary>
    public struct VertexWeight
    {
        /// <summary>
        /// Gets or sets the ID of the vertex.
        /// </summary>
        public uint VertexId;

        /// <summary>
        /// Gets or sets the weight associated with the vertex.
        /// </summary>
        public float Weight;

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexWeight"/> struct with the specified vertex ID and weight.
        /// </summary>
        /// <param name="vertexId">The ID of the vertex.</param>
        /// <param name="weight">The weight associated with the vertex.</param>
        public VertexWeight(uint vertexId, float weight)
        {
            VertexId = vertexId;
            Weight = weight;
        }

        /// <summary>
        /// Reads the vertex weight from the specified stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        public void Read(Stream stream, Endianness endianness)
        {
            VertexId = stream.ReadUInt32(endianness);
            Weight = stream.ReadFloat(endianness);
        }

        /// <summary>
        /// Writes the vertex weight to the specified stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="endianness">The endianness to use when writing data to the stream.</param>
        public void Write(Stream stream, Endianness endianness)
        {
            stream.WriteUInt32(VertexId, endianness);
            stream.WriteFloat(Weight, endianness);
        }
    }
}