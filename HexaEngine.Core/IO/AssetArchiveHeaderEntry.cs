namespace HexaEngine.Core.IO
{
    using System;
    using System.Buffers.Binary;
    using System.IO;
    using System.Text;

    public struct AssetArchiveHeaderEntry
    {
        /// <summary>
        /// The type of the asset.
        /// </summary>
        public AssetType Type;

        /// <summary>
        /// The path.
        /// </summary>
        public string Path;

        /// <summary>
        /// Position in file.
        /// </summary>
        public long Start;

        /// <summary>
        /// Length in file.
        /// </summary>
        public long Length;

        /// <summary>
        /// Decompressed size, is the same as lenght if the Compression is set to none.
        /// </summary>
        public long ActualLength;

        public void Read(Stream stream, Encoding encoding)
        {
            Type = (AssetType)stream.ReadUInt64();
            Start = stream.ReadInt64();
            Length = stream.ReadInt64();
            ActualLength = stream.ReadInt64();
            Path = stream.ReadString(encoding);
        }

        public void Write(Stream stream, Encoding encoding)
        {
            stream.WriteUInt64((ulong)Type);
            stream.WriteInt64(Start);
            stream.WriteInt64(Length);
            stream.WriteInt64(ActualLength);
            stream.WriteString(Path, encoding);
        }

        public int Read(ReadOnlySpan<byte> src, Encoding encoding)
        {
            Type = (AssetType)BinaryPrimitives.ReadUInt64LittleEndian(src);
            Start = BinaryPrimitives.ReadInt64LittleEndian(src[8..]);
            Length = BinaryPrimitives.ReadInt64LittleEndian(src[16..]);
            ActualLength = BinaryPrimitives.ReadInt64LittleEndian(src[24..]);
            Path = src[32..].ReadString(encoding, out int read);
            return 32 + read;
        }

        public int Write(Span<byte> dst, Encoding encoding)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(dst, (ulong)Type);
            BinaryPrimitives.WriteInt64LittleEndian(dst[8..], Start);
            BinaryPrimitives.WriteInt64LittleEndian(dst[16..], Length);
            BinaryPrimitives.WriteInt64LittleEndian(dst[24..], ActualLength);
            return 32 + dst[32..].WriteString(Path, encoding);
        }

        public int Size(Encoding encoding)
        {
            return 8 + 8 + 8 + 8 + encoding.GetByteCount(Path) + 4;
        }
    }
}