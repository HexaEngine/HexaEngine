namespace HexaEngine.DirectXTex
{
    [Flags]
    public enum TexFilterFlags
    {
        DEFAULT = 0,

        /// <summary>
        /// Wrap vs. Mirror vs. Clamp filtering options
        /// </summary>
        WRAP_U = 0x1,

        /// <summary>
        /// Wrap vs. Mirror vs. Clamp filtering options
        /// </summary>
        WRAP_V = 0x2,

        /// <summary>
        /// Wrap vs. Mirror vs. Clamp filtering options
        /// </summary>
        WRAP_W = 0x4,

        /// <summary>
        /// Wrap vs. Mirror vs. Clamp filtering options
        /// </summary>
        WRAP = WRAP_U | WRAP_V | WRAP_W,

        MIRROR_U = 0x10,

        /// <summary>
        /// Wrap vs. Mirror vs. Clamp filtering options
        /// </summary>
        MIRROR_V = 0x20,

        /// <summary>
        /// Wrap vs. Mirror vs. Clamp filtering options
        /// </summary>
        MIRROR_W = 0x40,

        /// <summary>
        /// Wrap vs. Mirror vs. Clamp filtering options
        /// </summary>
        MIRROR = MIRROR_U | MIRROR_V | MIRROR_W,

        /// <summary>
        /// Resize color and alpha channel independently
        /// </summary>
        SEPARATE_ALPHA = 0x100,

        /// <summary>
        /// Enable *2 - 1 conversion cases for unorm to/from float and positive-only float formats
        /// </summary>
        FLOAT_X2BIAS = 0x200,

        /// <summary>
        /// When converting RGB to R, defaults to using grayscale. These flags indicate copying a specific channel instead
        /// When converting RGB to RG, defaults to copying RED | GREEN. These flags control which channels are selected instead.
        /// </summary>
        RGB_COPY_RED = 0x1000,

        /// <summary>
        /// When converting RGB to R, defaults to using grayscale. These flags indicate copying a specific channel instead
        /// When converting RGB to RG, defaults to copying RED | GREEN. These flags control which channels are selected instead.
        /// </summary>
        RGB_COPY_GREEN = 0x2000,

        /// <summary>
        /// When converting RGB to R, defaults to using grayscale. These flags indicate copying a specific channel instead
        /// When converting RGB to RG, defaults to copying RED | GREEN. These flags control which channels are selected instead.
        /// </summary>
        RGB_COPY_BLUE = 0x4000,

        /// <summary>
        /// Use ordered 4x4 dithering for any required conversions
        /// </summary>
        DITHER = 0x10000,

        /// <summary>
        /// Use error-diffusion dithering for any required conversions
        /// </summary>
        DITHER_DIFFUSION = 0x20000,

        /// <summary>
        /// Filtering mode to use for any required image resizing
        /// </summary>
        POINT = 0x100000,

        /// <summary>
        /// Filtering mode to use for any required image resizing
        /// </summary>
        LINEAR = 0x200000,

        /// <summary>
        /// Filtering mode to use for any required image resizing
        /// </summary>
        CUBIC = 0x300000,

        /// <summary>
        /// Filtering mode to use for any required image resizing
        /// </summary>
        BOX = 0x400000,

        /// <summary>
        /// Filtering mode to use for any required image resizing
        /// Equiv to Box filtering for mipmap generation
        /// </summary>

        FANT = 0x400000,

        /// <summary>
        /// Filtering mode to use for any required image resizing
        /// </summary>
        TRIANGLE = 0x500000,

        SRGB_IN = 0x1000000,
        SRGB_OUT = 0x2000000,

        /// <summary>
        /// sRGB to/from RGB for use in conversion operations
        /// if the input format type is IsSRGB(), then SRGB_IN is on by default
        /// if the output format type is IsSRGB(), then SRGB_OUT is on by default
        /// </summary>
        SRGB = SRGB_IN | SRGB_OUT,

        /// <summary>
        /// Forces use of the non-WIC path when both are an option
        /// </summary>
        FORCE_NON_WIC = 0x10000000,

        /// <summary>
        /// Forces use of the WIC path even when logic would have picked a non-WIC path when both are an option
        /// </summary>
        FORCE_WIC = 0x20000000
    }
}