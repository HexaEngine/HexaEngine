namespace HexaEngine.UI.Graphics.Text
{
    using System.Numerics;

    public struct SpriteFontGlyph
    {
        public uint Char;
        public uint Index;
        public int Width, Height;
        public int BearingX, BearingY;
        public int Advance;
        public Vector2 UVStart;
        public Vector2 UVEnd;
    };
}