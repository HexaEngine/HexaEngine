namespace HexaEngine.Core.IO.Metadata
{
    using HexaEngine.Mathematics;
    using System.Text;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Represents metadata associated with an object.
    /// </summary>
    public unsafe class Metadata
    {
        /// <summary>
        /// Gets or sets the dictionary of properties in the metadata.
        /// </summary>
        public Dictionary<string, MetadataEntry> Properties;

        /// <summary>
        /// Represents an empty metadata instance.
        /// </summary>
        public static readonly Metadata Empty = new(new());

        /// <summary>
        /// Initializes a new instance of the <see cref="Metadata"/> class with the specified properties.
        /// </summary>
        /// <param name="properties">The dictionary of properties.</param>
        public Metadata(Dictionary<string, MetadataEntry> properties)
        {
            Properties = properties;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Metadata"/> class with an empty set of properties.
        /// </summary>
        public Metadata()
        {
            Properties = new();
        }

        /// <summary>
        /// Reads metadata from the specified stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to read metadata from.</param>
        /// <param name="encoding">The encoding used to read strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        /// <returns>The read metadata.</returns>
        public static Metadata ReadFrom(Stream stream, Encoding encoding, Endianness endianness)
        {
            Metadata metadata = new();
            metadata.Read(stream, encoding, endianness);
            return metadata;
        }

        /// <summary>
        /// Reads metadata from the specified stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to read metadata from.</param>
        /// <param name="encoding">The encoding used to read strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        public void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            var propertiesCount = stream.ReadInt32(endianness);

            Properties.Clear();
            Properties.EnsureCapacity(propertiesCount);

            for (int i = 0; i < propertiesCount; i++)
            {
                var key = stream.ReadString(encoding, endianness) ?? string.Empty;
                MetadataEntry entry = MetadataEntry.ReadFrom(stream, encoding, endianness);
                Properties.Add(key, entry);
            }
        }

        /// <summary>
        /// Writes metadata to the specified stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to write metadata to.</param>
        /// <param name="encoding">The encoding used to write strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        public void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            var props = Properties.ToList();
            stream.WriteInt32(Properties.Count, endianness);
            for (int i = 0; i < Properties.Count; i++)
            {
                var pair = props[i];
                stream.WriteString(pair.Key, encoding, endianness);
                pair.Value.Write(stream, encoding, endianness);
            }
        }

        /// <summary>
        /// Gets or adds a metadata entry of type T with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the metadata entry.</typeparam>
        /// <param name="key">The key of the metadata entry.</param>
        /// <returns>The metadata entry of type T.</returns>
        public T GetOrAdd<T>(string key) where T : MetadataEntry, new()
        {
            if (Properties.TryGetValue(key, out var entry))
            {
                return (T)entry;
            }
            entry = new T();
            Properties.Add(key, entry);
            return (T)entry;
        }
    }
}