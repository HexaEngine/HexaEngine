namespace HexaEngine.Core.Graphics.Shaders.Reflection
{
    using Hexa.NET.SPIRVReflect;

    public struct DescriptorBindingInfo
    {
        public string Name;
        public SpvReflectDescriptorType Type;
        public int BufferSize;
        public ConstantBufferMemberInfo[] Members;
    }
}