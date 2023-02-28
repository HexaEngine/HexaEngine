namespace HexaEngine.Core.Graphics.Structs
{
    public struct DrawIndexedInstancedIndirectArgs
    {
        public uint IndexCountPerInstance;
        public uint InstanceCount;
        public uint StartIndexLocation;
        public int BaseVertexLocation;
        public uint StartInstanceLocation;
    }
}