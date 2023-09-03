namespace HexaEngine.Core.IO.Materials
{
    using HexaEngine.Mathematics;
    using System.Text;

    public struct MaterialLibraryHeader
    {
        public static readonly byte[] MagicNumber = { 0x54, 0x72, 0x61, 0x6E, 0x73, 0x4D, 0x61, 0x74, 0x65, 0x72, 0x69, 0x61, 0x6C, 0x00 };
        public static readonly Version Version = 4;
        public static readonly Version MinVersion = 4;

        public Endianness Endianness;
        public Encoding Encoding;
        public Compression Compression;
        public uint MaterialCount;

        public void Read(Stream stream)
        {
            if (!stream.Compare(MagicNumber))
            {
                throw new InvalidDataException();
            }

            Endianness = (Endianness)stream.ReadByte();
            if (!stream.CompareVersion(MinVersion, Version, Endianness, out var version))
            {
                throw new InvalidDataException($"Version mismatch, file: {version} min: {MinVersion} max: {Version}");
            }

            Encoding = Encoding.GetEncoding(stream.ReadInt32(Endianness));
            Compression = (Compression)stream.ReadInt32(Endianness);
            MaterialCount = stream.ReadUInt32(Endianness);
        }

        public void Write(Stream stream)
        {
            stream.Write(MagicNumber);
            stream.WriteByte((byte)Endianness);
            stream.WriteUInt64(Version, Endianness);
            stream.WriteInt32(Encoding.CodePage, Endianness);
            stream.WriteInt32((int)Compression, Endianness);
            stream.WriteUInt32(MaterialCount, Endianness);
        }
    }
}