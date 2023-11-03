namespace HexaEngine.Mathematics
{
    using System;

    /// <summary>
    /// Represents a rational number as a fraction with a numerator and a denominator.
    /// </summary>
    public struct Rational : IEquatable<Rational>
    {
        /// <summary>
        /// Gets or sets the numerator of the rational number.
        /// </summary>
        public uint Numerator;

        /// <summary>
        /// Gets or sets the denominator of the rational number.
        /// </summary>
        public uint Denominator;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rational"/> struct with the specified numerator and denominator.
        /// </summary>
        /// <param name="numerator">The numerator of the rational number.</param>
        /// <param name="denominator">The denominator of the rational number.</param>
        public Rational(uint numerator, uint denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        /// <summary>
        /// Determines whether this <see cref="Rational"/> instance is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><c>true</c> if the objects are considered equal; otherwise, <c>false</c>.</returns>
        public override readonly bool Equals(object? obj)
        {
            return obj is Rational rational && Equals(rational);
        }

        /// <summary>
        /// Determines whether this <see cref="Rational"/> instance is equal to another instance.
        /// </summary>
        /// <param name="other">The <see cref="Rational"/> instance to compare with this instance.</param>
        /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
        public readonly bool Equals(Rational other)
        {
            return Numerator == other.Numerator &&
                   Denominator == other.Denominator;
        }

        /// <summary>
        /// Serves as a hash function for <see cref="Rational"/> instances.
        /// </summary>
        /// <returns>A hash code for this <see cref="Rational"/> instance.</returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Numerator, Denominator);
        }

        /// <summary>
        /// Determines whether two <see cref="Rational"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="Rational"/> instance to compare.</param>
        /// <param name="right">The second <see cref="Rational"/> instance to compare.</param>
        /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Rational left, Rational right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="Rational"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="Rational"/> instance to compare.</param>
        /// <param name="right">The second <see cref="Rational"/> instance to compare.</param>
        /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Rational left, Rational right)
        {
            return !(left == right);
        }
    }
}