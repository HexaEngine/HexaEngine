namespace HexaEngine.Graphics.Culling
{
    public struct TypeData
    {
        public uint IndexCountPerInstance;
        public uint StartIndexLocation;
        public int BaseVertexLocation;
        public uint StartInstanceLocation;

        public TypeData(uint indexCountPerInstance, uint startIndexLocation, int baseVertexLocation, uint startInstanceLocation)
        {
            IndexCountPerInstance = indexCountPerInstance;
            StartIndexLocation = startIndexLocation;
            BaseVertexLocation = baseVertexLocation;
            StartInstanceLocation = startInstanceLocation;
        }
    }
}