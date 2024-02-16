namespace HexaEngine.Core.IO.Binary.Materials
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System;
    using System.Text;

    /// <summary>
    /// Structure representing a material shader pass.
    /// </summary>
    public struct MaterialShaderPass
    {
        /// <summary>
        /// The name of the pass.
        /// </summary>
        public string Name;

        /// <summary>
        /// The entry point of the pass.
        /// </summary>
        public string Entry;

        /// <summary>
        /// The defining macros for the pass.
        /// </summary>
        public ShaderMacro[] Macros;

        /// <summary>
        /// Reads a <see cref="MaterialShaderPass"/> from the provided stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="src">The stream to read from.</param>
        /// <param name="encoding">The encoding used for reading strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        /// <returns>The read <see cref="MaterialShaderPass"/>.</returns>
        public static MaterialShaderPass Read(Stream src, Encoding encoding, Endianness endianness)
        {
            MaterialShaderPass pass;

            pass.Name = src.ReadString(encoding, endianness) ?? string.Empty;
            pass.Entry = src.ReadString(encoding, endianness) ?? string.Empty;
            var macroCount = src.ReadInt32(endianness);

            pass.Macros = new ShaderMacro[macroCount];

            for (int i = 0; i < macroCount; i++)
            {
                pass.Macros[i].Read(src, encoding, endianness);
            }

            return pass;
        }

        /// <summary>
        /// Writes the <see cref="MaterialShaderPass"/> to the provided stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="dst">The stream to write to.</param>
        /// <param name="encoding">The encoding used for writing strings.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        public readonly void Write(Stream dst, Encoding encoding, Endianness endianness)
        {
            dst.WriteString(Name, encoding, endianness);
            dst.WriteString(Entry, encoding, endianness);
            dst.WriteInt32(Macros.Length, endianness);
            for (int i = 0; i < Macros.Length; i++)
            {
                Macros[i].Write(dst, encoding, endianness);
            }
        }

        /// <summary>
        /// Deep clones a <see cref="MaterialShaderPass"/> instance.
        /// </summary>
        /// <returns>The deep cloned <see cref="MaterialShaderPass"/> instance.</returns>
        public MaterialShaderPass Clone()
        {
            MaterialShaderPass pass;
            pass.Name = (string)Name.Clone();
            pass.Entry = (string)Entry.Clone();
            pass.Macros = [.. Macros];
            return pass;
        }
    }
}