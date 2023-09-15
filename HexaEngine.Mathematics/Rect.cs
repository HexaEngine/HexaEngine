namespace HexaEngine.Mathematics
{
    using System.Numerics;

    public struct Rect
    {
        public long Left;
        public long Top;
        public long Right;
        public long Bottom;

        public Rect(long left, long top, long right, long bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public Rect(Point2 pos, Point2 size)
        {
            Left = pos.X;
            Top = pos.Y;
            Right = pos.X + size.X;
            Bottom = pos.Y + size.Y;
        }

        public Rect(UPoint2 pos, UPoint2 size)
        {
            Left = pos.X;
            Top = pos.Y;
            Right = pos.X + size.X;
            Bottom = pos.Y + size.Y;
        }
    }

    public struct Rect32
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public Rect32(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public Rect32(Point2 pos, Point2 size)
        {
            Left = pos.X;
            Top = pos.Y;
            Right = pos.X + size.X;
            Bottom = pos.Y + size.Y;
        }

        public readonly Vector2 Offset => new(Left, Top);
    }
}