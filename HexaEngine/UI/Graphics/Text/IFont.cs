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

        Vector2 MeasureSize(TextSpan text, float fontSize, float incrementalTabStop);

        bool GetKerning(uint left, uint right, out Vector2 kerning);

        void RenderText(UICommandList commandList, Vector2 origin, TextSpan textSpan, float fontSize, Brush brush);

        void RenderText(UICommandList commandList, Vector2 origin, TextSpan textSpan, float fontSize, float whitespaceScale, float incrementalTabStop, ReadingDirection readingDirection, Brush brush);
    }
}