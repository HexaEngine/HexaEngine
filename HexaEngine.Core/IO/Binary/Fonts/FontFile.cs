namespace HexaEngine.Core.IO.Binary.Fonts
{
    using HexaEngine.Core.IO;
    using Hexa.NET.Mathematics;
    using System.Numerics;
    using System.Text;

    /// <summary>
    /// Represents a font file, including header, glyphs, kerning pairs, and optional pixel data.
    /// </summary>
    public class FontFile
    {
        private FontFileHeader header;
        private readonly List<FontGlyph> glyphs;
        private readonly List<KerningPair> kerningPairs;
        private Vector4[]? pixelData;

        /// <summary>
        /// Initializes a new instance of the <see cref="FontFile"/> class with default values.
        /// </summary>
        public FontFile()
        {
            header = default;
            glyphs = new();
            kerningPairs = new();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FontFile"/> class with specified glyphs and kerning pairs.
        /// </summary>
        /// <param name="glyphs">The list of font glyphs.</param>
        /// <param name="kerningPairs">The list of kerning pairs.</param>
        public FontFile(IList<FontGlyph> glyphs, IList<KerningPair> kerningPairs)
        {
            header.Glyphs = (uint)glyphs.Count;
            header.KerningPairs = (uint)kerningPairs.Count;
            this.glyphs = new(glyphs);
            this.kerningPairs = new(kerningPairs);
        }

        /// <summary>
        /// Gets the header of the font file.
        /// </summary>
        public FontFileHeader Header => header;

        /// <summary>
        /// Gets the list of font glyphs.
        /// </summary>
        public List<FontGlyph> Glyphs => glyphs;

        /// <summary>
        /// Gets the list of kerning pairs.
        /// </summary>
        public List<KerningPair> KerningPairs => kerningPairs;

        /// <summary>
        /// Gets or sets the pixel data associated with the font.
        /// </summary>
        public Vector4[]? PixelData { get => pixelData; set => pixelData = value; }

        /// <summary>
        /// Saves the font file to the specified path.
        /// </summary>
        /// <param name="path">The path where the font file will be saved.</param>
        /// <param name="encoding">The encoding used for strings in the font file.</param>
        /// <param name="endianness">The endianness of the font file.</param>
        public void Save(string path, Encoding encoding, Endianness endianness = Endianness.LittleEndian)
        {
            Stream fs = File.Create(path);

            var stream = fs;

            header.Encoding = encoding;
            header.Endianness = endianness;
            header.Glyphs = (uint)glyphs.Count;
            header.KerningPairs = (uint)kerningPairs.Count;

            header.Write(fs);
            for (int i = 0; i < glyphs.Count; i++)
            {
                glyphs[i].Write(stream, endianness);
            }

            for (int i = 0; i < kerningPairs.Count; i++)
            {
                kerningPairs[i].Write(stream, endianness);
            }

            if ((pixelData?.Length ?? 0) != header.BitmapWidth * header.BitmapHeight)
            {
                throw new InvalidOperationException("PixelData was null or not equally long header.BitmapWidth * header.BitmapHeight");
            }

            if (pixelData != null)
            {
                for (int i = 0; i < pixelData.Length; i++)
                {
                    stream.WriteVector4(pixelData[i], endianness);
                }
            }

            fs.Close();
        }

        /// <summary>
        /// Loads a font file from the specified path.
        /// </summary>
        /// <param name="path">The path of the font file to load.</param>
        /// <returns>The loaded <see cref="FontFile"/>.</returns>
        public static FontFile Load(string path)
        {
            return Load(FileSystem.OpenRead(path));
        }

        /// <summary>
        /// Loads a font file from an external file specified by the given path.
        /// </summary>
        /// <param name="path">The path to the external font file.</param>
        /// <returns>The loaded <see cref="FontFile"/>.</returns>
        public static FontFile LoadExternal(string path)
        {
            return Load(File.OpenRead(path));
        }

        /// <summary>
        /// Loads a font file from the specified stream.
        /// </summary>
        /// <param name="stream">The stream containing the font file data.</param>
        /// <returns>The loaded <see cref="FontFile"/>.</returns>
        public static FontFile Load(Stream stream)
        {
            FontFile fontFile = new();

            fontFile.header.Read(stream);
            var header = fontFile.header;

            fontFile.glyphs.Capacity = (int)header.Glyphs;
            fontFile.kerningPairs.Capacity = (int)header.KerningPairs;

            for (int i = 0; i < header.Glyphs; i++)
            {
                fontFile.glyphs.Add(FontGlyph.Read(stream, header.Endianness));
            }

            for (int i = 0; i < header.KerningPairs; i++)
            {
                fontFile.kerningPairs.Add(KerningPair.Read(stream, header.Endianness));
            }

            var pixelCount = header.BitmapWidth * header.BitmapHeight;
            if (pixelCount > 0)
            {
                fontFile.pixelData = new Vector4[pixelCount];
                for (int i = 0; i < pixelCount; i++)
                {
                    fontFile.pixelData[i] = stream.ReadVector4(header.Endianness);
                }
            }

            return fontFile;
        }
    }
}