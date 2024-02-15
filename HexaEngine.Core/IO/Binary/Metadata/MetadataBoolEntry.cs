namespace HexaEngine.Core.IO.Binary.Metadata
{
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents a boolean entry in metadata.
    /// </summary>
    public unsafe class MetadataBoolEntry : MetadataEntry
    {
        /// <summary>
        /// Gets or sets the boolean value of the entry.
        /// </summary>
        public bool Value;

        /// <summary>
        /// Gets the type of the metadata entry.
        /// </summary>
        public override MetadataType Type => MetadataType.Bool;

        /// <inheritdoc/>
        public override MetadataEntry Clone()
        {
            return new MetadataBoolEntry() { Value = Value };
        }

        /// <summary>
        /// Reads the boolean entry from the specified stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding used to read strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        public override void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            base.Read(stream, encoding, endianness);
            Value = stream.ReadByte() == 1;
        }

        /// <summary>
        /// Writes the boolean entry to the specified stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="encoding">The encoding used to write strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        public override void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            base.Write(stream, encoding, endianness);
            stream.WriteByte((byte)(Value ? 1u : 0u));
        }
    }
}