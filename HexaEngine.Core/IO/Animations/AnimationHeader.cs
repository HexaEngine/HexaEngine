namespace HexaEngine.Core.IO.Animations
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.Text;

    public struct AnimationHeader
    {
        public static readonly byte[] MagicNumber = { 0x54, 0x72, 0x61, 0x6e, 0x73, 0x41, 0x6e, 0x69, 0x6d, 0x61, 0x74, 0x6F, 0x6E, 0x00 };
        public static readonly Version Version = 1;
        public static readonly Version MinVersion = 1;
        public Endianness Endianness;
        public Encoding Encoding;
        public Compression Compression;

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
        }

        public void Write(Stream stream)
        {
            stream.Write(MagicNumber);
            stream.WriteByte((byte)Endianness);
            stream.WriteUInt64(Version, Endianness);
            stream.WriteInt32(Encoding.CodePage, Endianness);
            stream.WriteInt32((int)Compression, Endianness);
        }
    }
}