namespace HexaEngine.Core.IO.Binary.Meshes
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Numerics;
    using System.Text;

    /// <summary>
    /// Represents bone-related data in a 3D model.
    /// </summary>
    public unsafe struct BoneData
    {
        /// <summary>
        /// The name of the bone.
        /// </summary>
        public string Name;

        /// <summary>
        /// The weights associated with the bone.
        /// </summary>
        public VertexWeight[] Weights;

        /// <summary>
        /// The offset matrix for the bone.
        /// </summary>
        public Matrix4x4 Offset;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoneData"/> struct with the specified parameters.
        /// </summary>
        /// <param name="name">The name of the bone.</param>
        /// <param name="weights">The weights associated with the bone.</param>
        /// <param name="offset">The offset matrix for the bone.</param>
        public BoneData(string name, VertexWeight[] weights, Matrix4x4 offset)
        {
            Name = name;
            Weights = weights;
            Offset = offset;
        }

        /// <summary>
        /// Reads bone data from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding used for reading strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        public void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            Name = stream.ReadString(encoding, endianness) ?? string.Empty;
            uint nweights = stream.ReadUInt32(endianness);
            Weights = new VertexWeight[nweights];
            for (uint i = 0; i < nweights; i++)
            {
                Weights[i].Read(stream, endianness);
            }
            Offset = stream.ReadMatrix4x4(endianness);
        }

        /// <summary>
        /// Writes bone data to a stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="encoding">The encoding used for writing strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        public readonly void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteString(Name, encoding, endianness);
            stream.WriteUInt32((uint)Weights.LongLength, endianness);
            for (uint i = 0; i < Weights.LongLength; i++)
            {
                Weights[i].Write(stream, endianness);
            }
            stream.WriteMatrix4x4(Offset, endianness);
        }
    }
}