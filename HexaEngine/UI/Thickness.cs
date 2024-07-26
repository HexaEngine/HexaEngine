namespace HexaEngine.UI
{
    using Hexa.NET.Mathematics;
    using System;
    using System.Numerics;

    public struct Thickness : IEquatable<Thickness>
    {
        public float Top;
        public float Right;
        public float Left;
        public float Bottom;

        public Thickness(float size) : this(size, size)
        {
        }

        public Thickness(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public Thickness(float width, float height)
        {
            Top = height / 2;
            Right = width / 2;
            Left = width / 2;
            Bottom = height / 2;
        }

        public static readonly Thickness Zero = default;

        public static readonly Thickness NaN = new(float.NaN, float.NaN, float.NaN, float.NaN);

        public readonly Vector2 Offset => new(Left, Top);

        public readonly Vector2 Size => new(Left + Right, Top + Bottom);

        public readonly Thickness Add(Thickness thickness)
        {
            return new Thickness(Left + thickness.Left, Top + thickness.Top, Right + thickness.Right, Bottom + thickness.Bottom);
        }

        public readonly Thickness Minus(Thickness thickness)
        {
            return new Thickness(Left - thickness.Left, Top - thickness.Top, Right - thickness.Right, Bottom - thickness.Bottom);
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is Thickness thickness && Equals(thickness);
        }

        public readonly bool Equals(Thickness other)
        {
            return Top == other.Top &&
                   Right == other.Right &&
                   Left == other.Left &&
                   Bottom == other.Bottom;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Top, Right, Left, Bottom);
        }

        public static implicit operator RectangleF(Thickness thickness) => new(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);

        public static implicit operator Thickness(RectangleF rect) => new(rect.Left, rect.Top, rect.Right, rect.Bottom);

        public static Thickness operator +(Thickness first, Thickness second)
        {
            return first.Add(second);
        }

        public static Thickness operator -(Thickness first, Thickness second)
        {
            return first.Minus(second);
        }

        public static bool operator ==(Thickness left, Thickness right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Thickness left, Thickness right)
        {
            return !(left == right);
        }

        public override readonly string ToString()
        {
            return $"T: {Top}, R: {Right}, L: {Left}, B: {Bottom}";
        }
    }
}