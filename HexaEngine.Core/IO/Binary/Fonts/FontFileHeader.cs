namespace HexaEngine.Core.IO.Binary.Fonts
{
    using HexaEngine.Mathematics;
    using System.Text;

    /// <summary>
    /// Represents the header of a font file, including metadata such as font name, size, bitmap dimensions, padding, spacing, and glyph information.
    /// </summary>
    public struct FontFileHeader
    {
        /// <summary>
        /// Gets the magic number that identifies the font file.
        /// </summary>
        public static readonly byte[] MagicNumber = [0x54, 0x72, 0x61, 0x6E, 0x73, 0x46, 0x6F, 0x6E, 0x74, 0x00];

        /// <summary>
        /// Gets the current version of the font file header.
        /// </summary>
        public static readonly Version Version = 1;

        /// <summary>
        /// Gets the minimum supported version of the font file header.
        /// </summary>
        public static readonly Version MinVersion = 1;

        /// <summary>
        /// Gets or sets the endianness of the font file.
        /// </summary>
        public Endianness Endianness;

        /// <summary>
        /// Gets or sets the encoding used for strings in the font file.
        /// </summary>
        public Encoding Encoding;

        /// <summary>
        /// Gets or sets the name of the font.
        /// </summary>
        public string FontName;

        /// <summary>
        /// Gets or sets the size of the font.
        /// </summary>
        public uint FontSize;

        /// <summary>
        /// Gets or sets the width of the font bitmap.
        /// </summary>
        public uint BitmapWidth;

        /// <summary>
        /// Gets or sets the height of the font bitmap.
        /// </summary>
        public uint BitmapHeight;

        /// <summary>
        /// Gets or sets the upward padding of the font.
        /// </summary>
        public uint PaddingUp;

        /// <summary>
        /// Gets or sets the downward padding of the font.
        /// </summary>
        public uint PaddingDown;

        /// <summary>
        /// Gets or sets the left padding of the font.
        /// </summary>
        public uint PaddingLeft;

        /// <summary>
        /// Gets or sets the right padding of the font.
        /// </summary>
        public uint PaddingRight;

        /// <summary>
        /// Gets or sets the horizontal spacing between glyphs.
        /// </summary>
        public uint SpacingHorizontal;

        /// <summary>
        /// Gets or sets the vertical spacing between glyphs.
        /// </summary>
        public uint SpacingVertical;

        /// <summary>
        /// Gets or sets the line height of the font.
        /// </summary>
        public uint LineHeight;

        /// <summary>
        /// Gets or sets the number of glyphs in the font.
        /// </summary>
        public uint Glyphs;

        /// <summary>
        /// Gets or sets the number of kerning pairs in the font.
        /// </summary>
        public uint KerningPairs;

        /// <summary>
        /// Reads the font file header from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        public void Read(Stream stream)
        {
            if (!stream.Compare(MagicNumber))
            {
                throw new InvalidDataException();
            }

            Endianness = (Endianness)stream.ReadByte();
            if (!stream.CompareVersion(MinVersion, Version, Endianness, out var version))
            {
                throw new InvalidDataException($"Version mismatch, file: {version} min: {MinVersion} max: {Version}");
            }

            Encoding = Encoding.GetEncoding(stream.ReadInt32(Endianness));
            FontSize = stream.ReadUInt32(Endianness);
            BitmapWidth = stream.ReadUInt32(Endianness);
            BitmapHeight = stream.ReadUInt32(Endianness);
            PaddingUp = stream.ReadUInt32(Endianness);
            PaddingDown = stream.ReadUInt32(Endianness);
            PaddingLeft = stream.ReadUInt32(Endianness);
            PaddingRight = stream.ReadUInt32(Endianness);
            SpacingHorizontal = stream.ReadUInt32(Endianness);
            SpacingVertical = stream.ReadUInt32(Endianness);
            LineHeight = stream.ReadUInt32(Endianness);
            Glyphs = stream.ReadUInt32(Endianness);
            KerningPairs = stream.ReadUInt32(Endianness);
            FontName = stream.ReadString(Encoding, Endianness) ?? string.Empty;
        }

        /// <summary>
        /// Writes the font file header to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        public void Write(Stream stream)
        {
            stream.Write(MagicNumber);
            stream.WriteByte((byte)Endianness);
            stream.WriteUInt64(Version, Endianness);
            stream.WriteInt32(Encoding.CodePage, Endianness);
            stream.WriteUInt32(FontSize, Endianness);
            stream.WriteUInt32(BitmapWidth, Endianness);
            stream.WriteUInt32(BitmapHeight, Endianness);
            stream.WriteUInt32(PaddingUp, Endianness);
            stream.WriteUInt32(PaddingDown, Endianness);
            stream.WriteUInt32(PaddingLeft, Endianness);
            stream.WriteUInt32(PaddingRight, Endianness);
            stream.WriteUInt32(SpacingHorizontal, Endianness);
            stream.WriteUInt32(SpacingVertical, Endianness);
            stream.WriteUInt32(LineHeight, Endianness);
            stream.WriteUInt32(Glyphs, Endianness);
            stream.WriteUInt32(KerningPairs, Endianness);
            stream.WriteString(FontName, Encoding, Endianness);
        }
    }
}