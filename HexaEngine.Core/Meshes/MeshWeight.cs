namespace HexaEngine.Core.Meshes
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;

    public struct MeshWeight
    {
        public uint VertexId;
        public float Weight;

        public MeshWeight(uint vertexId, float weight)
        {
            VertexId = vertexId;
            Weight = weight;
        }

        public void Read(Stream stream, Endianness endianness)
        {
            VertexId = stream.ReadUInt(endianness);
            Weight = stream.ReadFloat(endianness);
        }

        public void Write(Stream stream, Endianness endianness)
        {
            stream.WriteUInt(VertexId, endianness);
            stream.WriteFloat(Weight, endianness);
        }
    }
}