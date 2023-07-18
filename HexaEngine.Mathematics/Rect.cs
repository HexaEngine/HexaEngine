namespace HexaEngine.Mathematics
{
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
}