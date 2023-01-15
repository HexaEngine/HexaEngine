namespace HexaEngine.IO
{
    using K4os.Compression.LZ4.Streams;
    using System.IO;
    using System.IO.Compression;

    public class Asset
    {
        internal Asset(string archivePath, Compression compression, AssetType type, long pointer, long length, string path)
        {
            ArchivePath = archivePath;
            Compression = compression;
            Type = type;
            Pointer = pointer;
            Length = length;
            Path = path;
        }

        public readonly string ArchivePath;
        public readonly Compression Compression;
        public readonly AssetType Type;
        public readonly long Pointer;
        public readonly long Length;
        public readonly string Path;

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

        public byte[] GetData()
        {
            var fs = GetStream();
            fs.Position = Pointer;
            var data = fs.Read(Length);
            fs.Close();
            return data;
        }

        public void CopyTo(Stream target)
        {
            var fs = GetStream();
            fs.Position = Pointer;
            fs.CopyTo(target);
            fs.Close();
        }

        private VirtualStream OpenStream()
        {
            return new VirtualStream(File.Open(ArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read), Pointer, Length);
        }

        private VirtualStream DeflateDecompress()
        {
            var baseStream = OpenStream();
            var decompressor = new DeflateStream(baseStream, CompressionMode.Decompress);
            var wrapper = new VirtualStream(decompressor, 0, Length);
            return wrapper;
        }

        private VirtualStream LZ4Decompress()
        {
            var baseStream = OpenStream();
            var decompressor = LZ4Stream.Decode(baseStream);
            var wrapper = new VirtualStream(decompressor, 0, Length);
            return wrapper;
        }
    }

    public class BinaryAsset
    {
        public BinaryAsset(AssetType type, long pointer, long length, string path, byte[] data)
        {
            Type = type;
            Pointer = pointer;
            Length = length;
            Path = path;
            this.data = data;
        }

        public readonly AssetType Type;
        public readonly long Pointer;
        public readonly long Length;
        public readonly string Path;
        public readonly byte[] data;
    }
}