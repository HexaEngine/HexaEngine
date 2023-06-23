namespace HexaEngine.Core.Graphics.Shaders
{
    using System;

    public unsafe struct ShaderSpirvIL
    {
        public byte* Bytecode;
        public int Length;

        public readonly Span<byte> AsSpan()
        {
            return new Span<byte>(Bytecode, Length);
        }

        public readonly byte[] ToArray()
        {
            return AsSpan().ToArray();
        }

        public void Release()
        {
            Free(Bytecode);
            Bytecode = null;
            Length = 0;
        }
    }
}