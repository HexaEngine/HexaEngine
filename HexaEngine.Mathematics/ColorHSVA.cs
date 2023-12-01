namespace HexaEngine.Mathematics
{
    using System.Numerics;

    /// <summary>
    /// Represents a color in the Hue, Saturation, Value, and Alpha color space.
    /// </summary>
    public struct ColorHSVA : IEquatable<ColorHSVA>
    {
        /// <summary>
        /// Gets or sets the hue component of the color (in the range [0, 1]).
        /// </summary>
        public float H;

        /// <summary>
        /// Gets or sets the saturation component of the color (in the range [0, 1]).
        /// </summary>
        public float S;

        /// <summary>
        /// Gets or sets the value component of the color (in the range [0, 1]).
        /// </summary>
        public float V;

        /// <summary>
        /// Gets or sets the alpha component of the color (in the range [0, 1]).
        /// </summary>
        public float A;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorHSVA"/> struct.
        /// </summary>
        /// <param name="h">The hue component (angle in degrees, from 0 to 1).</param>
        /// <param name="s">The saturation component (normalized, from 0 to 1).</param>
        /// <param name="v">The value component (normalized, from 0 to 1).</param>
        /// <param name="a">The alpha (transparency) component (normalized, from 0 to 1).</param>
        public ColorHSVA(float h, float s, float v, float a)
        {
            H = h;
            S = s;
            V = v;
            A = a;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorHSVA"/> struct from an RGBA color.
        /// </summary>
        /// <param name="color">The RGBA color to convert to HSVA.</param>
        public ColorHSVA(Color color)
        {
            this = color.ToHSVA();
        }

        /// <summary>
        /// Converts the color to a <see cref="Color"/> representation.
        /// </summary>
        /// <returns>The color as a <see cref="Color"/>.</returns>
        public readonly Color ToRGBA()
        {
            float chroma = V * S;
            float huePrime = H * 6;
            float x = chroma * (1 - Math.Abs(huePrime % 2 - 1));

            float r, g, b;

            if (huePrime >= 0 && huePrime < 1)
            {
                r = chroma;
                g = x;
                b = 0;
            }
            else if (huePrime >= 1 && huePrime < 2)
            {
                r = x;
                g = chroma;
                b = 0;
            }
            else if (huePrime >= 2 && huePrime < 3)
            {
                r = 0;
                g = chroma;
                b = x;
            }
            else if (huePrime >= 3 && huePrime < 4)
            {
                r = 0;
                g = x;
                b = chroma;
            }
            else if (huePrime >= 4 && huePrime < 5)
            {
                r = x;
                g = 0;
                b = chroma;
            }
            else
            {
                r = chroma;
                g = 0;
                b = x;
            }

            float m = V - chroma;

            return new Color(r + m, g + m, b + m, A);
        }

        /// <summary>
        /// Converts the <see cref="ColorHSVA"/> to a <see cref="Vector4"/> in the format (H, S, V, A).
        /// </summary>
        /// <returns>A <see cref="Vector4"/> representing the HSVA components.</returns>
        public readonly Vector4 ToVector4()
        {
            return new Vector4(H, S, V, A);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="ColorHSVA"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="ColorHSVA"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current <see cref="ColorHSVA"/>; otherwise, <c>false</c>.</returns>
        public override readonly bool Equals(object? obj)
        {
            return obj is ColorHSVA hSVA && Equals(hSVA);
        }

        /// <summary>
        /// Determines whether the specified <see cref="ColorHSVA"/> is equal to the current <see cref="ColorHSVA"/>.
        /// </summary>
        /// <param name="other">The <see cref="ColorHSVA"/> to compare with the current <see cref="ColorHSVA"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="ColorHSVA"/> is equal to the current <see cref="ColorHSVA"/>; otherwise, <c>false</c>.</returns>
        public readonly bool Equals(ColorHSVA other)
        {
            return H == other.H &&
                   S == other.S &&
                   V == other.V &&
                   A == other.A;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current <see cref="ColorHSVA"/>.</returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(H, S, V, A);
        }

        /// <summary>
        /// Determines whether two specified <see cref="ColorHSVA"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="ColorHSVA"/> to compare.</param>
        /// <param name="right">The second <see cref="ColorHSVA"/> to compare.</param>
        /// <returns><c>true</c> if the specified <see cref="ColorHSVA"/> objects are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(ColorHSVA left, ColorHSVA right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two specified <see cref="ColorHSVA"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="ColorHSVA"/> to compare.</param>
        /// <param name="right">The second <see cref="ColorHSVA"/> to compare.</param>
        /// <returns><c>true</c> if the specified <see cref="ColorHSVA"/> objects are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(ColorHSVA left, ColorHSVA right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Explicitly converts a <see cref="ColorHSVA"/> to a <see cref="Color"/>.
        /// </summary>
        /// <param name="color">The <see cref="ColorHSVA"/> to convert.</param>
        /// <returns>The equivalent <see cref="Color"/> representation.</returns>
        public static explicit operator Color(ColorHSVA color)
        {
            return color.ToRGBA();
        }

        /// <summary>
        /// Explicitly converts a <see cref="Color"/> to a <see cref="ColorHSVA"/>.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to convert.</param>
        /// <returns>The equivalent <see cref="ColorHSVA"/> representation.</returns>
        public static explicit operator ColorHSVA(Color color)
        {
            return color.ToHSVA();
        }

        /// <summary>
        /// Returns a string representation of the color.
        /// </summary>
        /// <returns>A string representation of the color.</returns>
        public override readonly string ToString()
        {
            return $"<H: {H}, S: {S}, V: {V}, A: {A}>";
        }
    }
}