namespace HexaEngine.Mathematics
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Represents a 2D rectangle with integer coordinates.
    /// </summary>
    public struct Rectangle : IEquatable<Rectangle>
    {
        /// <summary>
        /// The left coordinate of the rectangle.
        /// </summary>
        public int Left;

        /// <summary>
        /// The top coordinate of the rectangle.
        /// </summary>
        public int Top;

        /// <summary>
        /// The right coordinate of the rectangle.
        /// </summary>
        public int Right;

        /// <summary>
        /// The bottom coordinate of the rectangle.
        /// </summary>
        public int Bottom;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rectangle"/> structure with integer coordinates.
        /// </summary>
        /// <param name="left">The left coordinate of the rectangle.</param>
        /// <param name="top">The top coordinate of the rectangle.</param>
        /// <param name="right">The right coordinate of the rectangle.</param>
        /// <param name="bottom">The bottom coordinate of the rectangle.</param>
        public Rectangle(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rectangle"/> structure using a position and size represented by <see cref="Point2"/>.
        /// </summary>
        /// <param name="pos">The position of the top-left corner of the rectangle.</param>
        /// <param name="size">The size of the rectangle represented by <see cref="Point2"/>.</param>
        public Rectangle(Point2 pos, Point2 size)
        {
            Left = pos.X;
            Top = pos.Y;
            Right = pos.X + size.X;
            Bottom = pos.Y + size.Y;
        }

        /// <summary>
        /// Gets the offset of the top-left corner of the rectangle as a 2D vector.
        /// </summary>
        public readonly Point2 Offset => new(Left, Top);

        /// <summary>
        /// Gets the size of the rectangle as a 2D vector.
        /// </summary>
        public readonly Point2 Size => new Point2(Right, Bottom) - Offset;

        /// <summary>
        /// Determines whether the rectangle contains the specified point.
        /// </summary>
        public readonly bool Contains(Point2 point)
        {
            return point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;
        }

        /// <summary>
        /// Determines whether the rectangle contains the specified rectangle.
        /// </summary>
        public readonly bool Contains(Rectangle other)
        {
            return Left <= other.Left && Top <= other.Top && Right >= other.Right && Bottom >= other.Bottom;
        }

        /// <summary>
        /// Determines whether the rectangle intersects with another rectangle and calculates the intersection.
        /// </summary>
        public readonly bool Intersects(Rectangle other, out Rectangle intersection)
        {
            int intersectLeft = Math.Max(Left, other.Left);
            int intersectTop = Math.Max(Top, other.Top);
            int intersectRight = Math.Min(Right, other.Right);
            int intersectBottom = Math.Min(Bottom, other.Bottom);

            if (intersectLeft < intersectRight && intersectTop < intersectBottom)
            {
                intersection = new Rectangle(intersectLeft, intersectTop, intersectRight, intersectBottom);
                return true;
            }

            intersection = default;
            return false;
        }

        /// <summary>
        /// Determines whether the rectangle intersects with another rectangle.
        /// </summary>
        public readonly bool Intersects(Rectangle other)
        {
            return Left < other.Right && Right > other.Left && Top < other.Bottom && Bottom > other.Top;
        }

        /// <summary>
        /// Merges the current rectangle with another rectangle.
        /// </summary>
        public readonly Rectangle Merge(Rectangle other)
        {
            return new(Math.Min(Left, other.Left), Math.Min(Top, other.Top), Math.Max(Right, other.Right), Math.Max(Bottom, other.Bottom));
        }

        /// <summary>
        /// Determines whether the current Rect instance is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current Rect instance.</param>
        /// <returns>
        /// <c>true</c> if the specified object is equal to the current Rect instance; otherwise, <c>false</c>.
        /// </returns>
        public override readonly bool Equals(object? obj)
        {
            return obj is Rectangle rect && Equals(rect);
        }

        /// <summary>
        /// Determines whether the current Rect instance is equal to another Rect instance.
        /// </summary>
        /// <param name="other">The Rect to compare with the current Rect instance.</param>
        /// <returns>
        /// <c>true</c> if the specified Rect is equal to the current Rect instance; otherwise, <c>false</c>.
        /// </returns>
        public readonly bool Equals(Rectangle other)
        {
            return Left == other.Left &&
                   Top == other.Top &&
                   Right == other.Right &&
                   Bottom == other.Bottom;
        }

        /// <summary>
        /// Gets the hash code for the Rect instance.
        /// </summary>
        /// <returns>A hash code for the current Rect instance.</returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Left, Top, Right, Bottom);
        }

        /// <summary>
        /// Determines whether two Rect instances are equal.
        /// </summary>
        /// <param name="left">The first Rect to compare.</param>
        /// <param name="right">The second Rect to compare.</param>
        /// <returns>
        /// <c>true</c> if the specified Rect instances are equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(Rectangle left, Rectangle right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two Rect instances are not equal.
        /// </summary>
        /// <param name="left">The first Rect to compare.</param>
        /// <param name="right">The second Rect to compare.</param>
        /// <returns>
        /// <c>true</c> if the specified Rect instances are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(Rectangle left, Rectangle right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Adds two rectangles by combining their coordinates.
        /// </summary>
        /// <param name="left">The first rectangle.</param>
        /// <param name="right">The second rectangle.</param>
        /// <returns>A new rectangle resulting from the addition.</returns>
        public static Rectangle operator +(Rectangle left, Rectangle right)
        {
            return new(left.Left - right.Left, left.Top - right.Top, left.Right + right.Right, left.Bottom + right.Bottom);
        }

        /// <summary>
        /// Subtracts the coordinates of the second rectangle from the first rectangle.
        /// </summary>
        /// <param name="left">The first rectangle.</param>
        /// <param name="right">The second rectangle.</param>
        /// <returns>A new rectangle resulting from the subtraction.</returns>
        public static Rectangle operator -(Rectangle left, Rectangle right)
        {
            return new(left.Left + right.Left, left.Top + right.Top, left.Right - right.Right, left.Bottom - right.Bottom);
        }

        /// <summary>
        /// Translates the rectangle by adding the specified vector to its coordinates.
        /// </summary>
        /// <param name="left">The rectangle.</param>
        /// <param name="right">The vector to add to the rectangle's coordinates.</param>
        /// <returns>A new rectangle resulting from the translation.</returns>
        public static Rectangle operator +(Rectangle left, Vector2 right)
        {
            return new((int)(left.Left + right.X), (int)(left.Top + right.Y), (int)(left.Right + right.X), (int)(left.Bottom + right.Y));
        }

        /// <summary>
        /// Translates the rectangle by subtracting the specified vector from its coordinates.
        /// </summary>
        /// <param name="left">The rectangle.</param>
        /// <param name="right">The vector to subtract from the rectangle's coordinates.</param>
        /// <returns>A new rectangle resulting from the translation.</returns>
        public static Rectangle operator -(Rectangle left, Vector2 right)
        {
            return new((int)(left.Left - right.X), (int)(left.Top - right.Y), (int)(left.Right - right.X), (int)(left.Bottom - right.Y));
        }

        /// <summary>
        /// Translates the rectangle by adding the specified vector to its coordinates.
        /// </summary>
        /// <param name="left">The rectangle.</param>
        /// <param name="right">The vector to add to the rectangle's coordinates.</param>
        /// <returns>A new rectangle resulting from the translation.</returns>
        public static Rectangle operator +(Rectangle left, Point2 right)
        {
            return new(left.Left + right.X, left.Top + right.Y, left.Right + right.X, left.Bottom + right.Y);
        }

        /// <summary>
        /// Translates the rectangle by subtracting the specified vector from its coordinates.
        /// </summary>
        /// <param name="left">The rectangle.</param>
        /// <param name="right">The vector to subtract from the rectangle's coordinates.</param>
        /// <returns>A new rectangle resulting from the translation.</returns>
        public static Rectangle operator -(Rectangle left, Point2 right)
        {
            return new(left.Left - right.X, left.Top - right.Y, left.Right - right.X, left.Bottom - right.Y);
        }

        /// <summary>
        /// Scales the rectangle by adding the specified value to its coordinates.
        /// </summary>
        /// <param name="left">The rectangle.</param>
        /// <param name="right">The value to add to the rectangle's coordinates.</param>
        /// <returns>A new rectangle resulting from the scaling.</returns>
        public static Rectangle operator +(Rectangle left, float right)
        {
            return new((int)(left.Left + right), (int)(left.Top + right), (int)(left.Right + right), (int)(left.Bottom + right));
        }

        /// <summary>
        /// Scales the rectangle by subtracting the specified value from its coordinates.
        /// </summary>
        /// <param name="left">The rectangle.</param>
        /// <param name="right">The value to subtract from the rectangle's coordinates.</param>
        /// <returns>A new rectangle resulting from the scaling.</returns>
        public static Rectangle operator -(Rectangle left, float right)
        {
            return new((int)(left.Left - right), (int)(left.Top - right), (int)(left.Right - right), (int)(left.Bottom - right));
        }

        /// <summary>
        /// Scales the rectangle by multiplying its coordinates by the specified value.
        /// </summary>
        /// <param name="left">The rectangle.</param>
        /// <param name="right">The value to multiply the rectangle's coordinates by.</param>
        /// <returns>A new rectangle resulting from the scaling.</returns>
        public static Rectangle operator *(Rectangle left, float right)
        {
            return new((int)(left.Left * right), (int)(left.Top * right), (int)(left.Right * right), (int)(left.Bottom * right));
        }

        /// <summary>
        /// Scales the rectangle by dividing its coordinates by the specified value.
        /// </summary>
        /// <param name="left">The rectangle.</param>
        /// <param name="right">The value to divide the rectangle's coordinates by.</param>
        /// <returns>A new rectangle resulting from the scaling.</returns>
        public static Rectangle operator /(Rectangle left, float right)
        {
            return new((int)(left.Left / right), (int)(left.Top / right), (int)(left.Right / right), (int)(left.Bottom / right));
        }

        /// <summary>
        /// Scales the rectangle by adding the specified value to its coordinates.
        /// </summary>
        /// <param name="left">The rectangle.</param>
        /// <param name="right">The value to add to the rectangle's coordinates.</param>
        /// <returns>A new rectangle resulting from the scaling.</returns>
        public static Rectangle operator +(Rectangle left, int right)
        {
            return new(left.Left + right, left.Top + right, left.Right + right, left.Bottom + right);
        }

        /// <summary>
        /// Scales the rectangle by subtracting the specified value from its coordinates.
        /// </summary>
        /// <param name="left">The rectangle.</param>
        /// <param name="right">The value to subtract from the rectangle's coordinates.</param>
        /// <returns>A new rectangle resulting from the scaling.</returns>
        public static Rectangle operator -(Rectangle left, int right)
        {
            return new(left.Left - right, left.Top - right, left.Right - right, left.Bottom - right);
        }

        /// <summary>
        /// Scales the rectangle by multiplying its coordinates by the specified value.
        /// </summary>
        /// <param name="left">The rectangle.</param>
        /// <param name="right">The value to multiply the rectangle's coordinates by.</param>
        /// <returns>A new rectangle resulting from the scaling.</returns>
        public static Rectangle operator *(Rectangle left, int right)
        {
            return new(left.Left * right, left.Top * right, left.Right * right, left.Bottom * right);
        }

        /// <summary>
        /// Scales the rectangle by dividing its coordinates by the specified value.
        /// </summary>
        /// <param name="left">The rectangle.</param>
        /// <param name="right">The value to divide the rectangle's coordinates by.</param>
        /// <returns>A new rectangle resulting from the scaling.</returns>
        public static Rectangle operator /(Rectangle left, int right)
        {
            return new(left.Left / right, left.Top / right, left.Right / right, left.Bottom / right);
        }

        /// <summary>
        /// Implicitly converts a integer rectangle to a float rectangle.
        /// </summary>
        /// <param name="rectangle"></param>
        public static implicit operator RectangleF(Rectangle rectangle)
        {
            return new(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
        }

        /// <summary>
        /// Returns a string representation of the Rect.
        /// </summary>
        /// <returns>A string containing the Left, Top, Right, and Bottom values of the Rect.</returns>
        public override readonly string ToString()
        {
            return $"Left: {Left}, Top: {Top}, Right: {Right}, Bottom: {Bottom}";
        }
    }
}