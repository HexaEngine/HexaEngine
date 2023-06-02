namespace HexaEngine.Core.IO.Metadata
{
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;
    using System.Text;

    public unsafe class Metadata
    {
        public Dictionary<string, MetadataEntry> Properties;

        public Metadata(Dictionary<string, MetadataEntry> properties)
        {
            Properties = properties;
        }

        private Metadata()
        {
        }

        public static Metadata ReadFrom(Stream stream, Encoding encoding, Endianness endianness)
        {
            Metadata metadata = new();
            metadata.Read(stream, encoding, endianness);
            return metadata;
        }

        public void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            var propertiesCount = stream.ReadInt(endianness);

            Properties = new();
            for (int i = 0; i < propertiesCount; i++)
            {
                // TODO: Implement Properties properly
                var key = stream.ReadString(encoding, endianness) ?? string.Empty;
                MetadataEntry entry = new();
                entry.Read(stream, encoding, endianness);
                Properties.Add(key, entry);
            }
        }

        public void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            var props = Properties.ToList();
            stream.WriteInt(Properties.Count, endianness);
            for (int i = 0; i < Properties.Count; i++)
            {
                var pair = props[i];
                stream.WriteString(pair.Key, encoding, endianness);
                pair.Value.Write(stream, encoding, endianness);
            }
        }
    }
}