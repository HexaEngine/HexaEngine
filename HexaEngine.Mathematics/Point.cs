namespace HexaEngine.Mathematics
{
    using System.Numerics;

    public struct Point
    {
        public int X;
        public int Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator Point(Vector2 vector) => new() { X = (int)vector.X, Y = (int)vector.Y };

        public static implicit operator Vector2(Point point) => new() { X = (int)point.X, Y = (int)point.Y };
    }
}