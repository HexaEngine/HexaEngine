namespace HexaEngine.Core.Effects.Noise
{
    public enum NoiseType
    {
        /// <summary>
        /// Blue noise 2D
        /// </summary>
        Blue2D,

        /// <summary>
        /// Cellular noise 2D, returning F1 and F2 in a float2.
        /// Standard 3x3 search window for good F1 and F2 values
        /// </summary>
        Cellular2D,

        /// <summary>
        /// Cellular noise 2D, returning F1 and F2 in a float2.
        /// Speeded up by using 2x2 search window instead of 3x3,
        /// at the expense of some strong pattern artifacts.
        /// F2 is often wrong and has sharp discontinuities.
        /// If you need a smooth F2, use the slower 3x3 version.
        /// F1 is sometimes wrong, too, but OK for most purposes.
        /// </summary>
        Cellular2x2,

        /// <summary>
        /// Cellular noise 3D, returning F1 and F2 in a float2.
        /// Speeded up by using 2x2x2 search window instead of 3x3x3,
        /// at the expense of some pattern artifacts.
        /// F2 is often wrong and has sharp discontinuities.
        /// If you need a good F2, use the slower 3x3x3 version.
        /// </summary>
        Cellular2x2x2,

        /// <summary>
        /// Cellular noise 3D, returning F1 and F2 in a float2.
        /// 3x3x3 search region for good F2 everywhere, but a lot
        /// slower than the 2x2x2 version.
        /// The code below is a bit scary even to its author,
        /// but it has at least half decent performance on a
        /// modern GPU. In any case, it beats any software
        /// implementation of Worley noise hands down.
        /// </summary>
        Cellular3D,

        /// <summary>
        /// White noise 1D
        /// </summary>
        Hash1D,

        /// <summary>
        /// White noise 2D
        /// </summary>
        Hash2D,

        /// <summary>
        /// White noise 3D
        /// </summary>
        Hash3D,

        /// <summary>
        /// Classic Perlin noise 2D
        /// </summary>
        Perlin2D,

        /// <summary>
        /// Classic Perlin noise 3D
        /// </summary>
        Perlin3D,

        /// <summary>
        /// Classic Perlin noise 4D
        /// </summary>
        Perlin4D,

        /// <summary>
        /// Simplex noise 2D
        /// </summary>
        Simplex2D,

        /// <summary>
        /// Simplex noise 3D
        /// </summary>
        Simplex3D,

        /// <summary>
        /// Simplex noise 4D
        /// </summary>
        Simplex4D,

        /// <summary>
        /// 2-D tiling simplex noise with rotating gradients and analytical derivative.
        /// The first component of the 3-element return vector is the noise value,
        /// and the second and third components are the x and y partial derivatives.
        /// </summary>
        Simplex2DPSRD,

        /// <summary>
        /// 2-D tiling simplex noise with rotating gradients,
        /// but without the analytical derivative.
        /// </summary>
        Simplex2DPSD,

        /// <summary>
        /// 2-D non-tiling simplex noise with rotating gradients and analytical derivative.
        /// The first component of the 3-element return vector is the noise value,
        /// and the second and third components are the x and y partial derivatives.
        /// </summary>
        Simplex2DSRD,

        /// <summary>
        /// 2-D non-tiling simplex noise with rotating gradients,
        /// without the analytical derivative.
        /// </summary>
        Simplex2DSR,
    }
}