namespace HexaEngine.Core.IO.Metadata
{
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Text;

    public unsafe class MetadataUInt32Entry : MetadataEntry
    {
        public uint Value;
        public override MetadataType Type => MetadataType.UInt32;

        public override void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            base.Read(stream, encoding, endianness);
            Value = stream.ReadUInt32(endianness);
        }

        public override void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            base.Write(stream, encoding, endianness);
            stream.WriteUInt32(Value, endianness);
        }
    }
}