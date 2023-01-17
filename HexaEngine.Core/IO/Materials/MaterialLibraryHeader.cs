namespace HexaEngine.Core.IO.Materials
{
    public struct MaterialLibraryHeader
    {
        public static readonly byte[] MagicNumber = { 0x54, 0x72, 0x61, 0x6E, 0x73, 0x4D, 0x61, 0x74, 0x65, 0x72, 0x69, 0x61, 0x6C };
        public const ulong Version = 1;
        public uint MaterialCount;

        public void Read(Stream stream)
        {
            if (!stream.Compare(MagicNumber))
                throw new InvalidDataException();
            if (!stream.Compare(Version))
                throw new InvalidDataException();

            MaterialCount = stream.ReadUInt();
        }

        public void Write(Stream stream)
        {
            stream.Write(MagicNumber);
            stream.WriteUInt64(Version);
            stream.WriteUInt(MaterialCount);
        }
    }
}