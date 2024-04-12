namespace HexaEngine.Mathematics.Noise
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// A class for generating Perlin noise.
    /// </summary>
    public class PerlinNoise
    {
        private readonly int repeat;

        private static readonly byte[] permutation = { 151,160,137,91,90,15,                 // Hash lookup table as defined by Ken Perlin.  This is a randomly
    131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,    // arranged array of all numbers from 0-255 inclusive.
    190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
    88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
    77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
    102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
    135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
    5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
    223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
    129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
    251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
    49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
    138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
};

        private readonly byte[] p;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerlinNoise"/> class.
        /// </summary>
        public PerlinNoise() : this(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerlinNoise"/> class with a seed value.
        /// </summary>
        /// <param name="seed">The seed value for initializing the noise generator. Use 0 for the default permutation.</param>
        public PerlinNoise(int seed)
        {
            repeat = -1;
            if (seed == 0)
            {
                p = new byte[256];
                for (int x = 0; x < 256; x++)
                {
                    p[x] = permutation[x];
                }
            }
            else
            {
                p = new byte[permutation.Length];
                Random random = new(seed);
                random.NextBytes(p);
            }
        }

        private static float Fade(float x)
        {
            return x * x * (3 - 2 * x);
        }

        private static float Lerp(float a, float b, float v)
        {
            return (b - a) * v + a;
        }

        private static float Saturate(float value)
        {
            return (float)((value + 1.0) / 2.0);
        }

        private static float Clamp01(float value)
        {
            return Math.Clamp(value, 0.0f, 1.0f);
        }

        private static float SaturateClamp01(float value)
        {
            value = Clamp01(value);
            return Saturate(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Inc(int num)
        {
            num++;
            if (repeat > 0)
            {
                num %= repeat;
            }

            return num;
        }

        /// <summary>
        /// Computes the 1D gradient value at a given hash and position.
        /// </summary>
        /// <param name="hash">A hash value used for gradient calculation.</param>
        /// <param name="x">The position on the x-axis.</param>
        /// <returns>The 1D gradient value.</returns>
        public static float Grad(int hash, float x)
        {
            var h = hash & 15;
            var grad = 1.0f + (h & 7);   // Gradient value 1.0, 2.0, ..., 8.0
            if ((h & 8) != 0)
            {
                grad = -grad;         // Set a random sign for the gradient
            }

            return grad * x;           // Multiply the gradient with the distance
        }

        /// <summary>
        /// Computes the 2D gradient value at a given hash and position.
        /// </summary>
        /// <param name="hash">A hash value used for gradient calculation.</param>
        /// <param name="x">The position on the x-axis.</param>
        /// <param name="y">The position on the y-axis.</param>
        /// <returns>The 2D gradient value.</returns>
        public static float Grad(int hash, float x, float y)
        {
            var h = hash & 7;      // Convert low 3 bits of hash code
            var u = h < 4 ? x : y;  // into 8 simple gradient directions,
            var v = h < 4 ? y : x;  // and compute the dot product with (x,y).
            return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -2.0f * v : 2.0f * v);
        }

        /// <summary>
        /// Computes the 3D gradient value at a given hash and position.
        /// </summary>
        /// <param name="hash">A hash value used for gradient calculation.</param>
        /// <param name="x">The position on the x-axis.</param>
        /// <param name="y">The position on the y-axis.</param>
        /// <param name="z">The position on the z-axis.</param>
        /// <returns>The 3D gradient value.</returns>
        public static float Grad(int hash, float x, float y, float z)
        {
            var h = hash & 15;     // Convert low 4 bits of hash code into 12 simple
            var u = h < 8 ? x : y; // gradient directions, and compute dot product.
            var v = h < 4 ? y : h == 12 || h == 14 ? x : z; // Fix repeats at h = 12 to 15
            return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -v : v);
        }

        /// <summary>
        /// 1D Perlin noise.
        /// </summary>
        /// <param name="x">The position on the x-axis for noise generation.</param>
        /// <returns>The Perlin noise value in the range [-1, 1].</returns>
        public float Noise(float x)
        {
            int xi = (int)x & 255;
            float xf = x - (int)x;
            float u = Fade(xf);

            int a, b;
            a = p[xi];
            b = p[Inc(xi)];

            return Lerp(Grad(a, xf), Grad(b, xf - 1), u);
        }

        /// <summary>
        /// 2D Perlin noise.
        /// </summary>
        /// <param name="x">The position on the x-axis for noise generation.</param>
        /// <param name="y">The position on the y-axis for noise generation.</param>
        /// <returns>The Perlin noise value in the range [-1, 1].</returns>
        public float Noise(float x, float y)
        {
            float _x = MathF.Floor(x);
            float _y = MathF.Floor(y);
            int ix = (int)_x & 255;
            int iy = (int)_y & 255;
            float xf = x - _x;
            float yf = y - _y;
            float u = Fade(xf);
            float v = Fade(yf);

            int aa, ab, ba, bb;

            int a = p[ix & 255] + iy & 255;
            int b = p[ix + 1 & 255] + iy & 255;

            aa = p[a] & 255;
            ab = p[a + 1 & 255] & 255;
            ba = p[b] & 255;
            bb = p[b + 1 & 255] & 255;

            float x1, x2;
            x1 = Lerp(Grad(aa, xf, yf),
                        Grad(ba, xf - 1, yf),
                        u);

            x2 = Lerp(Grad(ab, xf, yf - 1),
                        Grad(bb, xf - 1, yf - 1),
                          u);

            return Lerp(x1, x2, v);
        }

        /// <summary>
        /// 3D Perlin noise.
        /// </summary>
        /// <param name="x">The position on the x-axis for noise generation.</param>
        /// <param name="y">The position on the y-axis for noise generation.</param>
        /// <param name="z">The position on the z-axis for noise generation.</param>
        /// <returns>The Perlin noise value in the range [-1, 1].</returns>
        public float Noise(float x, float y, float z)
        {
            int xi = (int)x & 255;                              // Calculate the "unit cube" that the point asked will be located in
            int yi = (int)y & 255;                              // The left bound is ( |_x_|,|_y_|,|_z_| ) and the right bound is that
            int zi = (int)z & 255;                              // plus 1.  Next we calculate the location (from 0.0 to 1.0) in that cube.
            float xf = x - (int)x;                             // We also fade the location to smooth the result.
            float yf = y - (int)y;
            float zf = z - (int)z;
            float u = Fade(xf);
            float v = Fade(yf);
            float w = Fade(zf);

            int aaa, aba, aab, abb, baa, bba, bab, bbb;
            aaa = p[p[p[xi] + yi] + zi];
            aba = p[p[p[xi] + Inc(yi)] + zi];
            aab = p[p[p[xi] + yi] + Inc(zi)];
            abb = p[p[p[xi] + Inc(yi)] + Inc(zi)];
            baa = p[p[p[Inc(xi)] + yi] + zi];
            bba = p[p[p[Inc(xi)] + Inc(yi)] + zi];
            bab = p[p[p[Inc(xi)] + yi] + Inc(zi)];
            bbb = p[p[p[Inc(xi)] + Inc(yi)] + Inc(zi)];

            float x1, x2, y1, y2;
            x1 = Lerp(Grad(aaa, xf, yf, zf),                // The gradient function calculates the dot product between a pseudorandom
                        Grad(baa, xf - 1, yf, zf),              // gradient vector and the vector from the input coordinate to the 8
                        u);                                     // surrounding points in its unit cube.
            x2 = Lerp(Grad(aba, xf, yf - 1, zf),                // This is all then lerped together as a sort of weighted average based on the faded (u,v,w)
                        Grad(bba, xf - 1, yf - 1, zf),              // values we made earlier.
                          u);
            y1 = Lerp(x1, x2, v);

            x1 = Lerp(Grad(aab, xf, yf, zf - 1),
                        Grad(bab, xf - 1, yf, zf - 1),
                          u);
            x2 = Lerp(Grad(abb, xf, yf - 1, zf - 1),
                        Grad(bbb, xf - 1, yf - 1, zf - 1),
                          u);
            y2 = Lerp(x1, x2, v);

            return Lerp(y1, y2, w);
        }

        /// <summary>
        /// 1D Octave Perlin noise.
        /// </summary>
        /// <param name="x">The position on the x-axis for noise generation.</param>
        /// <param name="octaves">Number of octaves.</param>
        /// <param name="persistence">Persistence of the noise.</param>
        /// <param name="amplitude">Amplitude of the Noise.</param>
        /// <returns>The Octave Perlin noise value.</returns>
        public float OctaveNoise(float x, int octaves, float persistence, float amplitude)
        {
            float total = 0;
            float frequency = 1;

            for (int i = 0; i < octaves; i++)
            {
                total += SaturateClamp01(Noise(x * frequency)) * amplitude;

                amplitude *= persistence;
                frequency *= 2;
            }

            return total;
        }

        /// <summary>
        /// 2D Octave Perlin noise.
        /// </summary>
        /// <param name="x">The position on the x-axis for noise generation.</param>
        /// <param name="y">The position on the y-axis for noise generation.</param>
        /// <param name="octaves">Number of octaves.</param>
        /// <param name="persistence">Persistence of the noise.</param>
        /// <param name="amplitude">Amplitude of the Noise.</param>
        /// <returns>The Octave Perlin noise value.</returns>
        public float OctaveNoise(float x, float y, int octaves, float persistence, float amplitude)
        {
            float total = 0;
            float frequency = 1;
            for (int i = 0; i < octaves; i++)
            {
                total += Noise(x * frequency, y * frequency) * amplitude;

                amplitude *= persistence;
                frequency *= 2;
            }

            return total;
        }

        /// <summary>
        /// 3D Octave Perlin noise.
        /// </summary>
        /// <param name="x">The position on the x-axis for noise generation.</param>
        /// <param name="y">The position on the y-axis for noise generation.</param>
        /// <param name="z">The position on the z-axis for noise generation.</param>
        /// <param name="octaves">Number of octaves.</param>
        /// <param name="persistence">Persistence of the noise.</param>
        /// <param name="amplitude">Amplitude of the Noise.</param>
        /// <returns>The Octave Perlin noise value.</returns>
        public float OctaveNoise(float x, float y, float z, int octaves, float persistence, float amplitude)
        {
            float total = 0;
            float frequency = 1;
            for (int i = 0; i < octaves; i++)
            {
                total += SaturateClamp01(Noise(x * frequency, y * frequency, z * frequency)) * amplitude;

                amplitude *= persistence;
                frequency *= 2;
            }

            return total;
        }
    }
}