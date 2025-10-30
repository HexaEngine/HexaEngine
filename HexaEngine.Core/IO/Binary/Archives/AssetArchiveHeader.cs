namespace HexaEngine.Core.IO.Binary.Archives
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Security.Cryptography;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public enum RSASignatureMode
    {
        None,
        SHA256_PKCS1v15,
    }

    public struct RSASignature
    {
        public RSASignatureMode Mode;
        public byte[] PublicKey;
        public byte[] Signature;

        public RSASignature(RSASignatureMode mode, RSA rsa, Span<byte> signature) : this()
        {
            Mode = mode;
            PublicKey = rsa.ExportRSAPublicKey();
            Signature = signature.ToArray();
        }
    }

    /// <summary>
    /// Represents the header information of an asset archive.
    /// </summary>
    public struct AssetArchiveHeader
    {
        /// <summary>
        /// Magic number identifying the asset archive format.
        /// </summary>
        public static readonly byte[] Magic = [0x54, 0x72, 0x61, 0x6e, 0x73, 0x41, 0x72, 0x63, 0x68, 0x69, 0x76, 0x65, 0x00];

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
        /// The position in the stream where the content starts.
        /// </summary>
        public long ContentStart;

        public uint Alignment;
        public uint CRC32;
        public SHA256Signature SHA256;
        public RSASignature RSASignature;

        public Encoding Encoding;

        public const uint DefaultAlignment = 256;

        public AssetArchiveHeader(Endianness endianness = Endianness.LittleEndian, Encoding? encoding = null, Compression compression = Compression.LZ4, AssetArchiveFlags flags = AssetArchiveFlags.Normal, string[]? parts = null, uint alignment = DefaultAlignment)
        {
            Endianness = endianness;
            Encoding = encoding ?? Encoding.UTF8;
            Compression = compression;
            Flags = flags;
            Parts = parts ?? [];
            Alignment = alignment;
        }

        /// <summary>
        /// Reads the asset archive header from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding.</param>
        public void Read(Stream stream, Encoding encoding)
        {
            if (!stream.Compare(Magic))
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

            ContentStart = stream.ReadInt64(Endianness);
            CRC32 = stream.ReadUInt32(Endianness);
            Span<byte> buffer = stackalloc byte[32];
            stream.ReadExactly(buffer);
            SHA256 = new(buffer, Endianness == Endianness.BigEndian);
            Encoding = encoding;
        }

        /// <summary>
        /// Writes the asset archive header to a stream.
        /// </summary>
        /// <param name="stream">The output stream.</param>
        /// <param name="encoding">The character encoding.</param>
        public readonly void Write(Stream stream, Encoding encoding)
        {
            stream.Write(Magic);
            stream.WriteByte((byte)Endianness);
            stream.WriteUInt64(Version, Endianness);
            stream.WriteInt32((int)Compression, Endianness);
            stream.WriteInt32((int)Flags, Endianness);

            stream.WriteInt32(Parts.Length, Endianness);
            for (int i = 0; i < Parts.Length; i++)
            {
                stream.WriteString(Parts[i], encoding, Endianness);
            }

            stream.WriteInt64(ContentStart, Endianness);
            stream.WriteUInt32(CRC32, Endianness);
            Span<byte> buffer = stackalloc byte[32];
            SHA256.TryWriteBytes(buffer, Endianness == Endianness.BigEndian);
            stream.Write(buffer);
        }

        /// <summary>
        /// Calculates the size of the asset archive header in bytes.
        /// </summary>
        /// <returns>The size of the header in bytes.</returns>
        public readonly int SizeOf()
        {
            var size = Magic.Length + 1 + 8 + 4 + 4 + 4 + 8 + 4 + 32;
            foreach (var part in Parts)
            {
                size += part.SizeOf(Encoding);
            }
            return size;
        }
    }
}