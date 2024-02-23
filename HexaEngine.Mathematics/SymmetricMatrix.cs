namespace HexaEngine.Mathematics
{
    /// <summary>
    /// A symmetric matrix.
    /// </summary>
    public struct SymmetricMatrix : IEquatable<SymmetricMatrix>
    {
        /// <summary>
        /// The m11 component.
        /// </summary>
        public double M11;

        /// <summary>
        /// The m12 component.
        /// </summary>
        public double M12;

        /// <summary>
        /// The m13 component.
        /// </summary>
        public double M13;

        /// <summary>
        /// The m14 component.
        /// </summary>
        public double M14;

        /// <summary>
        /// The m22 component.
        /// </summary>
        public double M22;

        /// <summary>
        /// The m23 component.
        /// </summary>
        public double M23;

        /// <summary>
        /// The m24 component.
        /// </summary>
        public double M24;

        /// <summary>
        /// The m33 component.
        /// </summary>
        public double M33;

        /// <summary>
        /// The m34 component.
        /// </summary>
        public double M34;

        /// <summary>
        /// The m44 component.
        /// </summary>
        public double M44;

        /// <summary>
        /// Gets the component value with a specific index.
        /// </summary>
        /// <param name="index">The component index.</param>
        /// <returns>The value.</returns>
        public readonly double this[int index]
        {
            get
            {
                return index switch
                {
                    0 => M11,
                    1 => M12,
                    2 => M13,
                    3 => M14,
                    4 => M22,
                    5 => M23,
                    6 => M24,
                    7 => M33,
                    8 => M34,
                    9 => M44,
                    _ => throw new IndexOutOfRangeException(),
                };
            }
        }

        /// <summary>
        /// Creates a symmetric matrix with a value in each component.
        /// </summary>
        /// <param name="c">The component value.</param>
        public SymmetricMatrix(double c)
        {
            M11 = c;
            M12 = c;
            M13 = c;
            M14 = c;
            M22 = c;
            M23 = c;
            M24 = c;
            M33 = c;
            M34 = c;
            M44 = c;
        }

        /// <summary>
        /// Creates a symmetric matrix.
        /// </summary>
        /// <param name="m11">The m11 component.</param>
        /// <param name="m12">The m12 component.</param>
        /// <param name="m13">The m13 component.</param>
        /// <param name="m14">The m14 component.</param>
        /// <param name="m22">The m22 component.</param>
        /// <param name="m23">The m23 component.</param>
        /// <param name="m24">The m24 component.</param>
        /// <param name="m33">The m33 component.</param>
        /// <param name="m34">The m34 component.</param>
        /// <param name="m44">The m44 component.</param>
        public SymmetricMatrix(double m11, double m12, double m13, double m14, double m22, double m23, double m24, double m33, double m34, double m44)
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M14 = m14;
            M22 = m22;
            M23 = m23;
            M24 = m24;
            M33 = m33;
            M34 = m34;
            M44 = m44;
        }

        /// <summary>
        /// Creates a symmetric matrix from a plane.
        /// </summary>
        /// <param name="a">The plane x-component.</param>
        /// <param name="b">The plane y-component</param>
        /// <param name="c">The plane z-component</param>
        /// <param name="d">The plane w-component</param>
        public SymmetricMatrix(double a, double b, double c, double d)
        {
            M11 = a * a;
            M12 = a * b;
            M13 = a * c;
            M14 = a * d;

            M22 = b * b;
            M23 = b * c;
            M24 = b * d;

            M33 = c * c;
            M34 = c * d;

            M44 = d * d;
        }

        /// <summary>
        /// Adds two matrixes together.
        /// </summary>
        /// <param name="a">The left hand side.</param>
        /// <param name="b">The right hand side.</param>
        /// <returns>The resulting matrix.</returns>
        public static SymmetricMatrix operator +(SymmetricMatrix a, SymmetricMatrix b)
        {
            return new SymmetricMatrix(
                a.M11 + b.M11, a.M12 + b.M12, a.M13 + b.M13, a.M14 + b.M14,
                a.M22 + b.M22, a.M23 + b.M23, a.M24 + b.M24,
                a.M33 + b.M33, a.M34 + b.M34,
                a.M44 + b.M44
            );
        }

        /// <summary>
        /// Determines whether two <see cref="SymmetricMatrix"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="SymmetricMatrix"/> to compare.</param>
        /// <param name="right">The second <see cref="SymmetricMatrix"/> to compare.</param>
        /// <returns><see langword="true"/> if the specified <see cref="SymmetricMatrix"/> objects are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(SymmetricMatrix left, SymmetricMatrix right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="SymmetricMatrix"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="SymmetricMatrix"/> to compare.</param>
        /// <param name="right">The second <see cref="SymmetricMatrix"/> to compare.</param>
        /// <returns><see langword="true"/> if the specified <see cref="SymmetricMatrix"/> objects are not equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(SymmetricMatrix left, SymmetricMatrix right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Determinant(0, 1, 2, 1, 4, 5, 2, 5, 7)
        /// </summary>
        /// <returns></returns>
        public readonly double Determinant1()
        {
            double det =
                M11 * M22 * M33 +
                M13 * M12 * M23 +
                M12 * M23 * M13 -
                M13 * M22 * M13 -
                M11 * M23 * M23 -
                M12 * M12 * M33;
            return det;
        }

        /// <summary>
        /// Determinant(1, 2, 3, 4, 5, 6, 5, 7, 8)
        /// </summary>
        /// <returns></returns>
        public readonly double Determinant2()
        {
            double det =
                M12 * M23 * M34 +
                M14 * M22 * M33 +
                M13 * M24 * M23 -
                M14 * M23 * M23 -
                M12 * M24 * M33 -
                M13 * M22 * M34;
            return det;
        }

        /// <summary>
        /// Determinant(0, 2, 3, 1, 5, 6, 2, 7, 8)
        /// </summary>
        /// <returns></returns>
        public readonly double Determinant3()
        {
            double det =
                M11 * M23 * M34 +
                M14 * M12 * M33 +
                M13 * M24 * M13 -
                M14 * M23 * M13 -
                M11 * M24 * M33 -
                M13 * M12 * M34;
            return det;
        }

        /// <summary>
        /// Determinant(0, 1, 3, 1, 4, 6, 2, 5, 8)
        /// </summary>
        /// <returns></returns>
        public readonly double Determinant4()
        {
            double det =
                M11 * M22 * M34 +
                M14 * M12 * M23 +
                M12 * M24 * M13 -
                M14 * M22 * M13 -
                M11 * M24 * M23 -
                M12 * M12 * M34;
            return det;
        }

        /// <summary>
        /// Computes the determinant of this matrix.
        /// </summary>
        /// <param name="a11">The a11 index.</param>
        /// <param name="a12">The a12 index.</param>
        /// <param name="a13">The a13 index.</param>
        /// <param name="a21">The a21 index.</param>
        /// <param name="a22">The a22 index.</param>
        /// <param name="a23">The a23 index.</param>
        /// <param name="a31">The a31 index.</param>
        /// <param name="a32">The a32 index.</param>
        /// <param name="a33">The a33 index.</param>
        /// <returns>The determinant value.</returns>
        public readonly double Determinant(int a11, int a12, int a13,
            int a21, int a22, int a23,
            int a31, int a32, int a33)
        {
            double det =
                this[a11] * this[a22] * this[a33] +
                this[a13] * this[a21] * this[a32] +
                this[a12] * this[a23] * this[a31] -
                this[a13] * this[a22] * this[a31] -
                this[a11] * this[a23] * this[a32] -
                this[a12] * this[a21] * this[a33];
            return det;
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            return obj is SymmetricMatrix matrix && Equals(matrix);
        }

        /// <inheritdoc/>
        public readonly bool Equals(SymmetricMatrix other)
        {
            return M11 == other.M11 &&
                   M12 == other.M12 &&
                   M13 == other.M13 &&
                   M14 == other.M14 &&
                   M22 == other.M22 &&
                   M23 == other.M23 &&
                   M24 == other.M24 &&
                   M33 == other.M33 &&
                   M34 == other.M34 &&
                   M44 == other.M44;
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(M11);
            hash.Add(M12);
            hash.Add(M13);
            hash.Add(M14);
            hash.Add(M22);
            hash.Add(M23);
            hash.Add(M24);
            hash.Add(M33);
            hash.Add(M34);
            hash.Add(M44);
            return hash.ToHashCode();
        }
    }
}