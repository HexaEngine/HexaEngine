namespace HexaEngine.Core.IO.Binary.Metadata
{
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents a double precision floating-point number entry in metadata.
    /// </summary>
    public unsafe class MetadataDoubleEntry : MetadataEntry
    {
        /// <summary>
        /// Gets or sets the double precision floating-point value of the entry.
        /// </summary>
        public double Value;

        /// <summary>
        /// Gets the type of the metadata entry.
        /// </summary>
        public override MetadataType Type => MetadataType.Double;

        /// <inheritdoc/>
        public override MetadataEntry Clone()
        {
            return new MetadataDoubleEntry() { Value = Value };
        }

        /// <summary>
        /// Reads the double precision floating-point number entry from the specified stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding used to read strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        public override void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            base.Read(stream, encoding, endianness);
            Value = stream.ReadDouble(endianness);
        }

        /// <summary>
        /// Writes the double precision floating-point number entry to the specified stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="encoding">The encoding used to write strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        public override void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            base.Write(stream, encoding, endianness);
            stream.WriteDouble(Value, endianness);
        }
    }
}