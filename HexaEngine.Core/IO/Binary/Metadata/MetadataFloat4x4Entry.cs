namespace HexaEngine.Core.IO.Binary.Metadata
{
    using Hexa.NET.Mathematics;
    using System.IO;
    using System.Numerics;
    using System.Text;

    /// <summary>
    /// Represents a metadata entry with a four-by-four single-precision floating-point matrix value.
    /// </summary>
    public unsafe class MetadataFloat4x4Entry : MetadataEntry
    {
        /// <summary>
        /// Gets or sets the four-by-four single-precision floating-point matrix value of the metadata entry.
        /// </summary>
        public Matrix4x4 Value;

        /// <summary>
        /// Gets the type of the metadata entry.
        /// </summary>
        public override MetadataType Type => MetadataType.Float4x4;

        /// <inheritdoc/>
        public override MetadataEntry Clone()
        {
            return new MetadataFloat4x4Entry() { Value = Value };
        }

        /// <summary>
        /// Reads the metadata entry from the specified stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding used to read strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        public override void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            base.Read(stream, encoding, endianness);
            Value = stream.ReadMatrix4x4(endianness);
        }

        /// <summary>
        /// Writes the metadata entry to the specified stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="encoding">The encoding used to write strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        public override void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            base.Write(stream, encoding, endianness);
            stream.WriteMatrix4x4(Value, endianness);
        }
    }
}