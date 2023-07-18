namespace HexaEngine.Core.IO.Animations
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.Text;

    public struct AnimationHeader
    {
        public static readonly byte[] MagicNumber = { 0x54, 0x72, 0x61, 0x6e, 0x73, 0x41, 0x6e, 0x69, 0x6d, 0x61, 0x74, 0x6F, 0x6E, 0x00 };
        public const ulong Version = 1;
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
            if (!stream.Compare(Version, Endianness))
            {
                throw new InvalidDataException("Version mismatch");
            }

            Encoding = Encoding.GetEncoding(stream.ReadInt(Endianness));
            Compression = (Compression)stream.ReadInt(Endianness);
        }

        public void Write(Stream stream)
        {
            stream.Write(MagicNumber);
            stream.WriteByte((byte)Endianness);
            stream.WriteUInt64(Version, Endianness);
            stream.WriteInt(Encoding.CodePage, Endianness);
            stream.WriteInt((int)Compression, Endianness);
        }
    }
}