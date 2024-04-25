namespace HexaEngine.Core.Graphics.Shaders.Reflection
{
    using Silk.NET.SPIRV.Reflect;

    public struct DescriptorBindingInfo
    {
        public string Name;
        public DescriptorType Type;
        public int BufferSize;
        public ConstantBufferMemberInfo[] Members;
    }
}