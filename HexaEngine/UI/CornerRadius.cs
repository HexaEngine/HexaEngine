namespace HexaEngine.UI
{
    using System;

    public struct CornerRadius : IEquatable<CornerRadius>
    {
        public float TopLeft;
        public float TopRight;
        public float BottomRight;
        public float BottomLeft;

        public CornerRadius(float radius)
        {
            TopRight = radius;
            TopLeft = radius;
            BottomLeft = radius;
            BottomRight = radius;
        }

        public CornerRadius(float topRight, float topLeft, float bottomLeft, float bottomRight)
        {
            TopRight = topRight;
            TopLeft = topLeft;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is CornerRadius radius && Equals(radius);
        }

        public readonly bool Equals(CornerRadius other)
        {
            return TopLeft == other.TopLeft &&
                   TopRight == other.TopRight &&
                   BottomRight == other.BottomRight &&
                   BottomLeft == other.BottomLeft;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(TopLeft, TopRight, BottomRight, BottomLeft);
        }

        public static bool operator ==(CornerRadius left, CornerRadius right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CornerRadius left, CornerRadius right)
        {
            return !(left == right);
        }
    }
}