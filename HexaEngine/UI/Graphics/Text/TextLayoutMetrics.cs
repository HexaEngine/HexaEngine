namespace HexaEngine.UI.Graphics.Text
{
    using System.Numerics;

    public struct TextLayoutMetrics
    {
        public float Top;
        public float Left;
        public float Bottom;
        public float Right;

        public TextLayoutMetrics(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Bottom = bottom;
            Right = right;
        }

        public TextLayoutMetrics(Vector2 origin, Vector2 size)
        {
            Top = origin.Y;
            Left = origin.X;
            Bottom = origin.Y + size.Y;
            Right = origin.X + size.X;
        }

        public readonly Vector2 Origin => new(Left, Top);

        public readonly Vector2 Size => new Vector2(Right, Bottom) - Origin;

        public readonly float Width => Right - Left;

        public readonly float Height => Bottom - Top;

        public void Merge(TextLayoutMetrics other)
        {
            Top = Math.Min(Top, other.Top);
            Left = Math.Min(Left, other.Left);
            Bottom = Math.Max(Bottom, other.Bottom);
            Right = Math.Max(Right, other.Right);
        }
    }
}