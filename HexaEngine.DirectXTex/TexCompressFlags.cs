namespace HexaEngine.DirectXTex
{
    [Flags]
    public enum TexCompressFlags
    {
        DEFAULT = 0,

        /// <summary>
        /// Enables dithering RGB colors for BC1-3 compression
        /// </summary>
        RGB_DITHER = 0x10000,

        /// <summary>
        /// Enables dithering alpha for BC1-3 compression
        /// </summary>
        A_DITHER = 0x20000,

        /// <summary>
        /// Enables both RGB and alpha dithering for BC1-3 compression
        /// </summary>
        DITHER = 0x30000,

        /// <summary>
        /// Uniform color weighting for BC1-3 compression; by default uses perceptual weighting
        /// </summary>
        UNIFORM = 0x40000,

        /// <summary>
        /// Enables exhaustive search for BC7 compress for mode 0 and 2; by default skips trying these modes
        /// </summary>
        BC7_USE_3SUBSETS = 0x80000,

        /// <summary>
        /// Minimal modes (usually mode 6) for BC7 compression
        /// </summary>
        BC7_QUICK = 0x100000,

        SRGB_IN = 0x1000000,
        SRGB_OUT = 0x2000000,

        /// <summary>
        /// if the input format type is IsSRGB(), then SRGB_IN is on by default
        /// if the output format type is IsSRGB(), then SRGB_OUT is on by default
        /// </summary>
        SRGB = SRGB_IN | SRGB_OUT,

        /// <summary>
        /// Compress is free to use multithreading to improve performance (by default it does not use multithreading)
        /// </summary>
        PARALLEL = 0x10000000
    }
}