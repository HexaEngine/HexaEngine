namespace HexaEngine.Mathematics.Noise
{
    /// <summary>
    /// Represents a generic noise generation class using a custom algorithm.
    /// </summary>
    public class GenericNoise
    {
        private readonly double coefficient0;
        private readonly double coefficient1;
        private readonly double coefficient2;
        private readonly double coefficient3;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericNoise"/> class with default coefficients.
        /// </summary>
        public GenericNoise()
        {
            coefficient0 = 43758.5453123;
            coefficient1 = 12.9898;
            coefficient2 = 78.233;
            coefficient3 = 1.0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericNoise"/> class with custom coefficients based on a seed value.
        /// </summary>
        /// <param name="seed">The seed value used to generate custom coefficients.</param>
        public GenericNoise(int seed)
        {
            float factor = (float)seed / int.MaxValue;
            coefficient0 = 43758.5453123 * factor;
            coefficient1 = 12.9898 * factor;
            coefficient2 = 78.233 * factor;
            coefficient3 = 1.0 * factor;
        }

        private static double Frac(double v)
        {
            return v - double.Truncate(v);
        }

        private static double Dot(double x1, double y1, double x2, double y2)
        {
            return x1 * x2 + y1 * y2;
        }

        private static double Dot(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            return x1 * x2 + y1 * y2 + z1 * z2;
        }

        /// <summary>
        /// Calculates 1D noise value at a given position.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <returns>The 1D noise value in the range [0, 1).</returns>
        public double Noise(double x)
        {
            return Frac(Math.Sin(x) * coefficient0);
        }

        /// <summary>
        /// Calculates 2D noise value at a given position.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <returns>The 2D noise value in the range [0, 1).</returns>
        public double Noise(double x, double y)
        {
            return Frac(Math.Sin(Dot(x, y, coefficient1, coefficient2)) * coefficient0);
        }

        /// <summary>
        /// Calculates 3D noise value at a given position.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        /// <returns>The 3D noise value in the range [0, 1).</returns>
        public double Noise(double x, double y, double z)
        {
            return Frac(Math.Sin(Dot(x, y, z, coefficient1, coefficient2, coefficient3)) * coefficient0);
        }
    }
}