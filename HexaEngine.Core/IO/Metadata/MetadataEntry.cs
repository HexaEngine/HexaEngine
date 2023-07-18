namespace HexaEngine.Core.IO.Metadata
{
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Numerics;
    using System.Text;

    public unsafe class MetadataEntry
    {
        public MetadataType Type;
        public void* Data;

        public void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            throw new NotImplementedException();
        }

        public void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            throw new NotImplementedException();
        }
    }
}