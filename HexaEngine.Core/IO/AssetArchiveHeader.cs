namespace HexaEngine.IO
{
    using K4os.Compression.LZ4.Streams;
    using System;
    using System.Buffers.Binary;
    using System.IO;
    using System.Text;

    public struct AssetArchiveHeader : IBinarySerializable
    {
        public static readonly byte[] MagicNumber = { 0x54, 0x72, 0x61, 0x6e, 0x73, 0x41, 0x72, 0x63, 0x68, 0x69, 0x76, 0x65, 0x00 };
        public const ulong Version = 12;

        public Compression Compression;
        public AssetArchiveHeaderEntry[] Entries;

        public void Read(Stream stream, Encoding encoding)
        {
            if (stream.Compare(MagicNumber))
                throw new InvalidDataException();
            if (stream.Compare(Version))
                throw new InvalidDataException();

            Compression = (Compression)stream.ReadInt();

            int count = stream.ReadInt();
            Entries = new AssetArchiveHeaderEntry[count];

            for (int i = 0; i < count; i++)
            {
                Entries[i].Read(stream, encoding);
            }
        }

        public int Read(ReadOnlySpan<byte> src, Encoding encoding)
        {
            if (!src[..MagicNumber.Length].SequenceEqual(MagicNumber))
                throw new InvalidDataException();
            if (!src[MagicNumber.Length..].Compare(Version))
                throw new InvalidDataException();

            int idx = MagicNumber.Length + 8;

            Compression = (Compression)BinaryPrimitives.ReadInt32LittleEndian(src[idx..]);
            idx += 4;

            int count = BinaryPrimitives.ReadInt32LittleEndian(src[idx..]);
            idx += 4;

            Entries = new AssetArchiveHeaderEntry[count];

            for (int i = 0; i < count; i++)
            {
                idx += Entries[i].Read(src[idx..], encoding);
            }

            return idx;
        }

        public void Write(Stream stream, Encoding encoding)
        {
            stream.Write(MagicNumber);
            stream.WriteUInt64(Version);
            stream.WriteInt((int)Compression);
            stream.WriteInt(Entries.Length);
            for (int i = 0; i < Entries.Length; i++)
            {
                Entries[i].Write(stream, encoding);
            }
        }

        public int Write(Span<byte> dst, Encoding encoding)
        {
            MagicNumber.CopyTo(dst);
            BinaryPrimitives.WriteUInt64LittleEndian(dst[MagicNumber.Length..], Version);
            int idx = MagicNumber.Length + 8;
            BinaryPrimitives.WriteInt32LittleEndian(dst[idx..], (int)Compression);
            idx += 4;
            BinaryPrimitives.WriteInt32LittleEndian(dst[idx..], Entries.Length);
            idx += 4;

            for (int i = 0; i < Entries.Length; i++)
            {
                idx += Entries[i].Write(dst[idx..], encoding);
            }

            return idx;
        }

        public int Size(Encoding encoding)
        {
            return MagicNumber.Length + 8 + 4 + 4 + Entries.Sum(x => x.Size(encoding));
        }
    }
}