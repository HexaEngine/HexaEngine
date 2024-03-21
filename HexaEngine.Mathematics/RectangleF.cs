namespace HexaEngine.Mathematics
{
    using System;
    using System.Numerics;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents a rectangle in floating-point coordinates.
    /// </summary>
    public struct RectangleF : IEquatable<RectangleF>
    {
        /// <summary>
        /// Gets the left coordinate of the rectangle.
        /// </summary>
        public float Left;

        /// <summary>
        /// Gets the top coordinate of the rectangle.
        /// </summary>
        public float Top;

        /// <summary>
        /// Gets the right coordinate of the rectangle.
        /// </summary>
        public float Right;

        /// <summary>
        /// Gets the bottom coordinate of the rectangle.
        /// </summary>
        public float Bottom;

        /// <summary>
        /// Initializes a new instance of the <see cref="RectangleF"/> struct with specified coordinates.
        /// </summary>
        public RectangleF(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RectangleF"/> struct with specified position and size.
        /// </summary>
        public RectangleF(Vector2 pos, Vector2 size)
        {
            Left = pos.X;
            Top = pos.Y;
            Right = pos.X + size.X;
            Bottom = pos.Y + size.Y;
        }

        /// <summary>
        /// Gets the offset of the rectangle.
        /// </summary>
        public readonly Vector2 Offset => new(Left, Top);

        /// <summary>
        /// Gets the size of the rectangle.
        /// </summary>
        public readonly Vector2 Size => new Vector2(Right, Bottom) - Offset;

        /// <summary>
        /// Determines whether the rectangle contains the specified point.
        /// </summary>
        public readonly bool Contains(Vector2 point)
        {
            return point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;
        }

        /// <summary>
        /// Determines whether the rectangle contains the specified rectangle.
        /// </summary>
        public readonly bool Contains(RectangleF other)
        {
            return Left <= other.Left && Top <= other.Top && Right >= other.Right && Bottom >= other.Bottom;
        }

        /// <summary>
        /// Determines whether the rectangle contains the specified ellipse.
        /// </summary>
        public readonly bool Contains(EllipseF ellipse)
        {
            Vector2 ellipseCenter = ellipse.Center;

            Vector2 distanceVector = ellipseCenter - Offset;

            Vector2 normalizedDistance = new(distanceVector.X / Size.X, distanceVector.Y / Size.Y);

            return normalizedDistance.X * normalizedDistance.X + normalizedDistance.Y * normalizedDistance.Y <= 1.0f;
        }

        /// <summary>
        /// Determines whether the rectangle intersects with another rectangle and calculates the intersection.
        /// </summary>
        public readonly bool Intersects(RectangleF other, out RectangleF intersection)
        {
            float intersectLeft = Math.Max(Left, other.Left);
            float intersectTop = Math.Max(Top, other.Top);
            float intersectRight = Math.Min(Right, other.Right);
            float intersectBottom = Math.Min(Bottom, other.Bottom);

            if (intersectLeft < intersectRight && intersectTop < intersectBottom)
            {
                intersection = new RectangleF(intersectLeft, intersectTop, intersectRight, intersectBottom);
                return true;
            }

            intersection = default;
            return false;
        }

        /// <summary>
        /// Determines whether the rectangle intersects with another rectangle.
        /// </summary>
        public readonly bool Intersects(RectangleF other)
        {
            return Left < other.Right && Right > other.Left && Top < other.Bottom && Bottom > other.Top;
        }

        /// <summary>
        /// Merges the current rectangle with another rectangle.
        /// </summary>
        public readonly RectangleF Merge(RectangleF other)
        {
            return new(MathF.Min(Left, other.Left), MathF.Min(Top, other.Top), MathF.Max(Right, other.Right), MathF.Max(Bottom, other.Bottom));
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            return obj is RectangleF f && Equals(f);
        }

        /// <inheritdoc/>
        public readonly bool Equals(RectangleF other)
        {
            return Left == other.Left &&
                   Top == other.Top &&
                   Right == other.Right &&
                   Bottom == other.Bottom;
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Left, Top, Right, Bottom);
        }

        /// <summary>
        /// Equality operator for comparing two rectangles.
        /// </summary>
        public static bool operator ==(RectangleF left, RectangleF right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator for comparing two rectangles.
        /// </summary>
        public static bool operator !=(RectangleF left, RectangleF right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Adds two rectangles by combining their coordinates.
        /// </summary>
        /// <param name="left">The first rectangle.</param>
        /// <param name="right">The second rectangle.</param>
        /// <returns>A new rectangle resulting from the addition.</returns>
        public static RectangleF operator +(RectangleF left, RectangleF right)
        {
            return new(left.Left - right.Left, left.Top - right.Top, left.Right + right.Right, left.Bottom + right.Bottom);
        }

        /// <summary>
        /// Subtracts the coordinates of the second rectangle from the first rectangle.
        /// </summary>
        /// <param name="left">The first rectangle.</param>
        /// <param name="right">The second rectangle.</param>
        /// <returns>A new rectangle resulting from the subtraction.</returns>
        public static RectangleF operator -(RectangleF left, RectangleF right)
        {
            return new(left.Left + right.Left, left.Top + right.Top, left.Right - right.Right, left.Bottom - right.Bottom);
        }

        /// <summary>
        /// Translates the rectangle by adding the specified vector to its coordinates.
        /// </summary>
        /// <param name="left">The rectangle.</param>
        /// <param name="right">The vector to add to the rectangle's coordinates.</param>
        /// <returns>A new rectangle resulting from the translation.</returns>
        public static RectangleF operator +(RectangleF left, Vector2 right)
        {
            return new(left.Left + right.X, left.Top + right.Y, left.Right + right.X, left.Bottom + right.Y);
        }

        /// <summary>
        /// Translates the rectangle by subtracting the specified vector from its coordinates.
        /// </summary>
        /// <param name="left">The rectangle.</param>
        /// <param name="right">The vector to subtract from the rectangle's coordinates.</param>
        /// <returns>A new rectangle resulting from the translation.</returns>
        public static RectangleF operator -(RectangleF left, Vector2 right)
        {
            return new(left.Left - right.X, left.Top - right.Y, left.Right - right.X, left.Bottom - right.Y);
        }

        /// <summary>
        /// Scales the rectangle by adding the specified value to its coordinates.
        /// </summary>
        /// <param name="left">The rectangle.</param>
        /// <param name="right">The value to add to the rectangle's coordinates.</param>
        /// <returns>A new rectangle resulting from the scaling.</returns>
        public static RectangleF operator +(RectangleF left, float right)
        {
            return new(left.Left + right, left.Top + right, left.Right + right, left.Bottom + right);
        }

        /// <summary>
        /// Scales the rectangle by subtracting the specified value from its coordinates.
        /// </summary>
        /// <param name="left">The rectangle.</param>
        /// <param name="right">The value to subtract from the rectangle's coordinates.</param>
        /// <returns>A new rectangle resulting from the scaling.</returns>
        public static RectangleF operator -(RectangleF left, float right)
        {
            return new(left.Left - right, left.Top - right, left.Right - right, left.Bottom - right);
        }

        /// <summary>
        /// Scales the rectangle by multiplying its coordinates by the specified value.
        /// </summary>
        /// <param name="left">The rectangle.</param>
        /// <param name="right">The value to multiply the rectangle's coordinates by.</param>
        /// <returns>A new rectangle resulting from the scaling.</returns>
        public static RectangleF operator *(RectangleF left, float right)
        {
            return new(left.Left * right, left.Top * right, left.Right * right, left.Bottom * right);
        }

        /// <summary>
        /// Scales the rectangle by dividing its coordinates by the specified value.
        /// </summary>
        /// <param name="left">The rectangle.</param>
        /// <param name="right">The value to divide the rectangle's coordinates by.</param>
        /// <returns>A new rectangle resulting from the scaling.</returns>
        public static RectangleF operator /(RectangleF left, float right)
        {
            return new(left.Left / right, left.Top / right, left.Right / right, left.Bottom / right);
        }

        /// <summary>
        /// Explicitly converts a float  rectangle to a integer rectangle.
        /// </summary>
        /// <param name="rectangle"></param>
        public static explicit operator Rectangle(RectangleF rectangle)
        {
            return new((int)rectangle.Left, (int)rectangle.Top, (int)rectangle.Right, (int)rectangle.Bottom);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"<Left: {Left}, Top: {Top}, Right: {Right}, Bottom: {Bottom}>";
        }

        public static RectangleF Union(RectangleF rect1, RectangleF rect2)
        {
            float left = Math.Min(rect1.Left, rect2.Left);
            float top = Math.Min(rect1.Top, rect2.Top);
            float right = Math.Max(rect1.Right, rect2.Right);
            float bottom = Math.Max(rect1.Bottom, rect2.Bottom);

            return new RectangleF(left, top, right, bottom);
        }

        public static RectangleF Transform(RectangleF rectangle, Matrix3x2 matrix)
        {
            Vector2 min = new(rectangle.Left, rectangle.Top);
            Vector2 max = new(rectangle.Right, rectangle.Bottom);
            min = Vector2.Transform(min, matrix);
            max = Vector2.Transform(max, matrix);
            return new(min.X, min.Y, max.X, max.Y);
        }

        public readonly Vector4 ToVec4()
        {
            return new(Left, Top, Right, Bottom);
        }
    }
}