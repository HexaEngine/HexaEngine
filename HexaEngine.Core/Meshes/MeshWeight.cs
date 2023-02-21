namespace HexaEngine.Core.Meshes
{
    public struct MeshWeight
    {
        public uint VertexId;
        public float Weight;

        public MeshWeight(uint vertexId, float weight)
        {
            VertexId = vertexId;
            Weight = weight;
        }
    }
}