namespace HexaEngine.Core.Meshes
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Numerics;
    using System.Text;

    public unsafe struct BoneData
    {
        public string Name;
        public VertexWeight[] Weights;
        public Matrix4x4 Offset;

        public BoneData(string name, VertexWeight[] weights, Matrix4x4 offset)
        {
            Name = name;
            Weights = weights;
            Offset = offset;
        }

        public void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            Name = stream.ReadString(encoding, endianness);
            uint nweights = stream.ReadUInt(endianness);
            Weights = new VertexWeight[nweights];
            for (uint i = 0; i < nweights; i++)
            {
                Weights[i].Read(stream, endianness);
            }
            Offset = stream.ReadStruct<Matrix4x4>();
        }

        public void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteString(Name, encoding, endianness);
            stream.WriteUInt((uint)Weights.LongLength, endianness);
            for (uint i = 0; i < Weights.LongLength; i++)
            {
                Weights[i].Write(stream, endianness);
            }
            stream.WriteStruct(Offset);
        }
    }
}