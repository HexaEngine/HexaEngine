namespace HexaEngine.IO
{
    using System.IO;

    public class Asset
    {
        internal Asset(AssetType type, long pointer, long length, string path, Stream stream)
        {
            Type = type;
            Pointer = pointer;
            Length = length;
            Path = path;
            Stream = stream;
        }

        public readonly AssetType Type;
        public readonly long Pointer;
        public readonly long Length;
        public readonly string Path;
        public readonly Stream Stream;

        public VirtualStream GetStream()
        {
            return new(Stream, Pointer, Length, false);
        }

        public byte[] GetData()
        {
            var fs = Stream;
            fs.Position = Pointer;
            return fs.Read(Length);
        }

        public void CopyTo(Stream target)
        {
            var fs = Stream;
            fs.Position = Pointer;
            fs.CopyTo(target);
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