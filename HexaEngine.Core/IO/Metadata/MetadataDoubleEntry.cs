namespace HexaEngine.Core.IO.Metadata
{
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Text;

    public unsafe class MetadataDoubleEntry : MetadataEntry
    {
        public double Value;
        public override MetadataType Type => MetadataType.Double;

        public override void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            base.Read(stream, encoding, endianness);
            Value = stream.ReadDouble(endianness);
        }

        public override void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            base.Write(stream, encoding, endianness);
            stream.WriteDouble(Value, endianness);
        }
    }
}