﻿namespace HexaEngine.Core.Graphics
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
            Shader* result = AllocT<Shader>();
            result->Bytecode = AllocCopy(Bytecode, (int)Length);
            result->Length = Length;
            return result;
        }

        public static Shader* CreateFrom(byte[] bytes)
        {
            Shader* result = AllocT<Shader>();
            fixed (byte* ptr = bytes)
            {
                result->Bytecode = AllocCopy(ptr, bytes.Length);
            }

            result->Length = (nuint)bytes.Length;
            return result;
        }

        public void Release()
        {
            Marshal.FreeHGlobal((nint)Bytecode);
        }

        public Span<byte> AsSpan()
        {
            return new Span<byte>(Bytecode, (int)Length);
        }

        public byte[] ToArray()
        {
            byte[] bytes = new byte[Length];
            fixed (byte* ptr = bytes)
            {
                Memcpy(Bytecode, ptr, Length, Length);
            }
            return bytes;
        }
    }
}