namespace HexaEngine.Mathematics.Sky
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Represents a set of sky parameters used in environmental lighting calculations.
    /// </summary>
    public struct SkyParameters
    {
        /// <summary>
        /// Gets or sets the A vector.
        /// </summary>
        public Vector3 A;

        /// <summary>
        /// Gets or sets the B vector.
        /// </summary>
        public Vector3 B;

        /// <summary>
        /// Gets or sets the C vector.
        /// </summary>
        public Vector3 C;

        /// <summary>
        /// Gets or sets the D vector.
        /// </summary>
        public Vector3 D;

        /// <summary>
        /// Gets or sets the E vector.
        /// </summary>
        public Vector3 E;

        /// <summary>
        /// Gets or sets the F vector.
        /// </summary>
        public Vector3 F;

        /// <summary>
        /// Gets or sets the G vector.
        /// </summary>
        public Vector3 G;

        /// <summary>
        /// Gets or sets the I vector.
        /// </summary>
        public Vector3 I;

        /// <summary>
        /// Gets or sets the H vector.
        /// </summary>
        public Vector3 H;

        /// <summary>
        /// Gets or sets the Z vector.
        /// </summary>
        public Vector3 Z;

        /// <summary>
        /// The number of sky parameters in this struct.
        /// </summary>
        public const int Count = 10;

        /// <summary>
        /// Gets or sets the sky parameter at the specified index.
        /// </summary>
        /// <param name="index">The index of the sky parameter.</param>
        /// <value>The sky parameter at the specified index.</value>
        /// <exception cref="IndexOutOfRangeException">Thrown if the index is out of bounds.</exception>
        public Vector3 this[int index]
        {
            readonly get
            {
                return index switch
                {
                    0 => A,
                    1 => B,
                    2 => C,
                    3 => D,
                    4 => E,
                    5 => F,
                    6 => G,
                    7 => I,
                    8 => H,
                    9 => Z,
                    _ => throw new IndexOutOfRangeException(),
                };
            }

            set
            {
                switch (index)
                {
                    case 0:
                        A = value; break;

                    case 1:
                        B = value; break;

                    case 2:
                        C = value; break;

                    case 3:
                        D = value; break;

                    case 4:
                        E = value; break;

                    case 5:
                        F = value; break;

                    case 6:
                        G = value; break;

                    case 7:
                        I = value; break;

                    case 8:
                        H = value; break;

                    case 9:
                        Z = value; break;

                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }
    }
}