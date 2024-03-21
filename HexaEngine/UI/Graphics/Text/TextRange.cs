namespace HexaEngine.UI.Graphics.Text
{
    public struct TextRange
    {
        public string Text;
        public int Start;
        public int Length;

        public TextRange(string text, int start, int length)
        {
            Text = text;
            Start = start;
            Length = length;
        }

        public readonly char this[int index]
        {
            get
            {
                return Text[Start + index];
            }
        }

        public readonly int End => Start + Length;

        public static implicit operator TextRange(string text)
        {
            return new(text, 0, text.Length);
        }

        public override readonly string ToString()
        {
            return Text.Substring(Start, Length);
        }
    }
}