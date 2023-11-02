namespace HexaEngine.Core.IO.Assets
{
    using HexaEngine.Mathematics;
    using System;
    using System.Buffers.Binary;
    using System.IO;
    using System.Text;

    public struct AssetArchiveHeaderEntry
    {
        /// <summary>
        /// The part index
        /// </summary>
        public int PartIndex;

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
        /// Decompressed size, is the same as length if the Compression is set to none.
        /// </summary>
        public long ActualLength;

        public void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            PartIndex = stream.ReadInt32(endianness);
            Type = (AssetType)stream.ReadUInt64(endianness);
            Start = stream.ReadInt64(endianness);
            Length = stream.ReadInt64(endianness);
            ActualLength = stream.ReadInt64(endianness);
            Path = stream.ReadString(encoding, endianness);
        }

        public void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteInt32(PartIndex, endianness);
            stream.WriteUInt64((ulong)Type, endianness);
            stream.WriteInt64(Start, endianness);
            stream.WriteInt64(Length, endianness);
            stream.WriteInt64(ActualLength, endianness);
            stream.WriteString(Path, encoding, endianness);
        }

        public int Read(ReadOnlySpan<byte> src, Encoding encoding)
        {
            PartIndex = BinaryPrimitives.ReadInt32LittleEndian(src);
            Type = (AssetType)BinaryPrimitives.ReadUInt64LittleEndian(src);
            Start = BinaryPrimitives.ReadInt64LittleEndian(src[8..]);
            Length = BinaryPrimitives.ReadInt64LittleEndian(src[16..]);
            ActualLength = BinaryPrimitives.ReadInt64LittleEndian(src[24..]);
            int read = src[32..].ReadString(encoding, out Path);
            return 32 + read;
        }

        public int Write(Span<byte> dst, Encoding encoding)
        {
            BinaryPrimitives.WriteInt32LittleEndian(dst, PartIndex);
            BinaryPrimitives.WriteUInt64LittleEndian(dst[4..], (ulong)Type);
            BinaryPrimitives.WriteInt64LittleEndian(dst[12..], Start);
            BinaryPrimitives.WriteInt64LittleEndian(dst[20..], Length);
            BinaryPrimitives.WriteInt64LittleEndian(dst[28..], ActualLength);
            return 32 + dst[32..].WriteString(Path, encoding);
        }

        public int Size(Encoding encoding)
        {
            return 8 + 8 + 8 + 8 + encoding.GetByteCount(Path) + 4 + 4;
        }
    }
}