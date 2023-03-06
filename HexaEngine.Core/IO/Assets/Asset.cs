namespace HexaEngine.Core.IO.Assets
{
    using HexaEngine.Core.IO;
    using K4os.Compression.LZ4.Streams;
    using System.IO;
    using System.IO.Compression;

    public readonly struct AssetDesc
    {
        public readonly string Path;
        public readonly AssetType Type;
        public readonly Stream Stream;

        public AssetDesc(string path, AssetType type, Stream stream)
        {
            Path = path;
            Type = type;
            Stream = stream;
        }

        public static AssetDesc[] CreateFromPaths(string root, string[] paths)
        {
            AssetDesc[] descs = new AssetDesc[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                var file = paths[i];
                var path = System.IO.Path.GetRelativePath(root, file);
                var type = AssetArchive.GetAssetType(path, out string _);
                var fs = File.OpenRead(file);
                descs[i] = new(path, type, fs);
            }
            return descs;
        }

        public static AssetDesc[] CreateFromDir(string root)
        {
            var dir = new DirectoryInfo(root);
            List<string> files = new();
            foreach (var file in dir.GetFiles("*.*", SearchOption.AllDirectories))
            {
                if (file.Extension == ".assets")
                    continue;
                if (file.Extension == ".dll")
                    continue;
                if (file.Extension == ".hexlvl")
                    continue;
                files.Add(file.FullName);
            }

            AssetDesc[] descs = new AssetDesc[files.Count];

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var path = System.IO.Path.GetRelativePath(root, file);
                var type = AssetArchive.GetAssetType(path, out string _);
                var fs = File.OpenRead(file);
                descs[i] = new(path, type, fs);
            }

            return descs;
        }

        public static implicit operator AssetDesc(Asset asset)
        {
            return new(asset.Path, asset.Type, asset.GetStream());
        }
    }

    public class Asset
    {
        internal Asset(string archivePath, Compression compression, AssetType type, long pointer, long length, long actualLength, string path)
        {
            ArchivePath = archivePath;
            Compression = compression;
            Type = type;
            Pointer = pointer;
            Length = length;
            ActualLength = actualLength;
            Path = path;
        }

        public readonly string ArchivePath;
        public readonly Compression Compression;
        public readonly AssetType Type;
        public readonly long Pointer;
        public readonly long Length;
        public readonly long ActualLength;
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
            var wrapper = new VirtualStream(decompressor, 0, ActualLength);
            return wrapper;
        }

        private VirtualStream LZ4Decompress()
        {
            var baseStream = OpenStream();
            var decompressor = LZ4Stream.Decode(baseStream);
            var wrapper = new VirtualStream(decompressor, 0, ActualLength);
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