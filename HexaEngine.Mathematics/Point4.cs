namespace HexaEngine.Mathematics
{
    using System;
    using System.Buffers.Binary;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.Intrinsics;

    /// <summary>
    /// Represents a 4D signed integer point in space.
    /// </summary>
    public struct Point4 : IEquatable<Point4>
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

        /// <summary>
        /// The W component of the point.
        /// </summary>
        public int W;

        internal const int Count = 4;

        /// <summary>
        /// Gets a <see cref="Point4"/> instance with all elements set to zero.
        /// </summary>
        public static Point4 Zero => new(0);

        /// <summary>
        /// Gets a <see cref="Point4"/> instance with all elements set to one.
        /// </summary>
        public static Point4 One => new(1);

        /// <summary>
        /// Gets the <see cref="Point4"/> instance representing the X-axis unit vector (1, 0, 0, 0).
        /// </summary>
        public static Point4 UnitX => new(1, 0, 0, 0);

        /// <summary>
        /// Gets the <see cref="Point4"/> instance representing the Y-axis unit vector (0, 1, 0, 0).
        /// </summary>
        public static Point4 UnitY => new(0, 1, 0, 0);

        /// <summary>
        /// Gets the <see cref="Point4"/> instance representing the Z-axis unit vector (0, 0, 1, 0).
        /// </summary>
        public static Point4 UnitZ => new(0, 0, 1, 0);

        /// <summary>
        /// Gets the <see cref="Point4"/> instance representing the W-axis unit vector (0, 0, 0, 1).
        /// </summary>
        public static Point4 UnitW => new(0, 0, 0, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="Point4"/> struct with all components set to the specified value.
        /// </summary>
        /// <param name="value">The value to set for all components (X, Y, Z, and W).</param>
        public Point4(int value)
        {
            X = value;
            Y = value;
            Z = value;
            W = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Point4"/> struct with specified X, Y, Z, and W components.
        /// </summary>
        /// <param name="x">The value for the X component.</param>
        /// <param name="y">The value for the Y component.</param>
        /// <param name="z">The value for the Z component.</param>
        /// <param name="w">The value for the W component.</param>
        public Point4(int x, int y, int z, int w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Point4"/> struct with specified X, Y, Z, and W components.
        /// </summary>
        /// <param name="point">The value for the (X, Y) component</param>
        /// <param name="z">The value for the Z component.</param>
        /// <param name="w">The value for the W component.</param>
        public Point4(Point2 point, int z, int w)
        {
            X = point.X;
            Y = point.Y;
            Z = z;
            W = w;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Point4"/> struct with specified X, Y, Z, and W components.
        /// </summary>
        /// <param name="point">The value for the (X, Y, Z) component</param>
        /// <param name="w">The value for the W component.</param>
        public Point4(Point3 point, int w)
        {
            X = point.X;
            Y = point.Y;
            Z = point.Z;
            W = w;
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
        /// Determines whether the current <see cref="Point4"/> instance is equal to another <see cref="Point4"/> instance element-wise.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if the <see cref="Point4"/> instances are equal element-wise; otherwise, <c>false</c>.</returns>
        public override readonly bool Equals(object? obj)
        {
            return obj is Point4 point && Equals(point);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Point4"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Point4"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Point4"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public readonly bool Equals(Point4 other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Z == other.Z &&
                   W == other.W;
        }

        /// <summary>
        /// Returns a hash code for the current <see cref="Point4"/> instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z, W);
        }

        /// <summary>
        /// Determines whether two <see cref="Point4"/> instances are equal element-wise.
        /// </summary>
        /// <param name="left">The first <see cref="Point4"/> to compare.</param>
        /// <param name="right">The second <see cref="Point4"/> to compare.</param>
        /// <returns><c>true</c> if the <see cref="Point4"/> instances are equal element-wise; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Point4 left, Point4 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="Point4"/> instances are not equal element-wise.
        /// </summary>
        /// <param name="left">The first <see cref="Point4"/> to compare.</param>
        /// <param name="right">The second <see cref="Point4"/> to compare.</param>
        /// <returns><c>true</c> if the <see cref="Point4"/> instances are not equal element-wise; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Point4 left, Point4 right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Adds two <see cref="Point4"/> instances element-wise.
        /// </summary>
        /// <param name="left">The first <see cref="Point4"/> to add.</param>
        /// <param name="right">The second <see cref="Point4"/> to add.</param>
        /// <returns>The element-wise sum of the two <see cref="Point4"/> instances.</returns>
        public static Point4 operator +(Point4 left, Point4 right)
        {
            return new Point4(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
        }

        /// <summary>
        /// Subtracts the right <see cref="Point4"/> from the left <see cref="Point4"/> element-wise.
        /// </summary>
        /// <param name="left">The <see cref="Point4"/> to subtract from (minuend).</param>
        /// <param name="right">The <see cref="Point4"/> to subtract (subtrahend).</param>
        /// <returns>The element-wise difference between the left and right <see cref="Point4"/> instances.</returns>
        public static Point4 operator -(Point4 left, Point4 right)
        {
            return new Point4(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
        }

        /// <summary>
        /// Multiplies two <see cref="Point4"/> instances element-wise.
        /// </summary>
        /// <param name="left">The first <see cref="Point4"/> to multiply.</param>
        /// <param name="right">The second <see cref="Point4"/> to multiply.</param>
        /// <returns>The element-wise product of the two <see cref="Point4"/> instances.</returns>
        public static Point4 operator *(Point4 left, Point4 right)
        {
            return new Point4(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
        }

        /// <summary>
        /// Divides the left <see cref="Point4"/> by the right <see cref="Point4"/> element-wise.
        /// </summary>
        /// <param name="left">The <see cref="Point4"/> to divide (dividend).</param>
        /// <param name="right">The <see cref="Point4"/> to divide by (divisor).</param>
        /// <returns>The element-wise division of the left <see cref="Point4"/> by the right <see cref="Point4"/> instances.</returns>
        public static Point4 operator /(Point4 left, Point4 right)
        {
            return new Point4(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
        }

        /// <summary>
        /// Adds a constant value to each element of a <see cref="Point4"/> instance.
        /// </summary>
        /// <param name="left">The <see cref="Point4"/> instance to add to.</param>
        /// <param name="right">The constant value to add to each element.</param>
        /// <returns>A new <see cref="Point4"/> instance with each element increased by the constant value.</returns>
        public static Point4 operator +(Point4 left, int right)
        {
            return new Point4(left.X + right, left.Y + right, left.Z + right, left.W + right);
        }

        /// <summary>
        /// Subtracts a constant value from each element of a <see cref="Point4"/> instance.
        /// </summary>
        /// <param name="left">The <see cref="Point4"/> instance to subtract from.</param>
        /// <param name="right">The constant value to subtract from each element.</param>
        /// <returns>A new <see cref="Point4"/> instance with each element decreased by the constant value.</returns>
        public static Point4 operator -(Point4 left, int right)
        {
            return new Point4(left.X - right, left.Y - right, left.Z - right, left.W - right);
        }

        /// <summary>
        /// Multiplies each element of a <see cref="Point4"/> instance by a constant value.
        /// </summary>
        /// <param name="left">The <see cref="Point4"/> instance to multiply.</param>
        /// <param name="right">The constant value to multiply each element by.</param>
        /// <returns>A new <see cref="Point4"/> instance with each element multiplied by the constant value.</returns>
        public static Point4 operator *(Point4 left, int right)
        {
            return new Point4(left.X * right, left.Y * right, left.Z * right, left.W * right);
        }

        /// <summary>
        /// Divides each element of a <see cref="Point4"/> instance by a constant value.
        /// </summary>
        /// <param name="left">The <see cref="Point4"/> instance to divide.</param>
        /// <param name="right">The constant value to divide each element by.</param>
        /// <returns>A new <see cref="Point4"/> instance with each element divided by the constant value.</returns>
        public static Point4 operator /(Point4 left, int right)
        {
            return new Point4(left.X / right, left.Y / right, left.Z / right, left.W / right);
        }

        /// <summary>
        /// Increments all elements of a <see cref="Point4"/> instance by 1.
        /// </summary>
        /// <param name="point">The <see cref="Point4"/> instance to increment.</param>
        /// <returns>The <see cref="Point4"/> instance with all elements incremented by 1.</returns>
        public static Point4 operator ++(Point4 point)
        {
            return new Point4(point.X + 1, point.Y + 1, point.Z + 1, point.W + 1);
        }

        /// <summary>
        /// Decrements all elements of a <see cref="Point4"/> instance by 1.
        /// </summary>
        /// <param name="point">The <see cref="Point4"/> instance to decrement.</param>
        /// <returns>The <see cref="Point4"/> instance with all elements decremented by 1.</returns>
        public static Point4 operator --(Point4 point)
        {
            return new Point4(point.X - 1, point.Y - 1, point.Z - 1, point.W - 1);
        }

        /// <summary>
        /// Implicitly converts a <see cref="Vector4"/> to a <see cref="Point4"/>.
        /// </summary>
        /// <param name="vector">The Vector4 to convert to a <see cref="Point4"/>.</param>
        /// <returns>A <see cref="Point4"/> with each component rounded to the nearest unsigned integer value.</returns>
        public static implicit operator Point4(Vector4 vector) => new() { X = (int)vector.X, Y = (int)vector.Y, Z = (int)vector.Z, W = (int)vector.W };

        /// <summary>
        /// Implicitly converts a <see cref="Point4"/> to a <see cref="Vector4"/>.
        /// </summary>
        /// <param name="point">The <see cref="Point4"/> to convert to a <see cref="Vector4"/>.</param>
        /// <returns>A <see cref="Vector4"/> with each component equal to the respective <see cref="Point4"/> component as a float value.</returns>
        public static implicit operator Vector4(Point4 point) => new() { X = point.X, Y = point.Y, Z = point.Z, W = point.W };

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

            return $"<{X.ToString(format, formatProvider)}{separator} {Y.ToString(format, formatProvider)}{separator} {Z.ToString(format, formatProvider)}{separator} {W.ToString(format, formatProvider)}>";
        }

        /// <summary>
        /// Reads a <see cref="Point4"/> from a <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The source <see cref="Stream"/>.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The read <see cref="Point4"/>.</returns>
        public static Point4 Read(Stream stream, Endianness endianness)
        {
            Span<byte> src = stackalloc byte[16];
            stream.Read(src);
            Point4 point;
            if (endianness == Endianness.LittleEndian)
            {
                point.X = BinaryPrimitives.ReadInt32LittleEndian(src);
                point.Y = BinaryPrimitives.ReadInt32LittleEndian(src[4..]);
                point.Z = BinaryPrimitives.ReadInt32LittleEndian(src[8..]);
                point.W = BinaryPrimitives.ReadInt32LittleEndian(src[12..]);
            }
            else
            {
                point.X = BinaryPrimitives.ReadInt32BigEndian(src);
                point.Y = BinaryPrimitives.ReadInt32BigEndian(src[4..]);
                point.Z = BinaryPrimitives.ReadInt32BigEndian(src[8..]);
                point.W = BinaryPrimitives.ReadInt32BigEndian(src[12..]);
            }
            return point;
        }

        /// <summary>
        /// Writes a <see cref="Point4"/> to a <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The destination <see cref="Stream"/>.</param>
        /// <param name="endianness">The endianness.</param>
        public readonly void Write(Stream stream, Endianness endianness)
        {
            Span<byte> dst = stackalloc byte[16];
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteInt32LittleEndian(dst, X);
                BinaryPrimitives.WriteInt32LittleEndian(dst[4..], Y);
                BinaryPrimitives.WriteInt32LittleEndian(dst[8..], Z);
                BinaryPrimitives.WriteInt32LittleEndian(dst[12..], W);
            }
            else
            {
                BinaryPrimitives.WriteInt32BigEndian(dst, X);
                BinaryPrimitives.WriteInt32BigEndian(dst[4..], Y);
                BinaryPrimitives.WriteInt32BigEndian(dst[8..], Z);
                BinaryPrimitives.WriteInt32BigEndian(dst[12..], W);
            }

            stream.Write(dst);
        }
    }
}