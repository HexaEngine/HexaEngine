namespace HexaEngine.Core.IO.Assets
{
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Text;

    public struct AssetArchiveHeader
    {
        public static readonly byte[] MagicNumber = { 0x54, 0x72, 0x61, 0x6e, 0x73, 0x41, 0x72, 0x63, 0x68, 0x69, 0x76, 0x65, 0x00 };
        public const ulong Version = 12;

        public Endianness Endianness;
        public Compression Compression;
        public AssetArchiveHeaderEntry[] Entries;
        public long ContentStart;

        public void Read(Stream stream, Encoding encoding)
        {
            if (stream.Compare(MagicNumber))
                throw new InvalidDataException();
            Endianness = (Endianness)stream.ReadByte();
            if (stream.Compare(Version, Endianness))
                throw new InvalidDataException();

            Compression = (Compression)stream.ReadInt(Endianness);

            int count = stream.ReadInt(Endianness);
            Entries = new AssetArchiveHeaderEntry[count];

            for (int i = 0; i < count; i++)
            {
                Entries[i].Read(stream, encoding, Endianness);
            }
            ContentStart = stream.Position;
        }

        public void Write(Stream stream, Encoding encoding)
        {
            stream.Write(MagicNumber);
            stream.WriteUInt64(Version, Endianness);
            stream.WriteInt((int)Compression, Endianness);
            stream.WriteInt(Entries.Length, Endianness);
            for (int i = 0; i < Entries.Length; i++)
            {
                Entries[i].Write(stream, encoding, Endianness);
            }
        }

        public int Size(Encoding encoding)
        {
            return MagicNumber.Length + 1 + 8 + 4 + 4 + Entries.Sum(x => x.Size(encoding));
        }
    }
}