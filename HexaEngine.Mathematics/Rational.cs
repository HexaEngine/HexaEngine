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
        /// Adds two rational numbers.
        /// </summary>
        /// <param name="left">The left rational number.</param>
        /// <param name="right">The right rational number.</param>
        /// <returns>The sum of the two rational numbers.</returns>
        public static Rational operator +(Rational left, Rational right)
        {
            if (left.Denominator == right.Denominator)
            {
                return new(left.Numerator + right.Numerator, left.Denominator);
            }

            uint numeratorLeft = left.Numerator * right.Denominator;
            uint numeratorRight = right.Numerator * left.Denominator;
            uint denominator = right.Denominator * left.Denominator;
            return new Rational(numeratorLeft + numeratorRight, denominator);
        }

        /// <summary>
        /// Subtracts one rational number from another.
        /// </summary>
        /// <param name="left">The left rational number.</param>
        /// <param name="right">The right rational number.</param>
        /// <returns>The result of subtracting the right rational number from the left rational number.</returns>
        public static Rational operator -(Rational left, Rational right)
        {
            if (left.Denominator == right.Denominator)
            {
                return new(left.Numerator - right.Numerator, left.Denominator);
            }

            uint numeratorLeft = left.Numerator * right.Denominator;
            uint numeratorRight = right.Numerator * left.Denominator;
            uint denominator = right.Denominator * left.Denominator;
            return new Rational(numeratorLeft - numeratorRight, denominator);
        }

        /// <summary>
        /// Multiplies two rational numbers.
        /// </summary>
        /// <param name="left">The left rational number.</param>
        /// <param name="right">The right rational number.</param>
        /// <returns>The product of the two rational numbers.</returns>
        public static Rational operator *(Rational left, Rational right)
        {
            uint numerator = left.Numerator * right.Numerator;
            uint denominator = left.Denominator * right.Denominator;
            return new(numerator, denominator);
        }

        /// <summary>
        /// Divides one rational number by another.
        /// </summary>
        /// <param name="left">The numerator of the dividend.</param>
        /// <param name="right">The numerator of the divisor.</param>
        /// <returns>The result of dividing the left rational number by the right rational number.</returns>
        public static Rational operator /(Rational left, Rational right)
        {
            uint numerator = left.Numerator * right.Denominator;
            uint denominator = left.Denominator * right.Numerator;
            return new(numerator, denominator);
        }

        /// <summary>
        /// Reduces the rational number to its simplest form.
        /// </summary>
        public void Reduce()
        {
            if (Numerator == Denominator)
            {
                Numerator = 1;
                Denominator = 1;
                return;
            }

            if (Denominator < 100000 && MathUtil.IsPrime(Denominator))
            {
                return;
            }

            uint gcd = MathUtil.GCD(Numerator, Denominator);
            Numerator /= gcd;
            Denominator /= gcd;
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