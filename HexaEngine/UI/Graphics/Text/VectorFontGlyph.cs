namespace HexaEngine.UI.Graphics.Text
{
    public struct VectorFontGlyph
    {
        public uint Index;
        public uint BufferIndex;
        public int CurveCount;
        public int Width, Height;
        public int BearingX, BearingY;
        public int Advance;

        public VectorFontGlyph(uint index, uint bufferIndex, int curveCount, int width, int height, int bearingX, int bearingY, int advance)
        {
            Index = index;
            BufferIndex = bufferIndex;
            CurveCount = curveCount;
            Width = width;
            Height = height;
            BearingX = bearingX;
            BearingY = bearingY;
            Advance = advance;
        }
    }
}