namespace HexaEngine.Core.IO.Metadata
{
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Text;

    public unsafe class MetadataInt32Entry : MetadataEntry
    {
        public int Value;
        public override MetadataType Type => MetadataType.Int32;

        public override void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            base.Read(stream, encoding, endianness);
            Value = stream.ReadInt32(endianness);
        }

        public override void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            base.Write(stream, encoding, endianness);
            stream.WriteInt32(Value, endianness);
        }
    }
}