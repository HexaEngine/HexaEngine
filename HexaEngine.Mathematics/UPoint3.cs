namespace HexaEngine.Mathematics
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Numerics;

    public struct UPoint3 : IEquatable<UPoint3>
    {
        /// <summary>The X component of the point.</summary>
        public uint X;

        /// <summary>The Y component of the point.</summary>
        public uint Y;

        /// <summary>The Z component of the point.</summary>
        public uint Z;

        public static readonly UPoint3 Zero = new(0);

        public static readonly UPoint3 One = new(1);

        public static readonly UPoint3 UnitX = new(1, 0, 0);

        public static readonly UPoint3 UnitY = new(0, 1, 0);

        public static readonly UPoint3 UnitZ = new(0, 0, 1);

        public UPoint3(uint value)
        {
            X = value;
            Y = value;
            Z = value;
        }

        public UPoint3(UPoint2 point, uint z)
        {
            X = point.X;
            Y = point.Y;
            Z = z;
        }

        public UPoint3(uint x, uint y, uint z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override bool Equals(object? obj)
        {
            return obj is UPoint3 point && Equals(point);
        }

        public bool Equals(UPoint3 other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Z == other.Z;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public static bool operator ==(UPoint3 left, UPoint3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UPoint3 left, UPoint3 right)
        {
            return !(left == right);
        }

        public static UPoint3 operator +(UPoint3 left, UPoint3 right)
        {
            return new UPoint3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        public static UPoint3 operator -(UPoint3 left, UPoint3 right)
        {
            return new UPoint3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        public static UPoint3 operator *(UPoint3 left, UPoint3 right)
        {
            return new UPoint3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
        }

        public static UPoint3 operator /(UPoint3 left, UPoint3 right)
        {
            return new UPoint3(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
        }

        public static UPoint3 operator ++(UPoint3 point)
        {
            return new UPoint3(point.X + 1, point.Y + 1, point.Z + 1);
        }

        public static UPoint3 operator --(UPoint3 point)
        {
            return new UPoint3(point.X - 1, point.Y - 1, point.Z - 1);
        }

        public static implicit operator UPoint3(Vector3 vector) => new() { X = (uint)vector.X, Y = (uint)vector.Y, Z = (uint)vector.Z };

        public static implicit operator Vector3(UPoint3 point) => new() { X = point.X, Y = point.Y, Z = point.Z };

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