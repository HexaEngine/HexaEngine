namespace HexaEngine.Core.IO.Metadata
{
    using HexaEngine.Mathematics;
    using System.Text;

    public unsafe class Metadata
    {
        public Dictionary<string, MetadataEntry> Properties;

        public static readonly Metadata Empty = new(new());

        public Metadata(Dictionary<string, MetadataEntry>? properties = null)
        {
            Properties = properties ?? new();
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
            var propertiesCount = stream.ReadInt32(endianness);

            Properties = new();
            for (int i = 0; i < propertiesCount; i++)
            {
                var key = stream.ReadString(encoding, endianness) ?? string.Empty;
                MetadataEntry entry = MetadataEntry.ReadFrom(stream, encoding, endianness);
                Properties.Add(key, entry);
            }
        }

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