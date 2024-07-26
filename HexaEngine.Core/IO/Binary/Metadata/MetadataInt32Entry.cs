namespace HexaEngine.Core.IO.Binary.Metadata
{
    using Hexa.NET.Mathematics;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents a metadata entry with a 32-bit signed integer value.
    /// </summary>
    public unsafe class MetadataInt32Entry : MetadataEntry
    {
        /// <summary>
        /// Gets or sets the 32-bit signed integer value of the metadata entry.
        /// </summary>
        public int Value;

        /// <summary>
        /// Gets the type of the metadata entry.
        /// </summary>
        public override MetadataType Type => MetadataType.Int32;

        /// <inheritdoc/>
        public override MetadataEntry Clone()
        {
            return new MetadataInt32Entry() { Value = Value };
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
            Value = stream.ReadInt32(endianness);
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
            stream.WriteInt32(Value, endianness);
        }
    }
}