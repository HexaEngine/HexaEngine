namespace HexaEngine.Core.IO.Binary.Fonts
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;

    /// <summary>
    /// Represents a glyph in a font, including its identifier, position, size, offset, and advance value.
    /// </summary>
    public struct FontGlyph
    {
        /// <summary>
        /// Gets or sets the identifier (character) of the glyph.
        /// </summary>
        public char Id;

        /// <summary>
        /// Gets or sets the X-coordinate of the glyph in the font texture.
        /// </summary>
        public uint X;

        /// <summary>
        /// Gets or sets the Y-coordinate of the glyph in the font texture.
        /// </summary>
        public uint Y;

        /// <summary>
        /// Gets or sets the width of the glyph.
        /// </summary>
        public uint Width;

        /// <summary>
        /// Gets or sets the height of the glyph.
        /// </summary>
        public uint Height;

        /// <summary>
        /// Gets or sets the X offset of the glyph when rendering.
        /// </summary>
        public uint XOffset;

        /// <summary>
        /// Gets or sets the Y offset of the glyph when rendering.
        /// </summary>
        public uint YOffset;

        /// <summary>
        /// Gets or sets the X advance value of the glyph.
        /// </summary>
        public uint XAdvance;

        /// <summary>
        /// Reads a <see cref="FontGlyph"/> from the specified stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        /// <returns>The read <see cref="FontGlyph"/>.</returns>
        public static FontGlyph Read(Stream stream, Endianness endianness)
        {
            FontGlyph fontGlyph;
            fontGlyph.Id = (char)stream.ReadUInt16(endianness);
            fontGlyph.X = stream.ReadUInt32(endianness);
            fontGlyph.Y = stream.ReadUInt32(endianness);
            fontGlyph.Width = stream.ReadUInt32(endianness);
            fontGlyph.Height = stream.ReadUInt32(endianness);
            fontGlyph.XOffset = stream.ReadUInt32(endianness);
            fontGlyph.YOffset = stream.ReadUInt32(endianness);
            fontGlyph.XAdvance = stream.ReadUInt32(endianness);
            return fontGlyph;
        }

        /// <summary>
        /// Writes the <see cref="FontGlyph"/> to the specified stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="endianness">The endianness to use for writing data to the stream.</param>
        public void Write(Stream stream, Endianness endianness)
        {
            stream.WriteUInt16(Id, endianness);
            stream.WriteUInt32(X, endianness);
            stream.WriteUInt32(Y, endianness);
            stream.WriteUInt32(Width, endianness);
            stream.WriteUInt32(Height, endianness);
            stream.WriteUInt32(XOffset, endianness);
            stream.WriteUInt32(YOffset, endianness);
            stream.WriteUInt32(XAdvance, endianness);
        }
    }
}