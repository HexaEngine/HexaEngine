namespace HexaEngine.Core.IO.Terrains
{
    using HexaEngine.Mathematics;
    using System.Text;

    public struct TerrainHeader
    {
        public static readonly byte[] MagicNumber = { 0x54, 0x72, 0x61, 0x6E, 0x73, 0x53, 0x68, 0x61, 0x64, 0x65, 0x72, 0x00 };
        public static readonly Version Version = 1;
        public static readonly Version MinVersion = 1;

        public Endianness Endianness;
        public Encoding Encoding;
        public Compression Compression;
        public string MaterialLibrary;
        public uint X;
        public uint Y;
        public uint LODLevels;
        public ulong ContentStart;

        public void Read(Stream stream)
        {
            if (!stream.Compare(MagicNumber))
            {
                throw new InvalidDataException("Magic number mismatch");
            }

            Endianness = (Endianness)stream.ReadByte();
            if (!stream.CompareVersion(MinVersion, Version, Endianness, out var version))
            {
                throw new InvalidDataException($"Version mismatch, file: {version} min: {MinVersion} max: {Version}");
            }

            Encoding = Encoding.GetEncoding(stream.ReadInt32(Endianness));
            Compression = (Compression)stream.ReadInt32(Endianness);
            MaterialLibrary = stream.ReadString(Encoding, Endianness) ?? string.Empty;
            LODLevels = stream.ReadUInt32(Endianness);
            ContentStart = (ulong)stream.Position;
        }

        public void Write(Stream stream)
        {
            stream.Write(MagicNumber);
            stream.WriteByte((byte)Endianness);
            stream.WriteUInt64(Version, Endianness);
            stream.WriteInt32(Encoding.CodePage, Endianness);
            stream.WriteInt32((int)Compression, Endianness);
            stream.WriteString(MaterialLibrary, Encoding, Endianness);
            stream.WriteUInt32(LODLevels, Endianness);
        }
    }
}