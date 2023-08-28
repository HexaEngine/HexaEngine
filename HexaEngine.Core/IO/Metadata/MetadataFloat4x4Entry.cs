namespace HexaEngine.Core.IO.Metadata
{
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Numerics;
    using System.Text;

    public unsafe class MetadataFloat4x4Entry : MetadataEntry
    {
        public Matrix4x4 Value;

        public override MetadataType Type => MetadataType.Float4x4;

        public override void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            base.Read(stream, encoding, endianness);
            Value = stream.ReadMatrix4x4(endianness);
        }

        public override void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            base.Write(stream, encoding, endianness);
            stream.WriteMatrix4x4(Value, endianness);
        }
    }
}