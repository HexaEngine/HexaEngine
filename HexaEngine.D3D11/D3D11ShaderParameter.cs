namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;

    public unsafe struct D3D11ShaderParameter
    {
        public char* Name;
        public uint Hash;
        public uint Index;
        public uint Size;
        public ShaderStage Stage;
        public ShaderParameterType Type;
    }
}