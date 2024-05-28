namespace HexaEngine.Core.IO.Binary.Archives
{
    using HexaEngine.Core.Security.Cryptography;
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents the header information of an asset archive.
    /// </summary>
    public struct AssetArchiveHeader
    {
        /// <summary>
        /// Magic number identifying the asset archive format.
        /// </summary>
        public static readonly byte[] MagicNumber = [0x54, 0x72, 0x61, 0x6e, 0x73, 0x41, 0x72, 0x63, 0x68, 0x69, 0x76, 0x65, 0x00];

        /// <summary>
        /// Current version of the asset archive format.
        /// </summary>
        public static readonly Version Version = 14;

        /// <summary>
        /// Minimum supported version of the asset archive format.
        /// </summary>
        public static readonly Version MinVersion = 14;

        /// <summary>
        /// The endianness of the asset archive.
        /// </summary>
        public Endianness Endianness;

        /// <summary>
        /// The compression used for the asset archive.
        /// </summary>
        public Compression Compression;

        /// <summary>
        /// Flags indicating the type of asset archive.
        /// </summary>
        public AssetArchiveFlags Flags;

        /// <summary>
        /// Array of parts in the asset archive.
        /// </summary>
        public string[] Parts;

        /// <summary>
        /// Entry count of the archive.
        /// </summary>
        public int EntryCount;

        /// <summary>
        /// The position in the stream where the content starts.
        /// </summary>
        public long ContentStart;

        public uint CRC32;

        public SHA256Signature SHA256;

        /// <summary>
        /// Reads the asset archive header from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding.</param>
        public void Read(Stream stream, Encoding encoding)
        {
            if (!stream.Compare(MagicNumber))
            {
                throw new InvalidDataException();
            }

            Endianness = (Endianness)stream.ReadByte();
            if (!stream.CompareVersion(MinVersion, Version, Endianness, out var version))
            {
                throw new InvalidDataException($"Version mismatch, file: {version} min: {MinVersion} max: {Version}");
            }

            Compression = (Compression)stream.ReadInt32(Endianness);

            Flags = (AssetArchiveFlags)stream.ReadInt32(Endianness);

            int partCount = stream.ReadInt32(Endianness);
            Parts = new string[partCount];

            for (int i = 0; i < partCount; i++)
            {
                Parts[i] = stream.ReadString(encoding, Endianness) ?? string.Empty;
            }

            EntryCount = stream.ReadInt32(Endianness);
            ContentStart = stream.ReadInt64(Endianness);
            CRC32 = stream.ReadUInt32(Endianness);
            Span<byte> buffer = stackalloc byte[32];
            stream.Read(buffer);
            SHA256 = new(buffer, Endianness == Endianness.BigEndian);
        }

        /// <summary>
        /// Writes the asset archive header to a stream.
        /// </summary>
        /// <param name="stream">The output stream.</param>
        /// <param name="encoding">The character encoding.</param>
        public readonly void Write(Stream stream, Encoding encoding)
        {
            stream.Write(MagicNumber);
            stream.WriteByte((byte)Endianness);
            stream.WriteUInt64(Version, Endianness);
            stream.WriteInt32((int)Compression, Endianness);
            stream.WriteInt32((int)Flags, Endianness);

            stream.WriteInt32(Parts.Length, Endianness);
            for (int i = 0; i < Parts.Length; i++)
            {
                stream.WriteString(Parts[i], encoding, Endianness);
            }

            stream.WriteInt32(EntryCount, Endianness);
            stream.WriteInt64(ContentStart, Endianness);
            stream.WriteUInt32(CRC32, Endianness);
            Span<byte> buffer = stackalloc byte[32];
            SHA256.TryWriteBytes(buffer, Endianness == Endianness.BigEndian);
            stream.Write(buffer);
        }

        /// <summary>
        /// Calculates the size of the asset archive header in bytes.
        /// </summary>
        /// <param name="encoding">The character encoding.</param>
        /// <returns>The size of the header in bytes.</returns>
        public readonly int Size(Encoding encoding)
        {
            return MagicNumber.Length + 1 + 8 + 4 + 4 + 4 + Parts.Sum(x => encoding.GetByteCount(x) + 4) + 4 + 8 + 4;
        }
    }
}