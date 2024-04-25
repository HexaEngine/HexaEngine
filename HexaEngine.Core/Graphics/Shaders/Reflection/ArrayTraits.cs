namespace HexaEngine.Core.Graphics.Shaders.Reflection
{
    public struct ArrayTraits
    {
        public uint DimsCount;

        public unsafe fixed uint Dims[32];

        public unsafe fixed uint SpecConstantOpIds[32];

        public uint Stride;
    }
}