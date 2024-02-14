namespace HexaEngine.Core.IO.Metadata
{
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents the base class for entries in metadata.
    /// </summary>
    public abstract unsafe class MetadataEntry
    {
        /// <summary>
        /// Gets the type of the metadata entry.
        /// </summary>
        public abstract MetadataType Type { get; }

        /// <summary>
        /// Reads a metadata entry from the specified stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="src">The stream to read from.</param>
        /// <param name="encoding">The encoding used to read strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        /// <returns>The read metadata entry.</returns>
        public static MetadataEntry ReadFrom(Stream src, Encoding encoding, Endianness endianness)
        {
            MetadataEntry? entry = null;
            MetadataType type = (MetadataType)src.ReadInt32(endianness);
            switch (type)
            {
                case MetadataType.Bool:
                    entry = new MetadataBoolEntry(); break;
                case MetadataType.Int16:
                    entry = new MetadataInt16Entry(); break;

                case MetadataType.UInt16:
                    entry = new MetadataUInt16Entry(); break;

                case MetadataType.Int32:
                    entry = new MetadataInt32Entry(); break;

                case MetadataType.UInt32:
                    entry = new MetadataUInt32Entry(); break;

                case MetadataType.Int64:
                    entry = new MetadataInt64Entry(); break;

                case MetadataType.UInt64:
                    entry = new MetadataUInt64Entry(); break;

                case MetadataType.Float:
                    entry = new MetadataFloatEntry(); break;

                case MetadataType.Double:
                    entry = new MetadataDoubleEntry(); break;

                case MetadataType.String:
                    entry = new MetadataStringEntry(); break;

                case MetadataType.Float2:
                    entry = new MetadataFloat2Entry(); break;

                case MetadataType.Float3:
                    entry = new MetadataFloat3Entry(); break;

                case MetadataType.Float4:
                    entry = new MetadataFloat4Entry(); break;

                case MetadataType.Float4x4:
                    entry = new MetadataFloat4x4Entry(); break;

                case MetadataType.Metadata:
                    entry = new MetadataMetadataEntry(); break;
            }

            if (entry == null)
                throw new NotSupportedException($"The type {type} is not supported");

            entry.Read(src, encoding, endianness);
            return entry;
        }

        /// <summary>
        /// Reads the metadata entry from the specified stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding used to read strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        public virtual void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
        }

        /// <summary>
        /// Writes the metadata entry to the specified stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="encoding">The encoding used to write strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        public virtual void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteInt32((int)Type, endianness);
        }

        /// <summary>
        /// Deep clones a <see cref="MetadataEntry"/> instance.
        /// </summary>
        /// <returns>The deep cloned <see cref="MetadataEntry"/> instance.</returns>
        public abstract MetadataEntry Clone();
    }
}