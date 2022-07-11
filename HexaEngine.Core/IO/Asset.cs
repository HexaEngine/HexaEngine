namespace HexaEngine.Core.IO
{
    using System.IO;

    public class Asset
    {
        internal Asset(string path, long pointer, long length, Stream stream)
        {
            Path = path;
            Pointer = pointer;
            Length = length;
            Stream = stream;
        }

        public readonly string Path;
        public readonly long Pointer;
        public readonly long Length;
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
    }
}