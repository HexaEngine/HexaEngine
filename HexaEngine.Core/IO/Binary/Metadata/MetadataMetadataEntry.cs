namespace HexaEngine.Core.IO.Binary.Metadata
{
    using Hexa.NET.Mathematics;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents a metadata entry containing another metadata instance.
    /// </summary>
    public unsafe class MetadataMetadataEntry : MetadataEntry
    {
        /// <summary>
        /// Gets or sets the metadata value of the entry.
        /// </summary>
        public Metadata Value;

        /// <summary>
        /// Gets the type of the metadata entry.
        /// </summary>
        public override MetadataType Type => MetadataType.Metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataMetadataEntry"/> class with the specified metadata value.
        /// </summary>
        /// <param name="value">The metadata value of the entry.</param>
        public MetadataMetadataEntry(Metadata value)
        {
            Value = value;
        }

#nullable disable

        /// <summary>
        /// Internal constructor for creating an instance without initializing the metadata value.
        /// </summary>
        internal MetadataMetadataEntry()
        {
        }

#nullable restore

        /// <inheritdoc/>
        public override MetadataEntry Clone()
        {
            return new MetadataMetadataEntry() { Value = Value.Clone() };
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
            Value = Metadata.ReadFrom(stream, encoding, endianness);
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
            Value.Write(stream, encoding, endianness);
        }
    }
}