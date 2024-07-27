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
    public struct AssetArchiveEntry : IEquatable<AssetArchiveEntry>
    {
        /// <summary>
        /// The compression method used for the asset.
        /// </summary>
        public Compression Compression;

        /// <summary>
        /// The path to the archive or source file containing the data.
        /// </summary>
        public string ArchivePath;

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
        /// The content offset.
        /// </summary>
        public long BaseOffset;

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

        public AssetArchiveEntry(Compression compression, string archivePath, int partIndex, AssetType type, Guid guid, Guid parentGuid, string name, string pathInArchive, long start, long length, long actualLength)
        {
            Compression = compression;
            ArchivePath = archivePath;
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
        public readonly void Write(Stream stream, Encoding encoding, Endianness endianness)
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
        public readonly int Size(Encoding encoding)
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
            return Compression switch
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
            return new VirtualStream(File.Open(ArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read), Start + BaseOffset, Length);
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

        public override readonly bool Equals(object? obj)
        {
            return obj is AssetArchiveEntry entry && Equals(entry);
        }

        public readonly bool Equals(AssetArchiveEntry other)
        {
            return Compression == other.Compression &&
                   ArchivePath == other.ArchivePath &&
                   PartIndex == other.PartIndex &&
                   Type == other.Type &&
                   Guid.Equals(other.Guid) &&
                   ParentGuid.Equals(other.ParentGuid) &&
                   Name == other.Name &&
                   PathInArchive == other.PathInArchive &&
                   Start == other.Start &&
                   Length == other.Length &&
                   ActualLength == other.ActualLength;
        }

        public override readonly int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(Compression);
            hash.Add(ArchivePath);
            hash.Add(PartIndex);
            hash.Add(Type);
            hash.Add(Guid);
            hash.Add(ParentGuid);
            hash.Add(Name);
            hash.Add(PathInArchive);
            hash.Add(Start);
            hash.Add(Length);
            hash.Add(ActualLength);
            return hash.ToHashCode();
        }

        public static bool operator ==(AssetArchiveEntry left, AssetArchiveEntry right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AssetArchiveEntry left, AssetArchiveEntry right)
        {
            return !(left == right);
        }
    }
}