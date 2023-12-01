namespace HexaEngine.Mathematics
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Represents a 2D rectangle with long integer coordinates.
    /// </summary>
    public struct Rect : IEquatable<Rect>
    {
        /// <summary>
        /// The left coordinate of the rectangle.
        /// </summary>
        public long Left;

        /// <summary>
        /// The top coordinate of the rectangle.
        /// </summary>
        public long Top;

        /// <summary>
        /// The right coordinate of the rectangle.
        /// </summary>
        public long Right;

        /// <summary>
        /// The bottom coordinate of the rectangle.
        /// </summary>
        public long Bottom;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rect"/> structure with long integer coordinates.
        /// </summary>
        /// <param name="left">The left coordinate of the rectangle.</param>
        /// <param name="top">The top coordinate of the rectangle.</param>
        /// <param name="right">The right coordinate of the rectangle.</param>
        /// <param name="bottom">The bottom coordinate of the rectangle.</param>
        public Rect(long left, long top, long right, long bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rect"/> structure using a position and size represented by <see cref="Point2"/>.
        /// </summary>
        /// <param name="pos">The position of the top-left corner of the rectangle.</param>
        /// <param name="size">The size of the rectangle represented by <see cref="Point2"/>.</param>
        public Rect(Point2 pos, Point2 size)
        {
            Left = pos.X;
            Top = pos.Y;
            Right = pos.X + size.X;
            Bottom = pos.Y + size.Y;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rect"/> structure using a position and size represented by <see cref="UPoint2"/>.
        /// </summary>
        /// <param name="pos">The position of the top-left corner of the rectangle.</param>
        /// <param name="size">The size of the rectangle represented by <see cref="UPoint2"/>.</param>
        public Rect(UPoint2 pos, UPoint2 size)
        {
            Left = pos.X;
            Top = pos.Y;
            Right = pos.X + size.X;
            Bottom = pos.Y + size.Y;
        }

        /// <summary>
        /// Gets the offset of the top-left corner of the rectangle as a 2D vector.
        /// </summary>
        public readonly Vector2 Offset => new(Left, Top);

        /// <summary>
        /// Gets the size of the rectangle as a 2D vector.
        /// </summary>
        public readonly Vector2 Size => new Vector2(Right, Bottom) - Offset;

        /// <summary>
        /// Determines whether the current Rect instance is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current Rect instance.</param>
        /// <returns>
        /// <c>true</c> if the specified object is equal to the current Rect instance; otherwise, <c>false</c>.
        /// </returns>
        public override readonly bool Equals(object? obj)
        {
            return obj is Rect rect && Equals(rect);
        }

        /// <summary>
        /// Determines whether the current Rect instance is equal to another Rect instance.
        /// </summary>
        /// <param name="other">The Rect to compare with the current Rect instance.</param>
        /// <returns>
        /// <c>true</c> if the specified Rect is equal to the current Rect instance; otherwise, <c>false</c>.
        /// </returns>
        public readonly bool Equals(Rect other)
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
        public static bool operator ==(Rect left, Rect right)
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
        public static bool operator !=(Rect left, Rect right)
        {
            return !(left == right);
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