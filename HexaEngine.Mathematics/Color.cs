namespace HexaEngine.Mathematics
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Represents a color using floating-point values for each channel (R, G, B, A).
    /// </summary>
    public struct Color : IEquatable<Color>
    {
        /// <summary>
        /// Gets or sets the red channel value.
        /// </summary>
        public float R;

        /// <summary>
        /// Gets or sets the green channel value.
        /// </summary>
        public float G;

        /// <summary>
        /// Gets or sets the blue channel value.
        /// </summary>
        public float B;

        /// <summary>
        /// Gets or sets the alpha channel value.
        /// </summary>
        public float A;

        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct with specified channel values.
        /// </summary>
        /// <param name="r">The red channel value.</param>
        /// <param name="g">The green channel value.</param>
        /// <param name="b">The blue channel value.</param>
        /// <param name="a">The alpha channel value.</param>
        public Color(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct with specified channel values.
        /// </summary>
        /// <param name="r">The red channel value.</param>
        /// <param name="g">The green channel value.</param>
        /// <param name="b">The blue channel value.</param>
        /// <param name="a">The alpha channel value.</param>
        public Color(byte r, byte g, byte b, byte a)
        {
            R = r / (float)byte.MaxValue;
            G = g / (float)byte.MaxValue;
            B = b / (float)byte.MaxValue;
            A = a / (float)byte.MaxValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct from an unsigned integer representation.
        /// </summary>
        /// <param name="color">The unsigned integer representation of the color.</param>
        public Color(uint color)
        {
            R = (float)((color >> 24) & 0xff) / byte.MaxValue;
            G = (float)((color >> 16) & 0xff) / byte.MaxValue;
            B = (float)((color >> 8) & 0xff) / byte.MaxValue;
            A = (float)(color & 0xff) / byte.MaxValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct from a <see cref="Vector4"/> representation.
        /// </summary>
        /// <param name="color">The <see cref="Vector4"/> representation of the color.</param>
        public Color(Vector4 color)
        {
            R = color.X;
            G = color.Y;
            B = color.Z;
            A = color.W;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct from a <see cref="Color32"/> representation.
        /// </summary>
        /// <param name="color">The <see cref="Color32"/> representation of the color.</param>
        public Color(Color32 color)
        {
            var col = color.ToVector4();
            R = col.X;
            G = col.Y;
            B = col.Z;
            A = col.W;
        }

        public static Color FromABGR(uint color)
        {
            var a = (float)((color >> 24) & 0xff) / byte.MaxValue;
            var b = (float)((color >> 16) & 0xff) / byte.MaxValue;
            var g = (float)((color >> 8) & 0xff) / byte.MaxValue;
            var r = (float)(color & 0xff) / byte.MaxValue;
            return new Color(r, g, b, a);
        }

        /// <summary>
        /// Converts the color to an unsigned integer representation.
        /// </summary>
        /// <returns>The color as an unsigned integer.</returns>
        public readonly uint ToUIntRGBA()
        {
            var col = Saturate(this);
            byte r = (byte)(col.R * byte.MaxValue);
            byte g = (byte)(col.G * byte.MaxValue);
            byte b = (byte)(col.B * byte.MaxValue);
            byte a = (byte)(col.A * byte.MaxValue);
            return ((uint)r << 24) | ((uint)g << 16) | ((uint)b << 8) | a;
        }

        /// <summary>
        /// Converts the color to an unsigned integer representation.
        /// </summary>
        /// <returns>The color as an unsigned integer.</returns>
        public readonly uint ToUIntABGR()
        {
            var col = Saturate(this);
            byte r = (byte)(col.R * byte.MaxValue);
            byte g = (byte)(col.G * byte.MaxValue);
            byte b = (byte)(col.B * byte.MaxValue);
            byte a = (byte)(col.A * byte.MaxValue);
            return ((uint)a << 24) | ((uint)b << 16) | ((uint)g << 8) | r;
        }

        /// <summary>
        /// Converts the color to a <see cref="Vector4"/> representation.
        /// </summary>
        /// <returns>The color as a <see cref="Vector4"/>.</returns>
        public readonly Vector4 ToVector4()
        {
            return new Vector4(R, G, B, A);
        }

        /// <summary>
        /// Converts the color to a <see cref="ColorHSVA"/> representation.
        /// </summary>
        /// <returns>The color as a <see cref="ColorHSVA"/>.</returns>
        public readonly ColorHSVA ToHSVA()
        {
            float max = Math.Max(Math.Max(R, G), B);
            float min = Math.Min(Math.Min(R, G), B);

            float delta = max - min;

            float hue = 0;
            if (delta != 0)
            {
                if (max == R)
                    hue = (G - B) / delta + ((G < B) ? 6 : 0);
                else if (max == G)
                    hue = (B - R) / delta + 2;
                else
                    hue = (R - G) / delta + 4;

                hue /= 6;
            }

            float saturation = (max != 0) ? delta / max : 0;
            float value = max;

            return new ColorHSVA(hue, saturation, value, A);
        }

        /// <summary>
        /// Converts the color to a <see cref="ColorHSLA"/> representation.
        /// </summary>
        /// <returns>The color as a <see cref="ColorHSLA"/>.</returns>
        public readonly ColorHSLA ToHSLA()
        {
            float min = Math.Min(R, Math.Min(G, B));
            float max = Math.Max(R, Math.Max(G, B));
            float delta = max - min;

            // Calculate luminance (lightness)
            float l = (max + min) / 2;

            float h = 0, s = 0;

            // Calculate saturation
            if (delta != 0)
            {
                s = l < 0.5f ? delta / (max + min) : delta / (2 - max - min);

                // Calculate hue
                if (R == max)
                    h = (G - B) / delta + (G < B ? 6 : 0);
                else if (G == max)
                    h = (B - R) / delta + 2;
                else if (B == max)
                    h = (R - G) / delta + 4;
            }

            // Normalize hue to the range [0, 1]
            h /= 6;

            return new ColorHSLA(h, s, l, A);
        }

        /// <summary>
        /// Normalizes the color.
        /// </summary>
        /// <param name="color">The color to normalize.</param>
        /// <returns>The normalized color.</returns>
        public static Color Normalize(Color color)
        {
            Vector4 vector = color.ToVector4();
            vector = Vector4.Normalize(vector);
            return new Color(vector);
        }

        /// <summary>
        /// Saturates the color.
        /// </summary>
        /// <param name="color">The color to saturate.</param>
        /// <returns>The saturated color.</returns>
        public static Color Saturate(Color color)
        {
            Vector4 vector = color.ToVector4();
            vector = MathUtil.Clamp01(vector);
            return new Color(vector);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="Color"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current <see cref="Color"/>.</param>
        /// <returns><c>true</c> if the specified object is equal to the current <see cref="Color"/>; otherwise, <c>false</c>.</returns>
        public override readonly bool Equals(object? obj)
        {
            return obj is Color rGBA && Equals(rGBA);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Color"/> is equal to the current <see cref="Color"/>.
        /// </summary>
        /// <param name="other">The <see cref="Color"/> to compare with the current <see cref="Color"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="Color"/> is equal to the current <see cref="Color"/>; otherwise, <c>false</c>.</returns>
        public readonly bool Equals(Color other)
        {
            return R == other.R &&
                   G == other.G &&
                   B == other.B &&
                   A == other.A;
        }

        /// <summary>
        /// Returns the hash code for this <see cref="Color"/>.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(R, G, B, A);
        }

        /// <summary>
        /// Determines whether two <see cref="Color"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="Color"/> to compare.</param>
        /// <param name="right">The second <see cref="Color"/> to compare.</param>
        /// <returns><c>true</c> if the specified <see cref="Color"/> instances are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Color left, Color right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="Color"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="Color"/> to compare.</param>
        /// <param name="right">The second <see cref="Color"/> to compare.</param>
        /// <returns><c>true</c> if the specified <see cref="Color"/> instances are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Color left, Color right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Implicitly converts a <see cref="Color"/> to its unsigned integer representation.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to convert.</param>
        /// <returns>The unsigned integer representation of the <see cref="Color"/>.</returns>
        public static implicit operator uint(Color color)
        {
            return color.ToUIntRGBA();
        }

        /// <summary>
        /// Implicitly converts a <see cref="Color"/> to a <see cref="Vector4"/>.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to convert.</param>
        /// <returns>The <see cref="Vector4"/> representation of the <see cref="Color"/>.</returns>
        public static implicit operator Vector4(Color color)
        {
            return color.ToVector4();
        }

        /// <summary>
        /// Explicitly converts a <see cref="Color"/> to a <see cref="Color32"/>.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to convert.</param>
        /// <returns>The <see cref="Color32"/> representation of the <see cref="Color"/>.</returns>
        public static explicit operator Color32(Color color)
        {
            return new(color);
        }

        /// <summary>
        /// Adds two <see cref="Color"/> instances.
        /// </summary>
        /// <param name="left">The first <see cref="Color"/> operand.</param>
        /// <param name="right">The second <see cref="Color"/> operand.</param>
        /// <returns>A new <see cref="Color"/> representing the sum of the two input instances.</returns>
        public static Color operator +(Color left, Color right)
        {
            Color result;
            result.R = left.R + right.R;
            result.G = left.G + right.G;
            result.B = left.B + right.B;
            result.A = left.A + right.A;
            return result;
        }

        /// <summary>
        /// Subtracts the second <see cref="Color"/> instance from the first.
        /// </summary>
        /// <param name="left">The <see cref="Color"/> minuend.</param>
        /// <param name="right">The <see cref="Color"/> subtrahend.</param>
        /// <returns>A new <see cref="Color"/> representing the difference of the two input instances.</returns>
        public static Color operator -(Color left, Color right)
        {
            Color result;
            result.R = left.R - right.R;
            result.G = left.G - right.G;
            result.B = left.B - right.B;
            result.A = left.A - right.A;
            return result;
        }

        /// <summary>
        /// Multiplies two <see cref="Color"/> instances component-wise.
        /// </summary>
        /// <param name="left">The first <see cref="Color"/> operand.</param>
        /// <param name="right">The second <see cref="Color"/> operand.</param>
        /// <returns>A new <see cref="Color"/> representing the component-wise product of the two input instances.</returns>
        public static Color operator *(Color left, Color right)
        {
            Color result;
            result.R = left.R * right.R;
            result.G = left.G * right.G;
            result.B = left.B * right.B;
            result.A = left.A * right.A;
            return result;
        }

        /// <summary>
        /// Divides the first <see cref="Color"/> instance by the second component-wise.
        /// </summary>
        /// <param name="left">The <see cref="Color"/> dividend.</param>
        /// <param name="right">The <see cref="Color"/> divisor.</param>
        /// <returns>A new <see cref="Color"/> representing the component-wise division of the two input instances.</returns>
        public static Color operator /(Color left, Color right)
        {
            Color result;
            result.R = left.R / right.R;
            result.G = left.G / right.G;
            result.B = left.B / right.B;
            result.A = left.A / right.A;
            return result;
        }

        /// <summary>
        /// Multiplies each component of a <see cref="Color"/> by a scalar value.
        /// </summary>
        /// <param name="left">The <see cref="Color"/> operand.</param>
        /// <param name="right">The scalar operand.</param>
        /// <returns>A new <see cref="Color"/> representing the component-wise product of the input instance and the scalar value.</returns>
        public static Color operator *(Color left, float right)
        {
            Color result;
            result.R = left.R * right;
            result.G = left.G * right;
            result.B = left.B * right;
            result.A = left.A * right;
            return result;
        }

        /// <summary>
        /// Divides each component of a <see cref="Color"/> by a scalar value.
        /// </summary>
        /// <param name="left">The <see cref="Color"/> dividend.</param>
        /// <param name="right">The scalar divisor.</param>
        /// <returns>A new <see cref="Color"/> representing the component-wise division of the input instance by the scalar value.</returns>
        public static Color operator /(Color left, float right)
        {
            Color result;
            result.R = left.R / right;
            result.G = left.G / right;
            result.B = left.B / right;
            result.A = left.A / right;
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