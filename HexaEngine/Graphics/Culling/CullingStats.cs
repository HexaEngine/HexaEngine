using HexaEngine.Core.Graphics.Structs;

namespace HexaEngine.Graphics.Culling
{
    public unsafe struct CullingStats
    {
        public uint DrawCalls;
        public uint DrawInstanceCount;

        public uint ActualDrawCalls;
        public uint ActualDrawInstanceCount;
        public uint VertexCount;

        public GPUInstance* Instances;
        public uint InstanceCount;

        public DrawIndexedInstancedIndirectArgs* Args;
        public uint ArgsCount;

        public CullingStats(uint drawCalls, uint drawInstanceCount, uint actualDrawCalls, uint actualDrawInstanceCount, uint vertexCount, GPUInstance* instances, uint instanceCount, DrawIndexedInstancedIndirectArgs* args, uint argsCount)
        {
            DrawCalls = drawCalls;
            DrawInstanceCount = drawInstanceCount;
            ActualDrawCalls = actualDrawCalls;
            ActualDrawInstanceCount = actualDrawInstanceCount;
            VertexCount = vertexCount;
            Instances = instances;
            InstanceCount = instanceCount;
            Args = args;
            ArgsCount = argsCount;
        }
    }
}