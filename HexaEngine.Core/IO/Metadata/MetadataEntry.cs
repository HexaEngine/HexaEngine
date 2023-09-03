namespace HexaEngine.Core.IO.Metadata
{
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Text;

    public abstract unsafe class MetadataEntry
    {
        public abstract MetadataType Type { get; }

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

        public virtual void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
        }

        public virtual void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteInt32((int)Type, endianness);
        }
    }
}