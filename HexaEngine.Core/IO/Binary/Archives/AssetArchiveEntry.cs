namespace HexaEngine.Core.IO.Binary.Archives
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Assets;
    using K4os.Compression.LZ4.Streams;
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    /// <summary>
    /// Represents an entry in the header of an asset archive, providing information about a specific asset.
    /// </summary>
    public class AssetArchiveEntry
    {
        public AssetArchive Archive { get; private set; } = null!;

        /// <summary>
        /// The path to the archive or source file containing the data.
        /// </summary>
        public string ArchivePath => Archive.Parts[PartIndex];

        /// <summary>
        /// The index of the part to which this entry belongs.
        /// </summary>
        public int PartIndex;

        /// <summary>
        /// The type of the asset.
        /// </summary>
        public AssetType Type;

        /// <summary>
        /// The GUID of the asset.
        /// </summary>
        public Guid Guid;

        /// <summary>
        /// The GUID of the asset parent.
        /// </summary>
        public Guid ParentGuid;

        /// <summary>
        /// The name to the asset.
        /// </summary>
        public string Name;

        /// <summary>
        /// The path to the asset.
        /// </summary>
        public string PathInArchive;

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

        private AssetArchiveEntry()
        {
            Name = null!;
            PathInArchive = null!;
        }

        public AssetArchiveEntry(AssetArchive archive, int partIndex, AssetType type, Guid guid, Guid parentGuid, string name, string pathInArchive, long start, long length, long actualLength)
        {
            Archive = archive;
            PartIndex = partIndex;
            Type = type;
            Guid = guid;
            ParentGuid = parentGuid;
            Name = name;
            PathInArchive = pathInArchive;
            Start = start;
            Length = length;
            ActualLength = actualLength;
        }

        public static AssetArchiveEntry ReadFrom(Stream stream, Encoding encoding, Endianness endianness, AssetArchive archive)
        {
            AssetArchiveEntry entry = new();
            entry.Read(stream, encoding, endianness);
            entry.Archive = archive;
            return entry;
        }

        /// <summary>
        /// Reads the header entry from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        public void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            PartIndex = stream.ReadInt32(endianness);
            Guid = stream.ReadGuid(endianness);
            ParentGuid = stream.ReadGuid(endianness);
            Type = (AssetType)stream.ReadUInt64(endianness);
            Start = stream.ReadInt64(endianness);
            Length = stream.ReadInt64(endianness);
            ActualLength = stream.ReadInt64(endianness);
            Name = stream.ReadString(encoding, endianness) ?? string.Empty;
            PathInArchive = stream.ReadString(encoding, endianness) ?? string.Empty;
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
            stream.WriteGuid(Guid, endianness);
            stream.WriteGuid(ParentGuid, endianness);
            stream.WriteUInt64((ulong)Type, endianness);
            stream.WriteInt64(Start, endianness);
            stream.WriteInt64(Length, endianness);
            stream.WriteInt64(ActualLength, endianness);
            stream.WriteString(Name, encoding, endianness);
            stream.WriteString(PathInArchive, encoding, endianness);
        }

        /// <summary>
        /// Calculates the size of the header entry in bytes.
        /// </summary>
        /// <param name="encoding">The character encoding.</param>
        /// <returns>The size of the header entry in bytes.</returns>
        public int SizeOf(Encoding encoding)
        {
            int size = 76;
            if (Name != null)
            {
                size += encoding.GetByteCount(Name);
            }
            if (PathInArchive != null)
            {
                size += encoding.GetByteCount(PathInArchive);
            }
            return size;
        }

        /// <summary>
        /// Gets a <see cref="VirtualStream"/> representing the asset data.
        /// </summary>
        /// <returns>A <see cref="VirtualStream"/> representing the asset data.</returns>
        public VirtualStream GetStream()
        {
            return Archive.Compression switch
            {
                Compression.None => OpenStream(),
                Compression.Deflate => DeflateDecompress(),
                Compression.LZ4 => LZ4Decompress(),
                _ => throw new NotSupportedException(),
            };
        }

        /// <summary>
        /// Gets the decompressed data of the asset.
        /// </summary>
        /// <returns>The decompressed data of the asset.</returns>
        public byte[] GetData()
        {
            var fs = GetStream();
            var data = fs.Read(ActualLength);
            fs.Close();
            return data;
        }

        /// <summary>
        /// Copies the asset data to the specified target stream.
        /// </summary>
        /// <param name="target">The target stream to copy the asset data to.</param>
        public void CopyTo(Stream target)
        {
            var fs = GetStream();
            fs.CopyTo(target);
            fs.Close();
        }

        /// <summary>
        /// Opens a <see cref="VirtualStream"/> representing the uncompressed asset data.
        /// </summary>
        /// <returns>A <see cref="VirtualStream"/> representing the uncompressed asset data.</returns>
        private VirtualStream OpenStream()
        {
            return new VirtualStream(File.Open(ArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read), Archive.BaseOffset + Start, Length);
        }

        /// <summary>
        /// Decompresses the asset data using the Deflate compression algorithm.
        /// </summary>
        /// <returns>A <see cref="VirtualStream"/> representing the decompressed asset data.</returns>
        private VirtualStream DeflateDecompress()
        {
            var baseStream = OpenStream();
            var decompressor = new DeflateStream(baseStream, CompressionMode.Decompress);
            var wrapper = new VirtualStream(decompressor, 0, ActualLength);
            return wrapper;
        }

        /// <summary>
        /// Decompresses the asset data using the LZ4 compression algorithm.
        /// </summary>
        /// <returns>A <see cref="VirtualStream"/> representing the decompressed asset data.</returns>
        private VirtualStream LZ4Decompress()
        {
            var baseStream = OpenStream();
            var decompressor = LZ4Stream.Decode(baseStream);
            var wrapper = new VirtualStream(decompressor, 0, ActualLength);
            return wrapper;
        }
    }
}