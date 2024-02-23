namespace HexaEngine.Mathematics
{
    using System;
    using System.Buffers.Binary;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a 2D signed integer point in space.
    /// </summary>
    public struct Point2 : IEquatable<Point2>
    {
        /// <summary>
        /// The X component of the point.
        /// </summary>
        public int X;

        /// <summary>
        /// The Y component of the point.
        /// </summary>
        public int Y;

        internal const int Count = 2;

        /// <summary>
        /// Gets a <see cref="Point2"/> instance with all elements set to zero.
        /// </summary>
        public static readonly Point2 Zero = new(0);

        /// <summary>
        /// Gets a <see cref="Point2"/> instance with all elements set to one.
        /// </summary>
        public static readonly Point2 One = new(1);

        /// <summary>
        /// Gets the <see cref="Point2"/> instance representing the X-axis unit vector (1, 0).
        /// </summary>
        public static readonly Point2 UnitX = new(1, 0);

        /// <summary>
        /// Gets the <see cref="Point2"/> instance representing the Y-axis unit vector (0, 1).
        /// </summary>
        public static readonly Point2 UnitY = new(0, 1);

        /// <summary>
        /// Initializes a new <see cref="Point2"/> instance with the specified value for all elements.
        /// </summary>
        /// <param name="value">The value to set for all elements of the point.</param>
        public Point2(int value)
        {
            X = value;
            Y = value;
        }

        /// <summary>
        /// Initializes a new <see cref="Point2"/> instance with the specified X, and Y components.
        /// </summary>
        /// <param name="x">The X component of the point.</param>
        /// <param name="y">The Y component of the point.</param>
        public Point2(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The index of the element to get or set.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown when the index is out of the valid range.</exception>
        public unsafe int this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException($"Index must be smaller than {Count} and larger or equals to 0");
                return ((int*)Unsafe.AsPointer(ref this))[index];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException($"Index must be smaller than {Count} and larger or equals to 0");
                ((int*)Unsafe.AsPointer(ref this))[index] = value;
            }
        }

        /// <summary>
        /// Determines whether the current <see cref="Point2"/> instance is equal to another <see cref="Point2"/> instance element-wise.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if the <see cref="Point2"/> instances are equal element-wise; otherwise, <c>false</c>.</returns>
        public override readonly bool Equals(object? obj)
        {
            return obj is Point2 point && Equals(point);
        }

        /// <summary>
        /// Determines whether the current <see cref="Point2"/> instance is equal to another <see cref="Point2"/> instance element-wise.
        /// </summary>
        /// <param name="other">The <see cref="Point2"/> to compare with the current instance.</param>
        /// <returns><c>true</c> if the <see cref="Point2"/> instances are equal element-wise; otherwise, <c>false</c>.</returns>
        public readonly bool Equals(Point2 other)
        {
            return X == other.X && Y == other.Y;
        }

        /// <summary>
        /// Returns a hash code for the current <see cref="Point2"/> instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        /// <summary>
        /// Determines whether two <see cref="Point2"/> instances are equal element-wise.
        /// </summary>
        /// <param name="left">The first <see cref="Point2"/> to compare.</param>
        /// <param name="right">The second <see cref="Point2"/> to compare.</param>
        /// <returns><c>true</c> if the <see cref="Point2"/> instances are equal element-wise; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Point2 left, Point2 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="Point2"/> instances are not equal element-wise.
        /// </summary>
        /// <param name="left">The first <see cref="Point2"/> to compare.</param>
        /// <param name="right">The second <see cref="Point2"/> to compare.</param>
        /// <returns><c>true</c> if the <see cref="Point2"/> instances are not equal element-wise; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Point2 left, Point2 right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Adds two <see cref="Point2"/> instances element-wise.
        /// </summary>
        /// <param name="left">The first <see cref="Point2"/> to add.</param>
        /// <param name="right">The second <see cref="Point2"/> to add.</param>
        /// <returns>The element-wise sum of the two <see cref="Point2"/> instances.</returns>
        public static Point2 operator +(Point2 left, Point2 right)
        {
            return new Point2(left.X + right.X, left.Y + right.Y);
        }

        /// <summary>
        /// Subtracts the right <see cref="Point2"/> from the left <see cref="Point2"/> element-wise.
        /// </summary>
        /// <param name="left">The <see cref="Point2"/> to subtract from (minuend).</param>
        /// <param name="right">The <see cref="Point2"/> to subtract (subtrahend).</param>
        /// <returns>The element-wise difference between the left and right <see cref="Point2"/> instances.</returns>
        public static Point2 operator -(Point2 left, Point2 right)
        {
            return new Point2(left.X - right.X, left.Y - right.Y);
        }

        /// <summary>
        /// Multiplies two <see cref="Point2"/> instances element-wise.
        /// </summary>
        /// <param name="left">The first <see cref="Point2"/> to multiply.</param>
        /// <param name="right">The second <see cref="Point2"/> to multiply.</param>
        /// <returns>The element-wise product of the two <see cref="Point2"/> instances.</returns>
        public static Point2 operator *(Point2 left, Point2 right)
        {
            return new Point2(left.X * right.X, left.Y * right.Y);
        }

        /// <summary>
        /// Divides the left <see cref="Point2"/> by the right <see cref="Point2"/> element-wise.
        /// </summary>
        /// <param name="left">The <see cref="Point2"/> to divide (dividend).</param>
        /// <param name="right">The <see cref="Point2"/> to divide by (divisor).</param>
        /// <returns>The element-wise division of the left <see cref="Point2"/> by the right <see cref="Point2"/> instances.</returns>
        public static Point2 operator /(Point2 left, Point2 right)
        {
            return new Point2(left.X / right.X, left.Y / right.Y);
        }

        /// <summary>
        /// Adds a constant value to each element of a <see cref="Point2"/> instance.
        /// </summary>
        /// <param name="left">The <see cref="Point2"/> instance to add to.</param>
        /// <param name="right">The constant value to add to each element.</param>
        /// <returns>A new <see cref="Point2"/> instance with each element increased by the constant value.</returns>
        public static Point2 operator +(Point2 left, int right)
        {
            return new Point2(left.X + right, left.Y + right);
        }

        /// <summary>
        /// Subtracts a constant value from each element of a <see cref="Point2"/> instance.
        /// </summary>
        /// <param name="left">The <see cref="Point2"/> instance to subtract from.</param>
        /// <param name="right">The constant value to subtract from each element.</param>
        /// <returns>A new <see cref="Point2"/> instance with each element decreased by the constant value.</returns>
        public static Point2 operator -(Point2 left, int right)
        {
            return new Point2(left.X - right, left.Y - right);
        }

        /// <summary>
        /// Multiplies each element of a <see cref="Point2"/> instance by a constant value.
        /// </summary>
        /// <param name="left">The <see cref="Point2"/> instance to multiply.</param>
        /// <param name="right">The constant value to multiply each element by.</param>
        /// <returns>A new <see cref="Point2"/> instance with each element multiplied by the constant value.</returns>
        public static Point2 operator *(Point2 left, int right)
        {
            return new Point2(left.X * right, left.Y * right);
        }

        /// <summary>
        /// Divides each element of a <see cref="Point2"/> instance by a constant value.
        /// </summary>
        /// <param name="left">The <see cref="Point2"/> instance to divide.</param>
        /// <param name="right">The constant value to divide each element by.</param>
        /// <returns>A new <see cref="Point2"/> instance with each element divided by the constant value.</returns>
        public static Point2 operator /(Point2 left, int right)
        {
            return new Point2(left.X / right, left.Y / right);
        }

        /// <summary>
        /// Increments all elements of a <see cref="Point2"/> instance by 1.
        /// </summary>
        /// <param name="point">The <see cref="Point2"/> instance to increment.</param>
        /// <returns>The <see cref="Point2"/> instance with all elements incremented by 1.</returns>
        public static Point2 operator ++(Point2 point)
        {
            return new Point2(point.X++, point.Y++);
        }

        /// <summary>
        /// Decrements all elements of a <see cref="Point2"/> instance by 1.
        /// </summary>
        /// <param name="point">The <see cref="Point2"/> instance to decrement.</param>
        /// <returns>The <see cref="Point2"/> instance with all elements decremented by 1.</returns>
        public static Point2 operator --(Point2 point)
        {
            return new Point2(point.X--, point.Y--);
        }

        /// <summary>
        /// Implicitly converts a <see cref="Vector2"/> to a <see cref="Point2"/>.
        /// </summary>
        /// <param name="vector">The Vector4 to convert to a <see cref="Point2"/>.</param>
        /// <returns>A <see cref="Point2"/> with each component rounded to the nearest unsigned integer value.</returns>
        public static implicit operator Point2(Vector2 vector) => new() { X = (int)vector.X, Y = (int)vector.Y };

        /// <summary>
        /// Implicitly converts a <see cref="Point2"/> to a <see cref="Vector2"/>.
        /// </summary>
        /// <param name="point">The <see cref="Point2"/> to convert to a <see cref="Vector2"/>.</param>
        /// <returns>A <see cref="Vector2"/> with each component equal to the respective <see cref="Point2"/> component as a float value.</returns>
        public static implicit operator Vector2(Point2 point) => new() { X = point.X, Y = point.Y };

        /// <summary>Returns the string representation of the current instance using default formatting.</summary>
        /// <returns>The string representation of the current instance.</returns>
        /// <remarks>This method returns a string in which each element of the vector is formatted using the "G" (general) format string and the formatting conventions of the current thread culture. The "&lt;" and "&gt;" characters are used to begin and end the string, and the current culture's <see cref="System.Globalization.NumberFormatInfo.NumberGroupSeparator" /> property followed by a space is used to separate each element.</remarks>
        public override readonly string ToString()
        {
            return ToString("G", CultureInfo.CurrentCulture);
        }

        /// <summary>Returns the string representation of the current instance using the specified format string to format individual elements.</summary>
        /// <param name="format">A standard or custom numeric format string that defines the format of individual elements.</param>
        /// <returns>The string representation of the current instance.</returns>
        /// <remarks>This method returns a string in which each element of the vector is formatted using <paramref name="format" /> and the current culture's formatting conventions. The "&lt;" and "&gt;" characters are used to begin and end the string, and the current culture's <see cref="System.Globalization.NumberFormatInfo.NumberGroupSeparator" /> property followed by a space is used to separate each element.</remarks>
        /// <related type="Article" href="/dotnet/standard/base-types/standard-numeric-format-strings">Standard Numeric Format Strings</related>
        /// <related type="Article" href="/dotnet/standard/base-types/custom-numeric-format-strings">Custom Numeric Format Strings</related>
        public readonly string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>Returns the string representation of the current instance using the specified format string to format individual elements and the specified format provider to define culture-specific formatting.</summary>
        /// <param name="format">A standard or custom numeric format string that defines the format of individual elements.</param>
        /// <param name="formatProvider">A format provider that supplies culture-specific formatting information.</param>
        /// <returns>The string representation of the current instance.</returns>
        /// <remarks>This method returns a string in which each element of the vector is formatted using <paramref name="format" /> and <paramref name="formatProvider" />. The "&lt;" and "&gt;" characters are used to begin and end the string, and the format provider's <see cref="System.Globalization.NumberFormatInfo.NumberGroupSeparator" /> property followed by a space is used to separate each element.</remarks>
        /// <related type="Article" href="/dotnet/standard/base-types/custom-numeric-format-strings">Custom Numeric Format Strings</related>
        /// <related type="Article" href="/dotnet/standard/base-types/standard-numeric-format-strings">Standard Numeric Format Strings</related>
        public readonly string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format, IFormatProvider? formatProvider)
        {
            string separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;

            return $"<{X.ToString(format, formatProvider)}{separator} {Y.ToString(format, formatProvider)}>";
        }

        /// <summary>
        /// Reads a <see cref="Point2"/> from a <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The source <see cref="Stream"/>.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The read <see cref="Point2"/>.</returns>
        public static Point2 Read(Stream stream, Endianness endianness)
        {
            Span<byte> src = stackalloc byte[8];
            stream.Read(src);
            Point2 point;
            if (endianness == Endianness.LittleEndian)
            {
                point.X = BinaryPrimitives.ReadInt32LittleEndian(src);
                point.Y = BinaryPrimitives.ReadInt32LittleEndian(src[4..]);
            }
            else
            {
                point.X = BinaryPrimitives.ReadInt32BigEndian(src);
                point.Y = BinaryPrimitives.ReadInt32BigEndian(src[4..]);
            }
            return point;
        }

        /// <summary>
        /// Writes a <see cref="Point2"/> to a <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The destination <see cref="Stream"/>.</param>
        /// <param name="endianness">The endianness.</param>
        public readonly void Write(Stream stream, Endianness endianness)
        {
            Span<byte> dst = stackalloc byte[8];
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteInt32LittleEndian(dst, X);
                BinaryPrimitives.WriteInt32LittleEndian(dst[4..], Y);
            }
            else
            {
                BinaryPrimitives.WriteInt32BigEndian(dst, X);
                BinaryPrimitives.WriteInt32BigEndian(dst[4..], Y);
            }

            stream.Write(dst);
        }
    }
}