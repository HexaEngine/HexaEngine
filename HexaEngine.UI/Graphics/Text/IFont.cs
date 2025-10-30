namespace HexaEngine.UI.Graphics.Text
{
    using System.Numerics;

    public interface IFont : IUIResource
    {
        float EmSize { get; }

        float FontSize { get; set; }

        bool Hinting { get; set; }

        float GetLineHeight(float fontSize);

        GlyphMetrics GetMetrics(uint character);

        Vector2 MeasureSize(TextRange text, float fontSize, float incrementalTabStop);

        Vector2 MeasureSize(ReadOnlySpan<char> text, float fontSize, float incrementalTabStop);

        bool GetKerning(uint left, uint right, out Vector2 kerning);

        void RenderText(UICommandList commandList, Vector2 origin, TextRange textSpan, float fontSize, Brush brush);

        void RenderText(UICommandList commandList, Vector2 origin, TextRange textSpan, float fontSize, float whitespaceScale, float incrementalTabStop, ReadingDirection readingDirection, Brush brush);

        void RenderText(UICommandList commandList, Vector2 origin, ReadOnlySpan<char> textSpan, float fontSize, Brush brush);

        void RenderText(UICommandList commandList, Vector2 origin, ReadOnlySpan<char> textSpan, float fontSize, float whitespaceScale, float incrementalTabStop, ReadingDirection readingDirection, Brush brush);
    }
}