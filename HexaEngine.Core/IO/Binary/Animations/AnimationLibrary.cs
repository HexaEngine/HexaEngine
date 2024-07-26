namespace HexaEngine.Core.IO.Binary.Animations
{
    using HexaEngine.Core.IO;
    using Hexa.NET.Mathematics;
    using K4os.Compression.LZ4;
    using K4os.Compression.LZ4.Streams;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    /// <summary>
    /// Represents a library of animations.
    /// </summary>
    public class AnimationLibrary
    {
        /// <summary>
        /// Gets an empty animation library.
        /// </summary>
        public static AnimationLibrary Empty => new();

        private AnimationLibraryHeader header;
        private readonly List<AnimationClip> animations;

        /// <summary>
        /// Initializes an empty animation library.
        /// </summary>
        public AnimationLibrary()
        {
            header = default;
            animations = new();
        }

        /// <summary>
        /// Initializes an animation library with a collection of animations.
        /// </summary>
        /// <param name="animations">A list of animations to include in the library.</param>
        public AnimationLibrary(IList<AnimationClip> animations)
        {
            header.AnimationCount = animations.Count;
            this.animations = new(animations);
        }

        /// <summary>
        /// Gets the header information of the animation library.
        /// </summary>
        public AnimationLibraryHeader Header => header;

        /// <summary>
        /// Gets a list of animations in the library.
        /// </summary>
        public List<AnimationClip> Animations => animations;

        /// <summary>
        /// Saves the animation library to a file with the specified encoding, endianness, and compression.
        /// </summary>
        /// <param name="path">The path to the file where the library will be saved.</param>
        /// <param name="encoding">The encoding to use for writing strings.</param>
        /// <param name="endianness">The endianness of binary data.</param>
        /// <param name="compression">The compression method to use.</param>
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

        /// <summary>
        /// Loads an animation library from a file.
        /// </summary>
        /// <param name="path">The path to the file to load the library from.</param>
        /// <returns>An instance of the <see cref="AnimationLibrary"/> class loaded from the file.</returns>
        public static AnimationLibrary Load(string path)
        {
            return Load(FileSystem.OpenRead(path));
        }

        /// <summary>
        /// Loads an animation library from an external file.
        /// </summary>
        /// <param name="path">The path to the external file to load the library from.</param>
        /// <returns>An instance of the <see cref="AnimationLibrary"/> class loaded from the external file.</returns>
        public static AnimationLibrary LoadExternal(string path)
        {
            return Load(File.OpenRead(path));
        }

        /// <summary>
        /// Loads an animation library from a stream.
        /// </summary>
        /// <param name="fs">A stream containing the animation library data.</param>
        /// <returns>An instance of the <see cref="AnimationLibrary"/> class loaded from the stream.</returns>
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
                library.animations.Add(AnimationClip.Read(stream, header.Encoding, header.Endianness));
            }

            stream.Close();

            fs.Close();

            return library;
        }
    }
}