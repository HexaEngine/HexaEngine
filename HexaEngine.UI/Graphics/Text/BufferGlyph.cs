namespace HexaEngine.UI.Graphics.Text
{
    public struct BufferGlyph
    {
        public int Index;
        public int Count;

        public BufferGlyph(int index, int count)
        {
            Index = index;
            Count = count;
        }
    }
}