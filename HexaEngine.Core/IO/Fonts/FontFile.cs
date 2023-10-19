namespace HexaEngine.Core.IO.Fonts
{
    using HexaEngine.Mathematics;
    using System.Numerics;
    using System.Text;

    public class FontFile
    {
        private FontFileHeader header;
        private readonly List<FontGlyph> glyphs;
        private readonly List<KerningPair> kerningPairs;
        private Vector4[]? pixelData;

        public FontFile()
        {
            header = default;
            glyphs = new();
            kerningPairs = new();
        }

        public FontFile(IList<FontGlyph> glyphs, IList<KerningPair> kerningPairs)
        {
            header.Glyphs = (uint)glyphs.Count;
            header.KerningPairs = (uint)kerningPairs.Count;
            this.glyphs = new(glyphs);
            this.kerningPairs = new(kerningPairs);
        }

        public FontFileHeader Header => header;

        public List<FontGlyph> Glyphs => glyphs;

        public List<KerningPair> KerningPairs => kerningPairs;

        public Vector4[]? PixelData { get => pixelData; set => pixelData = value; }

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

        public static FontFile Load(string path)
        {
            return Load(FileSystem.Open(path));
        }

        public static FontFile LoadExternal(string path)
        {
            return Load(File.OpenRead(path));
        }

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