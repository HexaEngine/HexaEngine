namespace HexaEngine.Core.Graphics.Shaders
{
    using System;

    /// <summary>
    /// Represents the Intermediate Language (IL) of a compiled shader in SPIR-V format.
    /// </summary>
    public unsafe struct ShaderSpirvIL
    {
        /// <summary>
        /// Gets or sets a pointer to the bytecode of the compiled shader.
        /// </summary>
        public byte* Bytecode;

        /// <summary>
        /// Gets or sets the length of the bytecode in bytes.
        /// </summary>
        public int Length;

        /// <summary>
        /// Returns a readonly span representing the bytecode of the compiled shader.
        /// </summary>
        /// <returns>A readonly span of bytes.</returns>
        public readonly Span<byte> AsSpan()
        {
            return new Span<byte>(Bytecode, Length);
        }

        /// <summary>
        /// Converts the bytecode to a byte array.
        /// </summary>
        /// <returns>A byte array representing the bytecode of the compiled shader.</returns>
        public readonly byte[] ToArray()
        {
            return AsSpan().ToArray();
        }

        /// <summary>
        /// Releases the resources associated with the bytecode.
        /// </summary>
        public void Release()
        {
            Free(Bytecode);
            Bytecode = null;
            Length = 0;
        }
    }
}