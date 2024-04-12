namespace HexaEngine.Mathematics
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Represents a color in the HSL (Hue, Saturation, Lightness, Alpha) color model.
    /// </summary>
    public struct ColorHSLA : IEquatable<ColorHSLA>
    {
        /// <summary>
        /// Gets or sets the hue component of the color (0 to 1).
        /// </summary>
        public float H;

        /// <summary>
        /// Gets or sets the saturation component of the color (0 to 1).
        /// </summary>
        public float S;

        /// <summary>
        /// Gets or sets the lightness component of the color (0 to 1).
        /// </summary>
        public float L;

        /// <summary>
        /// Gets or sets the alpha (transparency) component of the color (0 to 1).
        /// </summary>
        public float A;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorHSLA"/> struct.
        /// </summary>
        /// <param name="h">The hue component of the color (0 to 1).</param>
        /// <param name="s">The saturation component of the color (0 to 1).</param>
        /// <param name="l">The lightness component of the color (0 to 1).</param>
        /// <param name="a">The alpha (transparency) component of the color (0 to 1).</param>
        public ColorHSLA(float h, float s, float l, float a)
        {
            H = h;
            S = s;
            L = l;
            A = a;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorHSLA"/> struct from a <see cref="Color"/>.
        /// </summary>
        /// <param name="color">The RGBA color to convert to HSLA.</param>
        public ColorHSLA(Color color)
        {
            this = color.ToHSLA();
        }

        /// <summary>
        /// Converts the HSLA color to an RGBA color.
        /// </summary>
        /// <returns>An RGBA color equivalent to the HSLA color.</returns>
        public readonly Color ToRGBA()
        {
            if (S == 0)
            {
                // Achromatic (gray)
                return new Color(L, L, L, A);
            }

            float q = L < 0.5f ? L * (1 + S) : L + S - L * S;
            float p = 2 * L - q;

            float h = H;

            // Convert hue to RGB
            float r = HueToRGB(p, q, h + 1 / 3.0f);
            float g = HueToRGB(p, q, h);
            float b = HueToRGB(p, q, h - 1 / 3.0f);

            return new Color(r, g, b, A);
        }

        private static float HueToRGB(float p, float q, float t)
        {
            if (t < 0)
            {
                t += 1;
            }

            if (t > 1)
            {
                t -= 1;
            }

            if (t < 1 / 6.0f)
            {
                return p + (q - p) * 6 * t;
            }

            if (t < 1 / 2.0f)
            {
                return q;
            }

            if (t < 2 / 3.0f)
            {
                return p + (q - p) * (2 / 3.0f - t) * 6;
            }

            return p;
        }

        /// <summary>
        /// Converts the <see cref="ColorHSLA"/> to a <see cref="Vector4"/> in the format (H, S, L, A).
        /// </summary>
        /// <returns>A <see cref="Vector4"/> representing the HSLA components.</returns>
        public readonly Vector4 ToVector4()
        {
            return new Vector4(H, S, L, A);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="ColorHSLA"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="ColorHSLA"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current <see cref="ColorHSLA"/>; otherwise, <c>false</c>.</returns>
        public override readonly bool Equals(object? obj)
        {
            return obj is ColorHSLA hSLA && Equals(hSLA);
        }

        /// <summary>
        /// Determines whether the specified <see cref="ColorHSLA"/> is equal to the current <see cref="ColorHSLA"/>.
        /// </summary>
        /// <param name="other">The <see cref="ColorHSLA"/> to compare with the current <see cref="ColorHSLA"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="ColorHSLA"/> is equal to the current <see cref="ColorHSLA"/>; otherwise, <c>false</c>.</returns>
        public readonly bool Equals(ColorHSLA other)
        {
            return H == other.H &&
                   S == other.S &&
                   L == other.L &&
                   A == other.A;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current <see cref="ColorHSLA"/>.</returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(H, S, L, A);
        }

        /// <summary>
        /// Determines whether two specified <see cref="ColorHSLA"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="ColorHSLA"/> to compare.</param>
        /// <param name="right">The second <see cref="ColorHSLA"/> to compare.</param>
        /// <returns><c>true</c> if the specified <see cref="ColorHSLA"/> objects are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(ColorHSLA left, ColorHSLA right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two specified <see cref="ColorHSLA"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="ColorHSLA"/> to compare.</param>
        /// <param name="right">The second <see cref="ColorHSLA"/> to compare.</param>
        /// <returns><c>true</c> if the specified <see cref="ColorHSLA"/> objects are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(ColorHSLA left, ColorHSLA right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Explicitly converts a <see cref="ColorHSLA"/> to a <see cref="Color"/>.
        /// </summary>
        /// <param name="color">The <see cref="ColorHSLA"/> to convert.</param>
        /// <returns>The equivalent <see cref="Color"/> representation.</returns>
        public static explicit operator Color(ColorHSLA color)
        {
            return color.ToRGBA();
        }

        /// <summary>
        /// Explicitly converts a <see cref="Color"/> to a <see cref="ColorHSLA"/>.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to convert.</param>
        /// <returns>The equivalent <see cref="ColorHSLA"/> representation.</returns>
        public static explicit operator ColorHSLA(Color color)
        {
            return color.ToHSLA();
        }

        /// <summary>
        /// Returns a string representation of the color.
        /// </summary>
        /// <returns>A string representation of the color.</returns>
        public override readonly string ToString()
        {
            return $"<H: {H}, S: {S}, L: {L}, A: {A}>";
        }
    }
}