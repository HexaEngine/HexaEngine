﻿namespace HexaEngine.Core.IO.Metadata
{
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents a metadata entry storing a 32-bit unsigned integer value.
    /// </summary>
    public unsafe class MetadataUInt32Entry : MetadataEntry
    {
        /// <summary>
        /// Gets or sets the value of the metadata entry.
        /// </summary>
        public uint Value;

        /// <summary>
        /// Gets the type of the metadata entry.
        /// </summary>
        public override MetadataType Type => MetadataType.UInt32;

        /// <summary>
        /// Reads the metadata entry from the specified stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding used for string-related operations.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        public override void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            base.Read(stream, encoding, endianness);
            Value = stream.ReadUInt32(endianness);
        }

        /// <summary>
        /// Writes the metadata entry to the specified stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="encoding">The encoding used for string-related operations.</param>
        /// <param name="endianness">The endianness to use when writing data to the stream.</param>
        public override void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            base.Write(stream, encoding, endianness);
            stream.WriteUInt32(Value, endianness);
        }
    }
}