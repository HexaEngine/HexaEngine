﻿namespace HexaEngine.Mathematics
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a 4D unsigned integer point in space.
    /// </summary>
    public struct UPoint4 : IEquatable<UPoint4>
    {
        /// <summary>
        /// The X component of the point.
        /// </summary>
        public uint X;

        /// <summary>
        /// The Y component of the point.
        /// </summary>
        public uint Y;

        /// <summary>
        /// The Z component of the point.
        /// </summary>
        public uint Z;

        /// <summary>
        /// The W component of the point.
        /// </summary>
        public uint W;

        internal const int Count = 4;

        /// <summary>
        /// Gets a <see cref="UPoint4"/> instance with all elements set to zero.
        /// </summary>
        public static UPoint4 Zero => new(0);

        /// <summary>
        /// Gets a <see cref="UPoint4"/> instance with all elements set to one.
        /// </summary>
        public static UPoint4 One => new(1);

        /// <summary>
        /// Gets the <see cref="UPoint4"/> instance representing the X-axis unit vector (1, 0, 0, 0).
        /// </summary>
        public static UPoint4 UnitX => new(1, 0, 0, 0);

        /// <summary>
        /// Gets the <see cref="UPoint4"/> instance representing the Y-axis unit vector (0, 1, 0, 0).
        /// </summary>
        public static UPoint4 UnitY => new(0, 1, 0, 0);

        /// <summary>
        /// Gets the <see cref="UPoint4"/> instance representing the Z-axis unit vector (0, 0, 1, 0).
        /// </summary>
        public static UPoint4 UnitZ => new(0, 0, 1, 0);

        /// <summary>
        /// Gets the <see cref="UPoint4"/> instance representing the W-axis unit vector (0, 0, 0, 1).
        /// </summary>
        public static UPoint4 UnitW => new(0, 0, 0, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="UPoint4"/> struct with all components set to the specified value.
        /// </summary>
        /// <param name="value">The value to set for all components (X, Y, Z, and W).</param>
        public UPoint4(uint value)
        {
            X = value;
            Y = value;
            Z = value;
            W = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UPoint4"/> struct with specified X, Y, Z, and W components.
        /// </summary>
        /// <param name="x">The value for the X component.</param>
        /// <param name="y">The value for the Y component.</param>
        /// <param name="z">The value for the Z component.</param>
        /// <param name="w">The value for the W component.</param>
        public UPoint4(uint x, uint y, uint z, uint w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UPoint4"/> struct with specified X, Y, Z, and W components.
        /// </summary>
        /// <param name="point">The value for the (X, Y) component</param>
        /// <param name="z">The value for the Z component.</param>
        /// <param name="w">The value for the W component.</param>
        public UPoint4(UPoint2 point, uint z, uint w)
        {
            X = point.X;
            Y = point.Y;
            Z = z;
            W = w;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UPoint4"/> struct with specified X, Y, Z, and W components.
        /// </summary>
        /// <param name="point">The value for the (X, Y, Z) component</param>
        /// <param name="w">The value for the W component.</param>
        public UPoint4(UPoint3 point, uint w)
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
        public unsafe uint this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new IndexOutOfRangeException($"Index must be smaller than {Count} and larger or equals to 0");
                }

                return ((uint*)Unsafe.AsPointer(ref this))[index];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (index < 0 || index >= Count)
                {
                    throw new IndexOutOfRangeException($"Index must be smaller than {Count} and larger or equals to 0");
                } ((uint*)Unsafe.AsPointer(ref this))[index] = value;
            }
        }

        /// <summary>
        /// Determines whether the current <see cref="UPoint4"/> instance is equal to another <see cref="UPoint4"/> instance element-wise.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if the <see cref="UPoint4"/> instances are equal element-wise; otherwise, <c>false</c>.</returns>
        public override readonly bool Equals(object? obj)
        {
            return obj is UPoint4 point && Equals(point);
        }

        /// <summary>
        /// Determines whether the specified <see cref="UPoint4"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="UPoint4"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="UPoint4"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public readonly bool Equals(UPoint4 other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Z == other.Z &&
                   W == other.W;
        }

        /// <summary>
        /// Returns a hash code for the current <see cref="UPoint4"/> instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z, W);
        }

        /// <summary>
        /// Determines whether two <see cref="UPoint4"/> instances are equal element-wise.
        /// </summary>
        /// <param name="left">The first <see cref="UPoint4"/> to compare.</param>
        /// <param name="right">The second <see cref="UPoint4"/> to compare.</param>
        /// <returns><c>true</c> if the <see cref="UPoint4"/> instances are equal element-wise; otherwise, <c>false</c>.</returns>
        public static bool operator ==(UPoint4 left, UPoint4 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="UPoint4"/> instances are not equal element-wise.
        /// </summary>
        /// <param name="left">The first <see cref="UPoint4"/> to compare.</param>
        /// <param name="right">The second <see cref="UPoint4"/> to compare.</param>
        /// <returns><c>true</c> if the <see cref="UPoint4"/> instances are not equal element-wise; otherwise, <c>false</c>.</returns>
        public static bool operator !=(UPoint4 left, UPoint4 right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Adds two <see cref="UPoint4"/> instances element-wise.
        /// </summary>
        /// <param name="left">The first <see cref="UPoint4"/> to add.</param>
        /// <param name="right">The second <see cref="UPoint4"/> to add.</param>
        /// <returns>The element-wise sum of the two <see cref="UPoint4"/> instances.</returns>
        public static UPoint4 operator +(UPoint4 left, UPoint4 right)
        {
            return new UPoint4(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
        }

        /// <summary>
        /// Subtracts the right <see cref="UPoint4"/> from the left <see cref="UPoint4"/> element-wise.
        /// </summary>
        /// <param name="left">The <see cref="UPoint4"/> to subtract from (minuend).</param>
        /// <param name="right">The <see cref="UPoint4"/> to subtract (subtrahend).</param>
        /// <returns>The element-wise difference between the left and right <see cref="UPoint4"/> instances.</returns>
        public static UPoint4 operator -(UPoint4 left, UPoint4 right)
        {
            return new UPoint4(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
        }

        /// <summary>
        /// Multiplies two <see cref="UPoint4"/> instances element-wise.
        /// </summary>
        /// <param name="left">The first <see cref="UPoint4"/> to multiply.</param>
        /// <param name="right">The second <see cref="UPoint4"/> to multiply.</param>
        /// <returns>The element-wise product of the two <see cref="UPoint4"/> instances.</returns>
        public static UPoint4 operator *(UPoint4 left, UPoint4 right)
        {
            return new UPoint4(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
        }

        /// <summary>
        /// Divides the left <see cref="UPoint4"/> by the right <see cref="UPoint4"/> element-wise.
        /// </summary>
        /// <param name="left">The <see cref="UPoint4"/> to divide (dividend).</param>
        /// <param name="right">The <see cref="UPoint4"/> to divide by (divisor).</param>
        /// <returns>The element-wise division of the left <see cref="UPoint4"/> by the right <see cref="UPoint4"/> instances.</returns>
        public static UPoint4 operator /(UPoint4 left, UPoint4 right)
        {
            return new UPoint4(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
        }

        /// <summary>
        /// Adds a constant value to each element of a <see cref="UPoint4"/> instance.
        /// </summary>
        /// <param name="left">The <see cref="UPoint4"/> instance to add to.</param>
        /// <param name="right">The constant value to add to each element.</param>
        /// <returns>A new <see cref="UPoint4"/> instance with each element increased by the constant value.</returns>
        public static UPoint4 operator +(UPoint4 left, uint right)
        {
            return new UPoint4(left.X + right, left.Y + right, left.Z + right, left.W + right);
        }

        /// <summary>
        /// Subtracts a constant value from each element of a <see cref="UPoint4"/> instance.
        /// </summary>
        /// <param name="left">The <see cref="UPoint4"/> instance to subtract from.</param>
        /// <param name="right">The constant value to subtract from each element.</param>
        /// <returns>A new <see cref="UPoint4"/> instance with each element decreased by the constant value.</returns>
        public static UPoint4 operator -(UPoint4 left, uint right)
        {
            return new UPoint4(left.X - right, left.Y - right, left.Z - right, left.W - right);
        }

        /// <summary>
        /// Multiplies each element of a <see cref="UPoint4"/> instance by a constant value.
        /// </summary>
        /// <param name="left">The <see cref="UPoint4"/> instance to multiply.</param>
        /// <param name="right">The constant value to multiply each element by.</param>
        /// <returns>A new <see cref="UPoint4"/> instance with each element multiplied by the constant value.</returns>
        public static UPoint4 operator *(UPoint4 left, uint right)
        {
            return new UPoint4(left.X * right, left.Y * right, left.Z * right, left.W * right);
        }

        /// <summary>
        /// Divides each element of a <see cref="UPoint4"/> instance by a constant value.
        /// </summary>
        /// <param name="left">The <see cref="UPoint4"/> instance to divide.</param>
        /// <param name="right">The constant value to divide each element by.</param>
        /// <returns>A new <see cref="UPoint4"/> instance with each element divided by the constant value.</returns>
        public static UPoint4 operator /(UPoint4 left, uint right)
        {
            return new UPoint4(left.X / right, left.Y / right, left.Z / right, left.W / right);
        }

        /// <summary>
        /// Increments all elements of a <see cref="UPoint4"/> instance by 1.
        /// </summary>
        /// <param name="point">The <see cref="UPoint4"/> instance to increment.</param>
        /// <returns>The <see cref="UPoint4"/> instance with all elements incremented by 1.</returns>
        public static UPoint4 operator ++(UPoint4 point)
        {
            return new UPoint4(point.X + 1, point.Y + 1, point.Z + 1, point.W + 1);
        }

        /// <summary>
        /// Decrements all elements of a <see cref="UPoint4"/> instance by 1.
        /// </summary>
        /// <param name="point">The <see cref="UPoint4"/> instance to decrement.</param>
        /// <returns>The <see cref="UPoint4"/> instance with all elements decremented by 1.</returns>
        public static UPoint4 operator --(UPoint4 point)
        {
            return new UPoint4(point.X - 1, point.Y - 1, point.Z - 1, point.W - 1);
        }

        /// <summary>
        /// Implicitly converts a <see cref="Vector4"/> to a <see cref="UPoint4"/>.
        /// </summary>
        /// <param name="vector">The Vector4 to convert to a <see cref="UPoint4"/>.</param>
        /// <returns>A <see cref="UPoint4"/> with each component rounded to the nearest unsigned integer value.</returns>
        public static implicit operator UPoint4(Vector4 vector) => new() { X = (uint)vector.X, Y = (uint)vector.Y, Z = (uint)vector.Z, W = (uint)vector.W };

        /// <summary>
        /// Implicitly converts a <see cref="UPoint4"/> to a <see cref="Vector4"/>.
        /// </summary>
        /// <param name="point">The <see cref="UPoint4"/> to convert to a <see cref="Vector4"/>.</param>
        /// <returns>A <see cref="Vector4"/> with each component equal to the respective <see cref="UPoint4"/> component as a float value.</returns>
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