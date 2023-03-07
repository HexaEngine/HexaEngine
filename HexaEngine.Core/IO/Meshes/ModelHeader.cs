namespace HexaEngine.Core.IO.Meshes
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Text;

    public struct ModelHeader
    {
        public static readonly byte[] MagicNumber = { 0x54, 0x72, 0x61, 0x6e, 0x73, 0x4d, 0x65, 0x73, 0x68, 0x00 };
        public const ulong Version = 5;
        public Endianness Endianness;
        public Encoding Encoding;
        public Compression Compression;
        public ulong MeshCount;    
        public ulong ContentStart;

        public void Read(Stream stream)
        {
            if (!stream.Compare(MagicNumber))
                throw new InvalidDataException("Magic number mismatch");
            Endianness = (Endianness)stream.ReadByte();
            if (!stream.Compare(Version, Endianness))
                throw new InvalidDataException("Version mismatch");
            Encoding = Encoding.GetEncoding(stream.ReadInt(Endianness));
            Compression = (Compression)stream.ReadInt(Endianness);
            MeshCount = stream.ReadUInt64(Endianness);
            ContentStart = (ulong)stream.Position;
        }

        public void Write(Stream stream)
        {
            stream.Write(MagicNumber);
            stream.WriteByte((byte)Endianness);
            stream.WriteUInt64(Version, Endianness);
            stream.WriteInt(Encoding.CodePage, Endianness);
            stream.WriteInt((int)Compression, Endianness);
            stream.WriteUInt64(MeshCount, Endianness);
        }
    }
}