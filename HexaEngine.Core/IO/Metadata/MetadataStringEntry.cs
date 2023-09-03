namespace HexaEngine.Core.IO.Metadata
{
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Text;

    public unsafe class MetadataStringEntry : MetadataEntry
    {
        public string? Value;
        public override MetadataType Type => MetadataType.String;

        public override void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            base.Read(stream, encoding, endianness);
            Value = stream.ReadString(encoding, endianness);
        }

        public override void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            base.Write(stream, encoding, endianness);
            stream.WriteString(Value, encoding, endianness);
        }
    }
}