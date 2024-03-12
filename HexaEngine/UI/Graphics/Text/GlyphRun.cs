namespace HexaEngine.UI.Graphics.Text
{
    using System.Numerics;

    public struct TextSpan
    {
        public string Text;
        public int Start;
        public int Length;

        public TextSpan(string text, int start, int length)
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

        public static implicit operator TextSpan(string text)
        {
            return new(text, 0, text.Length);
        }

        public override readonly string ToString()
        {
            return Text.Substring(Start, Length);
        }
    }

    public struct GlyphRun
    {
        private readonly Vector2 origin;
        private readonly TextSpan textSpan;
        private readonly IFont font;
        private readonly float fontSize;
        private readonly Brush brush;

        public GlyphRun(Vector2 origin, TextSpan textSpan, IFont font, float fontSize, Brush brush)
        {
            this.origin = origin;
            this.textSpan = textSpan;
            this.font = font;
            this.fontSize = fontSize;
            this.brush = brush;
        }

        public void Draw(UICommandList commandList)
        {
            font.RenderText(commandList, origin, textSpan, fontSize, brush);
        }
    }
}