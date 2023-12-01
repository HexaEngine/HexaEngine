namespace HexaEngine.Mathematics
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Represents a 32-bit RGBA color with byte components.
    /// </summary>
    public struct Color32 : IEquatable<Color32>
    {
        /// <summary>
        /// Gets or sets the red component of the color.
        /// </summary>
        public byte R;

        /// <summary>
        /// Gets or sets the green component of the color.
        /// </summary>
        public byte G;

        /// <summary>
        /// Gets or sets the blue component of the color.
        /// </summary>
        public byte B;

        /// <summary>
        /// Gets or sets the alpha component of the color.
        /// </summary>
        public byte A;

        /// <summary>
        /// Initializes a new instance of the <see cref="Color32"/> struct with byte components.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="a">The alpha component.</param>
        public Color32(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color32"/> struct with float components.
        /// </summary>
        /// <param name="r">The red component (0.0 to 1.0).</param>
        /// <param name="g">The green component (0.0 to 1.0).</param>
        /// <param name="b">The blue component (0.0 to 1.0).</param>
        /// <param name="a">The alpha component (0.0 to 1.0).</param>
        public Color32(float r, float g, float b, float a)
        {
            Vector4 color = MathUtil.Clamp01(new Vector4(r, g, b, a));
            R = (byte)(color.X * byte.MaxValue);
            G = (byte)(color.Y * byte.MaxValue);
            B = (byte)(color.Z * byte.MaxValue);
            A = (byte)(color.W * byte.MaxValue);
        }

        /// <summary>
        /// Converts the color to a 32-bit unsigned integer representation.
        /// </summary>
        /// <returns>The 32-bit unsigned integer representation of the color.</returns>
        public Color32(uint color)
        {
            R = (byte)((color >> 24) & 0xff);
            G = (byte)((color >> 16) & 0xff);
            B = (byte)((color >> 8) & 0xff);
            A = (byte)((color) & 0xff);
        }

        /// <summary>
        /// Converts the color to a Vector4 representation.
        /// </summary>
        /// <returns>The Vector4 representation of the color with components in the range [0.0, 1.0].</returns>
        public Color32(Vector4 color)
        {
            color = MathUtil.Clamp01(color);
            R = (byte)(color.X * byte.MaxValue);
            G = (byte)(color.Y * byte.MaxValue);
            B = (byte)(color.Z * byte.MaxValue);
            A = (byte)(color.W * byte.MaxValue);
        }

        /// <summary>
        /// Converts the color to a ColorRGBA representation.
        /// </summary>
        /// <returns>The ColorRGBA representation of the color with components in the range [0.0, 1.0].</returns>
        public Color32(Color color)
        {
            color = Color.Saturate(color);
            R = (byte)(color.R * byte.MaxValue);
            G = (byte)(color.G * byte.MaxValue);
            B = (byte)(color.B * byte.MaxValue);
            A = (byte)(color.A * byte.MaxValue);
        }

        /// <summary>
        /// Converts the color to a 32-bit unsigned integer representation.
        /// </summary>
        /// <returns>The 32-bit unsigned integer representation of the color.</returns>
        public readonly uint ToUInt()
        {
            return ((uint)R << 24) | ((uint)G << 16) | ((uint)B << 8) | A;
        }

        /// <summary>
        /// Converts the color to a Vector4 representation.
        /// </summary>
        /// <returns>The Vector4 representation of the color with components in the range [0.0, 1.0].</returns>
        public readonly Vector4 ToVector4()
        {
            return new Vector4(R, G, B, A) / byte.MaxValue;
        }

        /// <summary>
        /// Determines whether this color is equal to another color.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>True if the colors are equal; otherwise, false.</returns>
        public override readonly bool Equals(object? obj)
        {
            return obj is Color32 color && Equals(color);
        }

        /// <summary>
        /// Determines whether this color is equal to another color.
        /// </summary>
        /// <param name="other">The color to compare.</param>
        /// <returns>True if the colors are equal; otherwise, false.</returns>
        public readonly bool Equals(Color32 other)
        {
            return ToUInt() == other.ToUInt();
        }

        /// <summary>
        /// Gets the hash code for this color.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(ToUInt());
        }

        /// <summary>
        /// Determines whether two colors are equal.
        /// </summary>
        /// <param name="left">The first color to compare.</param>
        /// <param name="right">The second color to compare.</param>
        /// <returns>True if the colors are equal; otherwise, false.</returns>
        public static bool operator ==(Color32 left, Color32 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two colors are not equal.
        /// </summary>
        /// <param name="left">The first color to compare.</param>
        /// <param name="right">The second color to compare.</param>
        /// <returns>True if the colors are not equal; otherwise, false.</returns>
        public static bool operator !=(Color32 left, Color32 right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Implicitly converts a <see cref="Color32"/> to a 32-bit unsigned integer representation.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The 32-bit unsigned integer representation of the color.</returns>
        public static implicit operator uint(Color32 color)
        {
            return color.ToUInt();
        }

        /// <summary>
        /// Implicitly converts a <see cref="Color32"/> to a <see cref="Vector4"/> representation.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The Vector4 representation of the color with components in the range [0.0, 1.0].</returns>
        public static implicit operator Vector4(Color32 color)
        {
            return color.ToVector4();
        }

        /// <summary>
        /// Implicitly converts a <see cref="Color32"/> to a <see cref="Color"/> representation.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The ColorRGBA representation of the color with components in the range [0.0, 1.0].</returns>
        public static explicit operator Color(Color32 color)
        {
            return new(color);
        }

        /// <summary>
        /// Adds two colors element-wise.
        /// </summary>
        /// <param name="left">The first color.</param>
        /// <param name="right">The second color.</param>
        /// <returns>The result of the addition.</returns>
        public static Color32 operator +(Color32 left, Color32 right)
        {
            Color32 result;
            result.R = (byte)(left.R + right.R);
            result.G = (byte)(left.G + right.G);
            result.B = (byte)(left.B + right.B);
            result.A = (byte)(left.A + right.A);
            return result;
        }

        /// <summary>
        /// Subtracts the second color from the first element-wise.
        /// </summary>
        /// <param name="left">The first color.</param>
        /// <param name="right">The second color.</param>
        /// <returns>The result of the subtraction.</returns>
        public static Color32 operator -(Color32 left, Color32 right)
        {
            Color32 result;
            result.R = (byte)(left.R - right.R);
            result.G = (byte)(left.G - right.G);
            result.B = (byte)(left.B - right.B);
            result.A = (byte)(left.A - right.A);
            return result;
        }

        /// <summary>
        /// Multiplies two colors element-wise.
        /// </summary>
        /// <param name="left">The first color.</param>
        /// <param name="right">The second color.</param>
        /// <returns>The result of the multiplication.</returns>
        public static Color32 operator *(Color32 left, Color32 right)
        {
            Color32 result;
            result.R = (byte)(left.R * right.R);
            result.G = (byte)(left.G * right.G);
            result.B = (byte)(left.B * right.B);
            result.A = (byte)(left.A * right.A);
            return result;
        }

        /// <summary>
        /// Divides the first color by the second element-wise.
        /// </summary>
        /// <param name="left">The numerator color.</param>
        /// <param name="right">The denominator color.</param>
        /// <returns>The result of the division.</returns>
        public static Color32 operator /(Color32 left, Color32 right)
        {
            Color32 result;
            result.R = (byte)(left.R / right.R);
            result.G = (byte)(left.G / right.G);
            result.B = (byte)(left.B / right.B);
            result.A = (byte)(left.A / right.A);
            return result;
        }

        /// <summary>
        /// Multiplies each component of the color by a scalar.
        /// </summary>
        /// <param name="left">The color to multiply.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>The result of the multiplication.</returns>
        public static Color32 operator *(Color32 left, float right)
        {
            Color32 result;
            result.R = (byte)(left.R * right);
            result.G = (byte)(left.G * right);
            result.B = (byte)(left.B * right);
            result.A = (byte)(left.A * right);
            return result;
        }

        /// <summary>
        /// Divides each component of the color by a scalar.
        /// </summary>
        /// <param name="left">The color to divide.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>The result of the division.</returns>
        public static Color32 operator /(Color32 left, float right)
        {
            Color32 result;
            result.R = (byte)(left.R / right);
            result.G = (byte)(left.G / right);
            result.B = (byte)(left.B / right);
            result.A = (byte)(left.A / right);
            return result;
        }

        /// <summary>
        /// Returns a string representation of the color.
        /// </summary>
        /// <returns>A string representation of the color.</returns>
        public override readonly string ToString()
        {
            return $"<R: {R}, G: {G}, B: {B}, A: {A}>";
        }
    }
}