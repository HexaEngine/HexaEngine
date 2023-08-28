namespace HexaEngine.Core.IO.Metadata
{
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Numerics;
    using System.Text;

    public unsafe class MetadataFloat2Entry : MetadataEntry
    {
        public Vector2 Value;
        public override MetadataType Type => MetadataType.Float2;

        public override void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            base.Read(stream, encoding, endianness);
            Value = stream.ReadVector2(endianness);
        }

        public override void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            base.Write(stream, encoding, endianness);
            stream.WriteVector2(Value, endianness);
        }
    }
}