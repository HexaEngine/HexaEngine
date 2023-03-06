namespace HexaEngine.Core.IO.Materials
{
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;

    public struct MaterialLibraryHeader
    {
        public static readonly byte[] MagicNumber = { 0x54, 0x72, 0x61, 0x6E, 0x73, 0x4D, 0x61, 0x74, 0x65, 0x72, 0x69, 0x61, 0x6C, 0x00 };
        public const ulong Version = 2;
        public Endianness Endianness;
        public uint MaterialCount;

        public void Read(Stream stream)
        {
            if (!stream.Compare(MagicNumber))
                throw new InvalidDataException();
            Endianness = (Endianness)stream.ReadByte();
            if (!stream.Compare(Version, Endianness))
                throw new InvalidDataException();

            MaterialCount = stream.ReadUInt(Endianness);
        }

        public void Write(Stream stream)
        {
            stream.Write(MagicNumber);
            stream.WriteByte((byte)Endianness);
            stream.WriteUInt64(Version, Endianness);
            stream.WriteUInt(MaterialCount, Endianness);
        }
    }
}