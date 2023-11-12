namespace HexaEngine.Core.IO.Assets
{
    using HexaEngine.Mathematics;
    using System;
    using System.Buffers.Binary;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents an entry in the header of an asset archive, providing information about a specific asset.
    /// </summary>
    public struct AssetArchiveHeaderEntry
    {
        /// <summary>
        /// The index of the part to which this entry belongs.
        /// </summary>
        public int PartIndex;

        /// <summary>
        /// The type of the asset.
        /// </summary>
        public AssetType Type;

        /// <summary>
        /// The path to the asset.
        /// </summary>
        public string Path;

        /// <summary>
        /// The starting position of the asset's data in the archive.
        /// </summary>
        public long Start;

        /// <summary>
        /// The total length of the asset's data in the archive.
        /// </summary>
        public long Length;

        /// <summary>
        /// The actual length of the asset's data (uncompressed or without padding) in the archive.
        /// </summary>
        public long ActualLength;

        /// <summary>
        /// Reads the header entry from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        public void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            PartIndex = stream.ReadInt32(endianness);
            Type = (AssetType)stream.ReadUInt64(endianness);
            Start = stream.ReadInt64(endianness);
            Length = stream.ReadInt64(endianness);
            ActualLength = stream.ReadInt64(endianness);
            Path = stream.ReadString(encoding, endianness) ?? string.Empty;
        }

        /// <summary>
        /// Writes the header entry to a stream.
        /// </summary>
        /// <param name="stream">The output stream.</param>
        /// <param name="encoding">The character encoding.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        public void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteInt32(PartIndex, endianness);
            stream.WriteUInt64((ulong)Type, endianness);
            stream.WriteInt64(Start, endianness);
            stream.WriteInt64(Length, endianness);
            stream.WriteInt64(ActualLength, endianness);
            stream.WriteString(Path, encoding, endianness);
        }

        /// <summary>
        /// Reads the header entry from a read-only span of bytes.
        /// </summary>
        /// <param name="src">The source span of bytes.</param>
        /// <param name="encoding">The character encoding.</param>
        /// <returns>The number of bytes read.</returns>
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

        /// <summary>
        /// Writes the header entry to a span of bytes.
        /// </summary>
        /// <param name="dst">The destination span of bytes.</param>
        /// <param name="encoding">The character encoding.</param>
        /// <returns>The number of bytes written.</returns>
        public int Write(Span<byte> dst, Encoding encoding)
        {
            BinaryPrimitives.WriteInt32LittleEndian(dst, PartIndex);
            BinaryPrimitives.WriteUInt64LittleEndian(dst[4..], (ulong)Type);
            BinaryPrimitives.WriteInt64LittleEndian(dst[12..], Start);
            BinaryPrimitives.WriteInt64LittleEndian(dst[20..], Length);
            BinaryPrimitives.WriteInt64LittleEndian(dst[28..], ActualLength);
            return 32 + dst[32..].WriteString(Path, encoding);
        }

        /// <summary>
        /// Calculates the size of the header entry in bytes.
        /// </summary>
        /// <param name="encoding">The character encoding.</param>
        /// <returns>The size of the header entry in bytes.</returns>
        public int Size(Encoding encoding)
        {
            return 8 + 8 + 8 + 8 + encoding.GetByteCount(Path) + 4 + 4;
        }
    }
}