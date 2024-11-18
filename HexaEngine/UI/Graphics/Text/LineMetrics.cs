namespace HexaEngine.UI.Graphics.Text
{
    using System.Numerics;

    public struct LineMetrics
    {
        public TextRange Text;
        public Vector2 Position;
        public Vector2 Size;

        public readonly int Length => Text.Length;

        public readonly Vector2 Min => Position;

        public readonly Vector2 Max => Position + Size;

        public char this[int index]
        {
            get => Text[index];
        }

        public LineMetrics(TextRange text, Vector2 position, Vector2 size)
        {
            Text = text;
            Position = position;
            Size = size;
        }
    }
}