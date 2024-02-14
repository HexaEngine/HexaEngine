namespace HexaEngine.Core.IO.Metadata
{
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents a metadata entry with a 64-bit signed integer value.
    /// </summary>
    public unsafe class MetadataInt64Entry : MetadataEntry
    {
        /// <summary>
        /// Gets or sets the 64-bit signed integer value of the metadata entry.
        /// </summary>
        public long Value;

        /// <summary>
        /// Gets the type of the metadata entry.
        /// </summary>
        public override MetadataType Type => MetadataType.Int64;

        /// <inheritdoc/>
        public override MetadataEntry Clone()
        {
            return new MetadataInt64Entry() { Value = Value };
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
            Value = stream.ReadInt64(endianness);
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
            stream.WriteInt64(Value, endianness);
        }
    }
}