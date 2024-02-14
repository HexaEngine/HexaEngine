namespace HexaEngine.Core.IO.Materials
{
    using HexaEngine.Mathematics;
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Structure representing a material shader.
    /// </summary>
    public struct MaterialShader
    {
        /// <summary>
        /// The type of the material shader.
        /// </summary>
        public MaterialShaderType Type;

        /// <summary>
        /// The source code of the material shader.
        /// </summary>
        public string Source;

        /// <summary>
        /// The compiled bytecode of the material shader.
        /// </summary>
        public byte[]? Bytecode;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialShader"/> struct.
        /// </summary>
        /// <param name="type">The type of the material shader.</param>
        /// <param name="source">The source code of the material shader.</param>
        /// <param name="bytecode">The compiled bytecode of the material shader (optional).</param>
        public MaterialShader(MaterialShaderType type, string source, byte[]? bytecode = null)
        {
            Type = type;
            Source = source;
            Bytecode = bytecode;
        }

        /// <summary>
        /// Reads a <see cref="MaterialShader"/> from the provided stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="src">The stream to read from.</param>
        /// <param name="encoding">The encoding used for reading strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        /// <returns>The read <see cref="MaterialShader"/>.</returns>
        public static MaterialShader Read(Stream src, Encoding encoding, Endianness endianness)
        {
            MaterialShader shader;
            shader.Type = (MaterialShaderType)src.ReadInt32(endianness);
            shader.Source = src.ReadString(encoding, endianness) ?? string.Empty;
            var bytecount = src.ReadInt32(endianness);
            if (bytecount > 0)
            {
                shader.Bytecode = src.Read(bytecount);
            }
            else
            {
                shader.Bytecode = null;
            }
            return shader;
        }

        /// <summary>
        /// Writes the <see cref="MaterialShader"/> to the provided stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="dst">The stream to write to.</param>
        /// <param name="encoding">The encoding used for writing strings.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        public readonly void Write(Stream dst, Encoding encoding, Endianness endianness)
        {
            dst.WriteInt32((int)Type, endianness);
            dst.WriteString(Source, encoding, endianness);
            dst.WriteInt32(Bytecode?.Length ?? 0, endianness);
            if (Bytecode != null)
            {
                dst.Write(Bytecode);
            }
        }

        /// <summary>
        /// Deep clones a <see cref="MaterialShader"/> instance.
        /// </summary>
        /// <returns>The deep cloned <see cref="MaterialShader"/> instance.</returns>
        public MaterialShader Clone()
        {
            MaterialShader shader;
            shader.Type = Type;
            shader.Source = (string)Source.Clone();
            shader.Bytecode = [.. Bytecode];
            return shader;
        }
    }
}