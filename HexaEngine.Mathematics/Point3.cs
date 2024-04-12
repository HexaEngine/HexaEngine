namespace HexaEngine.Mathematics
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a 3D signed integer point in space.
    /// </summary>
    public struct Point3 : IEquatable<Point3>
    {
        /// <summary>
        /// The X component of the point.
        /// </summary>
        public int X;

        /// <summary>
        /// The Y component of the point.
        /// </summary>
        public int Y;

        /// <summary>
        /// The Z component of the point.
        /// </summary>
        public int Z;

        internal const int Count = 3;

        /// <summary>
        /// Gets a <see cref="Point3"/> instance with all elements set to zero.
        /// </summary>
        public static readonly Point3 Zero = new(0);

        /// <summary>
        /// Gets a <see cref="Point3"/> instance with all elements set to one.
        /// </summary>
        public static readonly Point3 One = new(1);

        /// <summary>
        /// Gets the <see cref="Point3"/> instance representing the X-axis unit vector (1, 0, 0).
        /// </summary>
        public static readonly Point3 UnitX = new(1, 0, 0);

        /// <summary>
        /// Gets the <see cref="Point3"/> instance representing the Y-axis unit vector (0, 1, 0).
        /// </summary>
        public static readonly Point3 UnitY = new(0, 1, 0);

        /// <summary>
        /// Gets the <see cref="Point3"/> instance representing the Z-axis unit vector (0, 0, 1).
        /// </summary>
        public static readonly Point3 UnitZ = new(0, 0, 1);

        /// <summary>
        /// Initializes a new <see cref="Point3"/> instance with the specified value for all elements.
        /// </summary>
        /// <param name="value">The value to set for all elements of the point.</param>
        public Point3(int value)
        {
            X = value;
            Y = value;
            Z = value;
        }

        /// <summary>
        /// Initializes a new <see cref="Point3"/> instance with the specified X, Y, and Z components.
        /// </summary>
        /// <param name="x">The X component of the point.</param>
        /// <param name="y">The Y component of the point.</param>
        /// <param name="z">The Z component of the point.</param>
        public Point3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Initializes a new <see cref="Point3"/> instance with the X and Y components from a UPoint2 and the specified Z component.
        /// </summary>
        /// <param name="point">The UPoint2 providing the X and Y components.</param>
        /// <param name="z">The Z component of the point.</param>
        public Point3(Point2 point, int z)
        {
            X = point.X;
            Y = point.Y;
            Z = z;
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
                {
                    throw new IndexOutOfRangeException($"Index must be smaller than {Count} and larger or equals to 0");
                }

                return ((int*)Unsafe.AsPointer(ref this))[index];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (index < 0 || index >= Count)
                {
                    throw new IndexOutOfRangeException($"Index must be smaller than {Count} and larger or equals to 0");
                } ((int*)Unsafe.AsPointer(ref this))[index] = value;
            }
        }

        /// <summary>
        /// Determines whether the current <see cref="Point3"/> instance is equal to another <see cref="Point3"/> instance element-wise.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if the <see cref="Point3"/> instances are equal element-wise; otherwise, <c>false</c>.</returns>
        public override readonly bool Equals(object? obj)
        {
            return obj is Point3 point && Equals(point);
        }

        /// <summary>
        /// Determines whether the current <see cref="Point3"/> instance is equal to another <see cref="Point3"/> instance element-wise.
        /// </summary>
        /// <param name="other">The <see cref="Point3"/> to compare with the current instance.</param>
        /// <returns><c>true</c> if the <see cref="Point3"/> instances are equal element-wise; otherwise, <c>false</c>.</returns>
        public readonly bool Equals(Point3 other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Z == other.Z;
        }

        /// <summary>
        /// Returns a hash code for the current <see cref="Point3"/> instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        /// <summary>
        /// Determines whether two <see cref="Point3"/> instances are equal element-wise.
        /// </summary>
        /// <param name="left">The first <see cref="Point3"/> to compare.</param>
        /// <param name="right">The second <see cref="Point3"/> to compare.</param>
        /// <returns><c>true</c> if the <see cref="Point3"/> instances are equal element-wise; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Point3 left, Point3 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="Point3"/> instances are not equal element-wise.
        /// </summary>
        /// <param name="left">The first <see cref="Point3"/> to compare.</param>
        /// <param name="right">The second <see cref="Point3"/> to compare.</param>
        /// <returns><c>true</c> if the <see cref="Point3"/> instances are not equal element-wise; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Point3 left, Point3 right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Adds two <see cref="Point3"/> instances element-wise.
        /// </summary>
        /// <param name="left">The first <see cref="Point3"/> to add.</param>
        /// <param name="right">The second <see cref="Point3"/> to add.</param>
        /// <returns>The element-wise sum of the two <see cref="Point3"/> instances.</returns>
        public static Point3 operator +(Point3 left, Point3 right)
        {
            return new Point3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        /// <summary>
        /// Subtracts the right <see cref="Point3"/> from the left <see cref="Point3"/> element-wise.
        /// </summary>
        /// <param name="left">The <see cref="Point3"/> to subtract from (minuend).</param>
        /// <param name="right">The <see cref="Point3"/> to subtract (subtrahend).</param>
        /// <returns>The element-wise difference between the left and right <see cref="Point3"/> instances.</returns>
        public static Point3 operator -(Point3 left, Point3 right)
        {
            return new Point3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        /// <summary>
        /// Multiplies two <see cref="Point3"/> instances element-wise.
        /// </summary>
        /// <param name="left">The first <see cref="Point3"/> to multiply.</param>
        /// <param name="right">The second <see cref="Point3"/> to multiply.</param>
        /// <returns>The element-wise product of the two <see cref="Point3"/> instances.</returns>
        public static Point3 operator *(Point3 left, Point3 right)
        {
            return new Point3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
        }

        /// <summary>
        /// Divides the left <see cref="Point3"/> by the right <see cref="Point3"/> element-wise.
        /// </summary>
        /// <param name="left">The <see cref="Point3"/> to divide (dividend).</param>
        /// <param name="right">The <see cref="Point3"/> to divide by (divisor).</param>
        /// <returns>The element-wise division of the left <see cref="Point3"/> by the right <see cref="Point3"/> instances.</returns>
        public static Point3 operator /(Point3 left, Point3 right)
        {
            return new Point3(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
        }

        /// <summary>
        /// Adds a constant value to each element of a <see cref="Point3"/> instance.
        /// </summary>
        /// <param name="left">The <see cref="Point3"/> instance to add to.</param>
        /// <param name="right">The constant value to add to each element.</param>
        /// <returns>A new <see cref="Point3"/> instance with each element increased by the constant value.</returns>
        public static Point3 operator +(Point3 left, int right)
        {
            return new Point3(left.X + right, left.Y + right, left.Z + right);
        }

        /// <summary>
        /// Subtracts a constant value from each element of a <see cref="Point3"/> instance.
        /// </summary>
        /// <param name="left">The <see cref="Point3"/> instance to subtract from.</param>
        /// <param name="right">The constant value to subtract from each element.</param>
        /// <returns>A new <see cref="Point3"/> instance with each element decreased by the constant value.</returns>
        public static Point3 operator -(Point3 left, int right)
        {
            return new Point3(left.X - right, left.Y - right, left.Z - right);
        }

        /// <summary>
        /// Multiplies each element of a <see cref="Point3"/> instance by a constant value.
        /// </summary>
        /// <param name="left">The <see cref="Point3"/> instance to multiply.</param>
        /// <param name="right">The constant value to multiply each element by.</param>
        /// <returns>A new <see cref="Point3"/> instance with each element multiplied by the constant value.</returns>
        public static Point3 operator *(Point3 left, int right)
        {
            return new Point3(left.X * right, left.Y * right, left.Z * right);
        }

        /// <summary>
        /// Divides each element of a <see cref="Point3"/> instance by a constant value.
        /// </summary>
        /// <param name="left">The <see cref="Point3"/> instance to divide.</param>
        /// <param name="right">The constant value to divide each element by.</param>
        /// <returns>A new <see cref="Point3"/> instance with each element divided by the constant value.</returns>
        public static Point3 operator /(Point3 left, int right)
        {
            return new Point3(left.X / right, left.Y / right, left.Z / right);
        }

        /// <summary>
        /// Increments all elements of a <see cref="Point3"/> instance by 1.
        /// </summary>
        /// <param name="point">The <see cref="Point3"/> instance to increment.</param>
        /// <returns>The <see cref="Point3"/> instance with all elements incremented by 1.</returns>
        public static Point3 operator ++(Point3 point)
        {
            return new Point3(point.X + 1, point.Y + 1, point.Z + 1);
        }

        /// <summary>
        /// Decrements all elements of a <see cref="Point3"/> instance by 1.
        /// </summary>
        /// <param name="point">The <see cref="Point3"/> instance to decrement.</param>
        /// <returns>The <see cref="Point3"/> instance with all elements decremented by 1.</returns>
        public static Point3 operator --(Point3 point)
        {
            return new Point3(point.X - 1, point.Y - 1, point.Z - 1);
        }

        /// <summary>
        /// Implicitly converts a <see cref="Vector3"/> to a <see cref="Point3"/>.
        /// </summary>
        /// <param name="vector">The Vector4 to convert to a <see cref="Point3"/>.</param>
        /// <returns>A <see cref="Point3"/> with each component rounded to the nearest unsigned integer value.</returns>
        public static implicit operator Point3(Vector3 vector) => new() { X = (int)vector.X, Y = (int)vector.Y, Z = (int)vector.Z };

        /// <summary>
        /// Implicitly converts a <see cref="Point3"/> to a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="point">The <see cref="Point3"/> to convert to a <see cref="Vector3"/>.</param>
        /// <returns>A <see cref="Vector3"/> with each component equal to the respective <see cref="Point3"/> component as a float value.</returns>
        public static implicit operator Vector3(Point3 point) => new() { X = point.X, Y = point.Y, Z = point.Z };

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

            return $"<{X.ToString(format, formatProvider)}{separator} {Y.ToString(format, formatProvider)}{separator} {Z.ToString(format, formatProvider)}>";
        }
    }
}