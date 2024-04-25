namespace HexaEngine.Core.Graphics.Shaders.Reflection
{
    public unsafe struct ConstantBufferMemberInfo
    {
        public string Name;

        public uint Offset;
        public uint AbsoluteOffset;
        public uint Size;
        public uint PaddedSize;
        public TypeInfo* Type;

        public void Release()
        {
            if (Type != null)
            {
                Type->Release();

                Free(Type);
                Type = null;
            }
        }
    }
}