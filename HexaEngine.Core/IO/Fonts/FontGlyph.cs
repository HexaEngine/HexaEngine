namespace HexaEngine.Core.IO.Fonts
{
    using HexaEngine.Mathematics;

    public struct FontGlyph
    {
        public char Id;
        public uint X;
        public uint Y;
        public uint Width;
        public uint Height;
        public uint XOffset;
        public uint YOffset;
        public uint XAdvance;

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