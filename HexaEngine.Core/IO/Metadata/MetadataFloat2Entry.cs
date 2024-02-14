namespace HexaEngine.Core.IO.Metadata
{
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Numerics;
    using System.Text;

    /// <summary>
    /// Represents a metadata entry with a two-dimensional single-precision floating-point vector value.
    /// </summary>
    public unsafe class MetadataFloat2Entry : MetadataEntry
    {
        /// <summary>
        /// Gets or sets the two-dimensional single-precision floating-point vector value of the metadata entry.
        /// </summary>
        public Vector2 Value;

        /// <summary>
        /// Gets the type of the metadata entry.
        /// </summary>
        public override MetadataType Type => MetadataType.Float2;

        /// <inheritdoc/>
        public override MetadataEntry Clone()
        {
            return new MetadataFloat2Entry() { Value = Value };
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
            Value = stream.ReadVector2(endianness);
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
            stream.WriteVector2(Value, endianness);
        }
    }
}