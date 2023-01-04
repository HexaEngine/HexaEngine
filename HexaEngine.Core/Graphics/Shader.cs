namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Core.Unsafes;
    using System.Runtime.InteropServices;

    public unsafe struct Shader : IFreeable
    {
        public byte* Bytecode;
        public nuint Length;

        public void CopyTo(Span<byte> span)
        {
            fixed (byte* ptr = span)
            {
                Buffer.MemoryCopy(Bytecode, ptr, Length, Length);
            }
        }

        public Shader* Clone()
        {
            Shader* result = Alloc<Shader>();
            result->Bytecode = AllocCopy(Bytecode, (int)Length);
            result->Length = Length;
            return result;
        }

        public void Free()
        {
            Marshal.FreeHGlobal((nint)Bytecode);
        }
    }
}