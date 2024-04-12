namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Core.Unsafes;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents a shader with its bytecode and length.
    /// </summary>
    public unsafe struct Shader : IFreeable, IEquatable<Shader>
    {
        /// <summary>
        /// Pointer to the bytecode of the shader.
        /// </summary>
        public byte* Bytecode;

        /// <summary>
        /// Length of the shader bytecode.
        /// </summary>
        public nuint Length;

        /// <summary>
        /// Copies the shader bytecode to the provided <paramref name="span"/>.
        /// </summary>
        /// <param name="span">The span to which the bytecode will be copied.</param>
        public readonly void CopyTo(Span<byte> span)
        {
            fixed (byte* ptr = span)
            {
                Buffer.MemoryCopy(Bytecode, ptr, Length, Length);
            }
        }

        /// <summary>
        /// Creates a deep copy of the shader.
        /// </summary>
        /// <returns>A pointer to the cloned shader.</returns>
        public readonly Shader* Clone()
        {
            Shader* result = AllocT<Shader>();
            result->Bytecode = AllocCopyT(Bytecode, (int)Length);
            result->Length = Length;
            return result;
        }

        /// <summary>
        /// Creates a shader from the provided <paramref name="bytes"/>.
        /// </summary>
        /// <param name="bytes">The bytecode of the shader.</param>
        /// <returns>A pointer to the created shader.</returns>
        public static Shader* CreateFrom(byte[] bytes)
        {
            Shader* result = AllocT<Shader>();
            fixed (byte* ptr = bytes)
            {
                result->Bytecode = AllocCopyT(ptr, bytes.Length);
            }

            result->Length = (nuint)bytes.Length;
            return result;
        }

        /// <summary>
        /// Releases the allocated memory for the shader bytecode.
        /// </summary>
        public void Release()
        {
            Marshal.FreeHGlobal((nint)Bytecode);
            Bytecode = null;
            Length = 0;
        }

        /// <summary>
        /// Gets a span that represents the shader bytecode.
        /// </summary>
        /// <returns>A span representing the shader bytecode.</returns>
        public readonly Span<byte> AsSpan()
        {
            return new Span<byte>(Bytecode, (int)Length);
        }

        /// <summary>
        /// Converts the shader bytecode to a byte array.
        /// </summary>
        /// <returns>A byte array representing the shader bytecode.</returns>
        public readonly byte[] ToArray()
        {
            byte[] bytes = new byte[Length];
            fixed (byte* ptr = bytes)
            {
                Memcpy(Bytecode, ptr, Length, Length);
            }
            return bytes;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is Shader shader && Equals(shader);
        }

        public readonly bool Equals(Shader other)
        {
            return (Bytecode == other.Bytecode);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine((nint)Bytecode);
        }

        public static bool operator ==(Shader left, Shader right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Shader left, Shader right)
        {
            return !(left == right);
        }
    }
}