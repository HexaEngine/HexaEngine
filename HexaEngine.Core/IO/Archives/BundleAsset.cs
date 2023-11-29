namespace HexaEngine.Core.IO.Archives
{
    using HexaEngine.Core.IO;
    using K4os.Compression.LZ4.Streams;
    using System.IO;
    using System.IO.Compression;

    /// <summary>
    /// Represents an asset within a bundle archive, providing methods to access and decompress its data.
    /// </summary>
    public class BundleAsset
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BundleAsset"/> class.
        /// </summary>
        /// <param name="archivePath">The path to the archive containing the asset.</param>
        /// <param name="compression">The compression method used for the asset.</param>
        /// <param name="partIndex">The index of the part within the archive.</param>
        /// <param name="type">The type of the asset.</param>
        /// <param name="pointer">The position of the asset within the archive.</param>
        /// <param name="length">The compressed length of the asset data.</param>
        /// <param name="actualLength">The actual length of the decompressed asset data.</param>
        /// <param name="path">The path of the asset within the archive.</param>
        internal BundleAsset(string archivePath, Compression compression, int partIndex, AssetType type, long pointer, long length, long actualLength, string path)
        {
            PartIndex = partIndex;
            ArchivePath = archivePath;
            Compression = compression;
            Type = type;
            Pointer = pointer;
            Length = length;
            ActualLength = actualLength;
            Path = path;
        }

        /// <summary>
        /// Gets the index of the part within the archive.
        /// </summary>
        public readonly int PartIndex;

        /// <summary>
        /// Gets the path to the archive containing the asset.
        /// </summary>
        public readonly string ArchivePath;

        /// <summary>
        /// Gets the compression method used for the asset.
        /// </summary>
        public readonly Compression Compression;

        /// <summary>
        /// Gets the type of the asset.
        /// </summary>
        public readonly AssetType Type;

        /// <summary>
        /// Gets the position of the asset within the archive.
        /// </summary>
        public readonly long Pointer;

        /// <summary>
        /// Gets the compressed length of the asset data.
        /// </summary>
        public readonly long Length;

        /// <summary>
        /// Gets the actual length of the decompressed asset data.
        /// </summary>
        public readonly long ActualLength;

        /// <summary>
        /// Gets the path of the asset within the archive.
        /// </summary>
        public readonly string Path;

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
            fs.Position = Pointer;
            var data = fs.Read(Length);
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
            fs.Position = Pointer;
            fs.CopyTo(target);
            fs.Close();
        }

        /// <summary>
        /// Opens a <see cref="VirtualStream"/> representing the uncompressed asset data.
        /// </summary>
        /// <returns>A <see cref="VirtualStream"/> representing the uncompressed asset data.</returns>
        private VirtualStream OpenStream()
        {
            return new VirtualStream(File.Open(ArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read), Pointer, Length);
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