namespace HexaEngine.UI.Graphics.Text
{
    public struct CharacterMetrics
    {
        public string Text;
        public int Index;
        public float Width;

        public CharacterMetrics(string text, int index, float width)
        {
            Text = text;
            Index = index;
            Width = width;
        }

        public readonly char Char => Text[Index];
    }
}