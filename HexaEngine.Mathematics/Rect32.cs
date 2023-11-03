namespace HexaEngine.Mathematics
{
    using System.Numerics;

    /// <summary>
    /// Represents a 2D rectangle with integer coordinates.
    /// </summary>
    public struct Rect32
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
        /// Initializes a new instance of the <see cref="Rect32"/> structure with integer coordinates.
        /// </summary>
        /// <param name="left">The left coordinate of the rectangle.</param>
        /// <param name="top">The top coordinate of the rectangle.</param>
        /// <param name="right">The right coordinate of the rectangle.</param>
        /// <param name="bottom">The bottom coordinate of the rectangle.</param>
        public Rect32(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rect32"/> structure using a position and size represented by <see cref="Point2"/>.
        /// </summary>
        /// <param name="pos">The position of the top-left corner of the rectangle.</param>
        /// <param name="size">The size of the rectangle represented by <see cref="Point2"/>.</param>
        public Rect32(Point2 pos, Point2 size)
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
    }
}