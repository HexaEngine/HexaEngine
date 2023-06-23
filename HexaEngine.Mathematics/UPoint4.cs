namespace HexaEngine.Mathematics
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Numerics;

    public struct UPoint4 : IEquatable<UPoint4>
    {
        /// <summary>The X component of the point.</summary>
        public uint X;

        /// <summary>The Y component of the point.</summary>
        public uint Y;

        /// <summary>The Z component of the point.</summary>
        public uint Z;

        /// <summary>The W component of the point.</summary>
        public uint W;

        internal const int Count = 4;

        public UPoint4(uint value)
        {
            X = value;
            Y = value;
            Z = value;
            W = value;
        }

        public UPoint4(UPoint2 point, uint z, uint w)
        {
            X = point.X;
            Y = point.Y;
            Z = z;
            W = w;
        }

        public UPoint4(UPoint3 point, uint w)
        {
            X = point.X;
            Y = point.Y;
            Z = point.Z;
            W = w;
        }

        public UPoint4(uint x, uint y, uint z, uint w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        /// <summary>Gets a point whose 4 elements are equal to zero.</summary>
        /// <value>A point whose four elements are equal to zero (that is, it returns the point <c>(0,0,0,0)</c>.</value>
        public static UPoint4 Zero => new(0);

        /// <summary>Gets a point whose 4 elements are equal to one.</summary>
        /// <value>Returns <see cref="Point4" />.</value>
        /// <remarks>A point whose four elements are equal to one (that is, it returns the point <c>(1,1,1,1)</c>.</remarks>
        public static UPoint4 One => new(1);

        /// <summary>Gets the point (1,0,0,0).</summary>
        /// <value>The point <c>(1,0,0,0)</c>.</value>
        public static UPoint4 UnitX => new(1, 0, 0, 0);

        /// <summary>Gets the point (0,1,0,0).</summary>
        /// <value>The point <c>(0,1,0,0)</c>.</value>
        public static UPoint4 UnitY => new(0, 1, 0, 0);

        /// <summary>Gets the point (0,0,1,0).</summary>
        /// <value>The point <c>(0,0,1,0)</c>.</value>
        public static UPoint4 UnitZ => new(0, 0, 1, 0);

        /// <summary>Gets the point (0,0,0,1).</summary>
        /// <value>The point <c>(0,0,0,1)</c>.</value>
        public static UPoint4 UnitW => new(0, 0, 0, 1);

        public override bool Equals(object? obj)
        {
            return obj is UPoint4 point && Equals(point);
        }

        public bool Equals(UPoint4 other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Z == other.Z &&
                   W == other.W;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z, W);
        }

        public static bool operator ==(UPoint4 left, UPoint4 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UPoint4 left, UPoint4 right)
        {
            return !(left == right);
        }

        public static UPoint4 operator +(UPoint4 left, UPoint4 right)
        {
            return new UPoint4(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
        }

        public static UPoint4 operator -(UPoint4 left, UPoint4 right)
        {
            return new UPoint4(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
        }

        public static UPoint4 operator *(UPoint4 left, UPoint4 right)
        {
            return new UPoint4(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
        }

        public static UPoint4 operator /(UPoint4 left, UPoint4 right)
        {
            return new UPoint4(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
        }

        public static UPoint4 operator ++(UPoint4 point)
        {
            return new UPoint4(point.X + 1, point.Y + 1, point.Z + 1, point.W + 1);
        }

        public static UPoint4 operator --(UPoint4 point)
        {
            return new UPoint4(point.X - 1, point.Y - 1, point.Z - 1, point.W - 1);
        }

        public static implicit operator UPoint4(Vector4 vector) => new() { X = (uint)vector.X, Y = (uint)vector.Y, Z = (uint)vector.Z, W = (uint)vector.W };

        public static implicit operator Vector4(UPoint4 point) => new() { X = point.X, Y = point.Y, Z = point.Z, W = point.W };

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
    }
}