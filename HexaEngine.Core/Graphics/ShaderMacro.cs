namespace HexaEngine.Core.Graphics
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.IO;
    using System.Text;

    /// <summary>
    /// Represents a shader macro with a name and definition.
    /// </summary>
    public struct ShaderMacro : IEquatable<ShaderMacro>
    {
        /// <summary>
        /// Gets or sets the name of the shader macro.
        /// </summary>
        public string Name;

        /// <summary>
        /// Gets or sets the definition of the shader macro.
        /// </summary>
        public string Definition;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderMacro"/> struct.
        /// </summary>
        /// <param name="name">The macro name.</param>
        /// <param name="definition">The macro definition.</param>
        public ShaderMacro(string name, object? definition)
        {
            Name = name;
            Definition = definition?.ToString() ?? throw new ArgumentNullException(nameof(definition));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderMacro"/> struct.
        /// </summary>
        /// <param name="name">The macro name.</param>
        /// <param name="definition">The macro definition.</param>
        public ShaderMacro(string name, string definition = "")
        {
            Name = name;
            Definition = definition;
        }

        /// <inheritdoc/>
        public readonly bool Equals(ShaderMacro other)
        {
            return string.Equals(Name, other.Name) && string.Equals(Definition, other.Definition);
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            return obj is ShaderMacro shaderMacro && Equals(shaderMacro);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Name, Definition);
        }

        /// <summary>
        /// Equality operator for <see cref="ShaderMacro"/>.
        /// </summary>
        public static bool operator ==(ShaderMacro left, ShaderMacro right) => left.Equals(right);

        /// <summary>
        /// Inequality operator for <see cref="ShaderMacro"/>.
        /// </summary>
        public static bool operator !=(ShaderMacro left, ShaderMacro right) => !left.Equals(right);

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"{Name}: {Definition}";
        }

        /// <summary>
        /// Writes the <see cref="ShaderMacro"/> to a span of bytes.
        /// </summary>
        public static int Write(Span<byte> dst, ShaderMacro macro, Encoding encoder)
        {
            int idx = 0;
            idx += dst[0..].WriteString(macro.Name, encoder);
            idx += dst[idx..].WriteString(macro.Definition, encoder);
            return idx;
        }

        /// <summary>
        /// Reads a <see cref="ShaderMacro"/> from a span of bytes.
        /// </summary>
        public static int Read(ReadOnlySpan<byte> src, Encoding decoder, out ShaderMacro macro)
        {
            int idx = 0;
            idx += src[0..].ReadString(decoder, out string name);
            idx += src[idx..].ReadString(decoder, out string definition);
            macro = new ShaderMacro(name, definition);
            return idx;
        }

        /// <summary>
        /// Writes the <see cref="ShaderMacro"/> to a stream.
        /// </summary>
        public readonly void Write(Stream stream, Encoding encoder, Endianness endianness)
        {
            stream.WriteString(Name, encoder, endianness);
            stream.WriteString(Definition, encoder, endianness);
        }

        /// <summary>
        /// Reads a <see cref="ShaderMacro"/> from a stream.
        /// </summary>
        public void Read(Stream stream, Encoding decoder, Endianness endianness)
        {
            Name = stream.ReadString(decoder, endianness) ?? string.Empty;
            Definition = stream.ReadString(decoder, endianness) ?? string.Empty;
        }

        /// <summary>
        /// Gets the size of the <see cref="ShaderMacro"/> in bytes.
        /// </summary>
        public readonly int GetSize(Encoder encoder)
        {
            return Name.SizeOf(encoder) + Definition.SizeOf(encoder);
        }
    }
}