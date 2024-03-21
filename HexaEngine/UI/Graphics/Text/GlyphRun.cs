namespace HexaEngine.UI.Graphics.Text
{
    using System.Numerics;

    public struct GlyphRun
    {
        private readonly Vector2 origin;
        private readonly TextRange textSpan;
        private readonly IFont font;
        private readonly float fontSize;
        private readonly Brush brush;

        public GlyphRun(Vector2 origin, TextRange textSpan, IFont font, float fontSize, Brush brush)
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