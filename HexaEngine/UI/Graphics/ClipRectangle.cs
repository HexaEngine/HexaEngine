namespace HexaEngine.UI.Graphics
{
    using Hexa.NET.Mathematics;
    using System.Numerics;

    public struct ClipRectangle : IEquatable<ClipRectangle>
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public ClipRectangle(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public ClipRectangle(Vector2 origin, Vector2 size)
        {
            Left = (int)MathF.Floor(origin.X);
            Top = (int)MathF.Floor(origin.Y);
            Right = (int)MathF.Ceiling(origin.X + size.X);
            Bottom = (int)MathF.Ceiling(origin.Y + size.Y);
        }

        public ClipRectangle(RectangleF rectangle)
        {
            Left = (int)MathF.Floor(rectangle.Left);
            Top = (int)MathF.Floor(rectangle.Top);
            Right = (int)MathF.Ceiling(rectangle.Right);
            Bottom = (int)MathF.Ceiling(rectangle.Bottom);
        }

        public static implicit operator ClipRectangle(RectangleF rectangle)
        {
            return new(rectangle);
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is ClipRectangle rectangle && Equals(rectangle);
        }

        public readonly bool Equals(ClipRectangle other)
        {
            return Left == other.Left &&
                   Top == other.Top &&
                   Right == other.Right &&
                   Bottom == other.Bottom;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Left, Top, Right, Bottom);
        }

        public readonly Vector4 ToVec4()
        {
            return new(Left, Top, Right, Bottom);
        }

        public static bool operator ==(ClipRectangle left, ClipRectangle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ClipRectangle left, ClipRectangle right)
        {
            return !(left == right);
        }
    }
}