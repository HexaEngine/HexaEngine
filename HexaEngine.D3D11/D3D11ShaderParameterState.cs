namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;

    public unsafe struct D3D11ShaderParameterState
    {
        public ShaderParameterType Type;
        public void* Resource;
        public uint InitialCount; // only used by UAVs
    }
}