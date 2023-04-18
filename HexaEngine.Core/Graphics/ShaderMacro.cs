namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.Text;

    public struct ShaderMacro
    {
        public string Name;
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

        public bool Equals(ShaderMacro other)
        {
            return string.Equals(Name, other.Name) && string.Equals(Definition, other.Definition);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is ShaderMacro shaderMacro && Equals(shaderMacro);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 0;
                if (!string.IsNullOrEmpty(Name))
                {
                    hashCode = Name.GetHashCode() * 397;
                }

                if (!string.IsNullOrEmpty(Definition))
                {
                    hashCode ^= Definition.GetHashCode();
                }

                return hashCode;
            }
        }

        public static bool operator ==(ShaderMacro left, ShaderMacro right) => left.Equals(right);

        public static bool operator !=(ShaderMacro left, ShaderMacro right) => !left.Equals(right);

        public override string ToString()
        {
            return $"{Name}: {Definition}";
        }

        public static int Write(Span<byte> dst, ShaderMacro macro, Encoder encoder)
        {
            int idx = 0;
            idx += dst[0..].WriteString(macro.Name, encoder);
            idx += dst[idx..].WriteString(macro.Definition, encoder);
            return idx;
        }

        public static int Read(ReadOnlySpan<byte> src, Decoder decoder, out ShaderMacro macro)
        {
            int idx = 0;
            idx += src[0..].ReadString(out string name, decoder);
            idx += src[idx..].ReadString(out string definition, decoder);
            macro = new ShaderMacro(name, definition);
            return idx;
        }

        public void Write(Stream stream, Encoding encoder, Endianness endianness)
        {
            stream.WriteString(Name, encoder, endianness);
            stream.WriteString(Definition, encoder, endianness);
        }

        public void Read(Stream stream, Encoding decoder, Endianness endianness)
        {
            Name = stream.ReadString(decoder, endianness);
            Definition = stream.ReadString(decoder, endianness);
        }

        public int GetSize(Encoder encoder)
        {
            return Name.SizeOf(encoder) + Definition.SizeOf(encoder);
        }
    }
}