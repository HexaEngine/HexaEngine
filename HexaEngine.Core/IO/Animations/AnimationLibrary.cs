namespace HexaEngine.Core.IO.Animations
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using K4os.Compression.LZ4;
    using K4os.Compression.LZ4.Streams;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    public class AnimationLibrary
    {
        public static AnimationLibrary Empty => new();

        private AnimationLibraryHeader header;
        private readonly List<Animation> animations;

        public AnimationLibrary()
        {
            header = default;
            animations = new();
        }

        public AnimationLibrary(IList<Animation> animations)
        {
            header.AnimationCount = animations.Count;
            this.animations = new(animations);
        }

        public AnimationLibraryHeader Header => header;

        public List<Animation> Animations => animations;

        public void Save(string path, Encoding encoding, Endianness endianness = Endianness.LittleEndian, Compression compression = Compression.LZ4)
        {
            Stream fs = File.Create(path);

            header.Encoding = encoding;
            header.Endianness = endianness;
            header.Compression = compression;
            header.AnimationCount = animations.Count;
            header.Write(fs);

            var stream = fs;
            if (compression == Compression.Deflate)
            {
                stream = new DeflateStream(fs, CompressionLevel.SmallestSize, true);
            }

            if (compression == Compression.LZ4)
            {
                stream = LZ4Stream.Encode(fs, LZ4Level.L12_MAX, 0, true);
            }

            for (int i = 0; i < animations.Count; i++)
            {
                animations[i].Write(stream, encoding, endianness);
            }

            stream.Close();
            fs.Close();
        }

        public static AnimationLibrary Load(string path)
        {
            return Load(FileSystem.OpenRead(path));
        }

        public static AnimationLibrary LoadExternal(string path)
        {
            return Load(File.OpenRead(path));
        }

        public static AnimationLibrary Load(Stream fs)
        {
            AnimationLibrary library = new();

            library.header.Read(fs);
            var header = library.header;

            var stream = fs;
            if (header.Compression == Compression.Deflate)
            {
                stream = new DeflateStream(fs, CompressionMode.Decompress, true);
            }

            if (header.Compression == Compression.LZ4)
            {
                stream = LZ4Stream.Decode(fs, 0, true);
            }

            library.animations.Capacity = header.AnimationCount;
            for (int i = 0; i < header.AnimationCount; i++)
            {
                library.animations.Add(Animation.ReadFrom(stream, header.Encoding, header.Endianness));
            }

            stream.Close();

            fs.Close();

            return library;
        }
    }
}