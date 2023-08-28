namespace HexaEngine.Core.IO.Metadata
{
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Text;

    public unsafe class MetadataMetadataEntry : MetadataEntry
    {
        public Metadata Value;
        public override MetadataType Type => MetadataType.Metadata;

        public MetadataMetadataEntry(Metadata value)
        {
            Value = value;
        }

        internal MetadataMetadataEntry()
        {
        }

        public override void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            base.Read(stream, encoding, endianness);
            Value = Metadata.ReadFrom(stream, encoding, endianness);
        }

        public override void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            base.Write(stream, encoding, endianness);
            Value.Write(stream, encoding, endianness);
        }
    }
}