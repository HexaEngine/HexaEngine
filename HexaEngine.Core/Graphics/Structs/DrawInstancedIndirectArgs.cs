namespace HexaEngine.Core.Graphics.Structs
{
    public struct DrawInstancedIndirectArgs
    {
        public uint VertexCountPerInstance;
        public uint InstanceCount;
        public uint StartVertexLocation;
        public uint StartInstanceLocation;

        public DrawInstancedIndirectArgs(uint vertexCountPerInstance, uint instanceCount, uint startVertexLocation, uint startInstanceLocation)
        {
            VertexCountPerInstance = vertexCountPerInstance;
            InstanceCount = instanceCount;
            StartVertexLocation = startVertexLocation;
            StartInstanceLocation = startInstanceLocation;
        }
    }
}