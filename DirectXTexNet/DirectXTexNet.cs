namespace DirectXTexNet
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using ID3D11DeviceContextPtr = System.IntPtr;

    using ID3D11DevicePtr = System.IntPtr;
    using ID3D11ResourcePtr = System.IntPtr;
    using ID3D11ShaderResourceViewPtr = System.IntPtr;
    // just some aliases for better readability
    using IWICImagingFactoryPtr = System.IntPtr;

    // The unmanaged size_t is UInt32 in 32bit processes and UInt64 in 64bit processes
    // For simplicity and CLS compliancy we generally use Int32, but where it matters Int64
    using Size_t = System.Int32;
    using Size_T = System.Int64;

    using XMVectorPtr = System.IntPtr;

#pragma warning disable CA1069 // Enumerationswerte dürfen nicht dupliziert werden

    #region enums

    public enum DXGI_FORMAT
    {
        UNKNOWN = 0,
        R32G32B32A32_TYPELESS = 1,
        R32G32B32A32_FLOAT = 2,
        R32G32B32A32_UINT = 3,
        R32G32B32A32_SINT = 4,
        R32G32B32_TYPELESS = 5,
        R32G32B32_FLOAT = 6,
        R32G32B32_UINT = 7,
        R32G32B32_SINT = 8,
        R16G16B16A16_TYPELESS = 9,
        R16G16B16A16_FLOAT = 10,
        R16G16B16A16_UNORM = 11,
        R16G16B16A16_UINT = 12,
        R16G16B16A16_SNORM = 13,
        R16G16B16A16_SINT = 14,
        R32G32_TYPELESS = 15,
        R32G32_FLOAT = 16,
        R32G32_UINT = 17,
        R32G32_SINT = 18,
        R32G8X24_TYPELESS = 19,
        D32_FLOAT_S8X24_UINT = 20,
        R32_FLOAT_X8X24_TYPELESS = 21,
        X32_TYPELESS_G8X24_UINT = 22,
        R10G10B10A2_TYPELESS = 23,
        R10G10B10A2_UNORM = 24,
        R10G10B10A2_UINT = 25,
        R11G11B10_FLOAT = 26,
        R8G8B8A8_TYPELESS = 27,
        R8G8B8A8_UNORM = 28,
        R8G8B8A8_UNORM_SRGB = 29,
        R8G8B8A8_UINT = 30,
        R8G8B8A8_SNORM = 31,
        R8G8B8A8_SINT = 32,
        R16G16_TYPELESS = 33,
        R16G16_FLOAT = 34,
        R16G16_UNORM = 35,
        R16G16_UINT = 36,
        R16G16_SNORM = 37,
        R16G16_SINT = 38,
        R32_TYPELESS = 39,
        D32_FLOAT = 40,
        R32_FLOAT = 41,
        R32_UINT = 42,
        R32_SINT = 43,
        R24G8_TYPELESS = 44,
        D24_UNORM_S8_UINT = 45,
        R24_UNORM_X8_TYPELESS = 46,
        X24_TYPELESS_G8_UINT = 47,
        R8G8_TYPELESS = 48,
        R8G8_UNORM = 49,
        R8G8_UINT = 50,
        R8G8_SNORM = 51,
        R8G8_SINT = 52,
        R16_TYPELESS = 53,
        R16_FLOAT = 54,
        D16_UNORM = 55,
        R16_UNORM = 56,
        R16_UINT = 57,
        R16_SNORM = 58,
        R16_SINT = 59,
        R8_TYPELESS = 60,
        R8_UNORM = 61,
        R8_UINT = 62,
        R8_SNORM = 63,
        R8_SINT = 64,
        A8_UNORM = 65,
        R1_UNORM = 66,
        R9G9B9E5_SHAREDEXP = 67,
        R8G8_B8G8_UNORM = 68,
        G8R8_G8B8_UNORM = 69,
        BC1_TYPELESS = 70,
        BC1_UNORM = 71,
        BC1_UNORM_SRGB = 72,
        BC2_TYPELESS = 73,
        BC2_UNORM = 74,
        BC2_UNORM_SRGB = 75,
        BC3_TYPELESS = 76,
        BC3_UNORM = 77,
        BC3_UNORM_SRGB = 78,
        BC4_TYPELESS = 79,
        BC4_UNORM = 80,
        BC4_SNORM = 81,
        BC5_TYPELESS = 82,
        BC5_UNORM = 83,
        BC5_SNORM = 84,
        B5G6R5_UNORM = 85,
        B5G5R5A1_UNORM = 86,
        B8G8R8A8_UNORM = 87,
        B8G8R8X8_UNORM = 88,
        R10G10B10_XR_BIAS_A2_UNORM = 89,
        B8G8R8A8_TYPELESS = 90,
        B8G8R8A8_UNORM_SRGB = 91,
        B8G8R8X8_TYPELESS = 92,
        B8G8R8X8_UNORM_SRGB = 93,
        BC6H_TYPELESS = 94,
        BC6H_UF16 = 95,
        BC6H_SF16 = 96,
        BC7_TYPELESS = 97,
        BC7_UNORM = 98,
        BC7_UNORM_SRGB = 99,
        AYUV = 100,
        Y410 = 101,
        Y416 = 102,
        NV12 = 103,
        P010 = 104,
        P016 = 105,
        OPAQUE_420 = 106,
        YUY2 = 107,
        Y210 = 108,
        Y216 = 109,
        NV11 = 110,
        AI44 = 111,
        IA44 = 112,
        P8 = 113,
        A8P8 = 114,
        B4G4R4A4_UNORM = 115,

        P208 = 130,
        V208 = 131,
        V408 = 132,
        //FORCE_UINT = 0xffffffff
    }

    public enum D3D11_USAGE
    {
        DEFAULT = 0,
        IMMUTABLE = 1,
        DYNAMIC = 2,
        STAGING = 3
    }

    [Flags]
    public enum D3D11_BIND_FLAG
    {
        VERTEX_BUFFER = 0x1,
        INDEX_BUFFER = 0x2,
        CONSTANT_BUFFER = 0x4,
        SHADER_RESOURCE = 0x8,
        STREAM_OUTPUT = 0x10,
        RENDER_TARGET = 0x20,
        DEPTH_STENCIL = 0x40,
        UNORDERED_ACCESS = 0x80,
        DECODER = 0x200,
        VIDEO_ENCODER = 0x400
    }

    [Flags]
    public enum D3D11_CPU_ACCESS_FLAG
    {
        WRITE = 0x10000,
        READ = 0x20000
    }

    [Flags]
    public enum D3D11_RESOURCE_MISC_FLAG
    {
        GENERATE_MIPS = 0x1,
        SHARED = 0x2,
        TEXTURECUBE = 0x4,
        DRAWINDIRECT_ARGS = 0x10,
        BUFFER_ALLOW_RAW_VIEWS = 0x20,
        BUFFER_STRUCTURED = 0x40,
        RESOURCE_CLAMP = 0x80,
        SHARED_KEYEDMUTEX = 0x100,
        GDI_COMPATIBLE = 0x200,
        SHARED_NTHANDLE = 0x800,
        RESTRICTED_CONTENT = 0x1000,
        RESTRICT_SHARED_RESOURCE = 0x2000,
        RESTRICT_SHARED_RESOURCE_DRIVER = 0x4000,
        GUARDED = 0x8000,
        TILE_POOL = 0x20000,
        TILED = 0x40000,
        HW_PROTECTED = 0x80000
    }

    [Flags]
    public enum CP_FLAGS
    {
        /// <summary>
        /// Normal operation
        /// </summary>
        NONE = 0x0,

        /// <summary>
        /// Assume pitch is DWORD aligned instead of BYTE aligned
        /// </summary>
        LEGACY_DWORD = 0x1,

        /// <summary>
        /// Assume pitch is 16-byte aligned instead of BYTE aligned
        /// </summary>
        PARAGRAPH = 0x2,

        /// <summary>
        /// Assume pitch is 32-byte aligned instead of BYTE aligned
        /// </summary>
        YMM = 0x4,

        /// <summary>
        /// The ZMM
        /// </summary>
        ZMM = 0x8,

        /// <summary>
        /// Assume pitch is 4096-byte aligned instead of BYTE aligned
        /// </summary>
        PAGE4K = 0x200,

        /// <summary>
        /// BC formats with malformed mipchain blocks smaller than 4x4
        /// </summary>
        BAD_DXTN_TAILS = 0x1000,

        /// <summary>
        /// Override with a legacy 24 bits-per-pixel format size
        /// </summary>
        BPP24 = 0x10000,

        /// <summary>
        /// Override with a legacy 16 bits-per-pixel format size
        /// </summary>
        BPP16 = 0x20000,

        /// <summary>
        /// Override with a legacy 8 bits-per-pixel format size
        /// </summary>
        BPP8 = 0x40000,
    }

    public enum TEX_DIMENSION
    {
        TEXTURE1D = 2,
        TEXTURE2D = 3,
        TEXTURE3D = 4,
    }

    /// <summary>
    /// Subset here matches D3D10_RESOURCE_MISC_FLAG and D3D11_RESOURCE_MISC_FLAG
    /// </summary>
    [Flags]
    public enum TEX_MISC_FLAG
    {
        TEXTURECUBE = 0x4,
    }

    [Flags]
    public enum TEX_MISC_FLAG2
    {
        ALPHA_MODE_MASK = 0x7,
    }

    /// <summary>
    /// Matches DDS_ALPHA_MODE, encoded in MISC_FLAGS2
    /// </summary>
    public enum TEX_ALPHA_MODE
    {
        UNKNOWN = 0,
        STRAIGHT = 1,
        PREMULTIPLIED = 2,
        OPAQUE = 3,
        CUSTOM = 4,
    }

    [Flags]
    public enum DDS_FLAGS
    {
        NONE = 0x0,

        /// <summary>
        /// Assume pitch is DWORD aligned instead of BYTE aligned (used by some legacy DDS files)
        /// </summary>
        LEGACY_DWORD = 0x1,

        /// <summary>
        /// Do not implicitly convert legacy formats that result in larger pixel sizes (24 bpp, 3:3:2, A8L8, A4L4, P8, A8P8)
        /// </summary>
        NO_LEGACY_EXPANSION = 0x2,

        /// <summary>
        /// Do not use work-around for long-standing D3DX DDS file format issue which reversed the 10:10:10:2 color order masks
        /// </summary>
        NO_R10B10G10A2_FIXUP = 0x4,

        /// <summary>
        /// Convert DXGI 1.1 BGR formats to DXGI_FORMAT_R8G8B8A8_UNORM to avoid use of optional WDDM 1.1 formats
        /// </summary>
        FORCE_RGB = 0x8,

        /// <summary>
        /// Conversions avoid use of 565, 5551, and 4444 formats and instead expand to 8888 to avoid use of optional WDDM 1.2 formats
        /// </summary>
        NO_16BPP = 0x10,

        /// <summary>
        /// When loading legacy luminance formats expand replicating the color channels rather than leaving them packed (L8, L16, A8L8)
        /// </summary>
        EXPAND_LUMINANCE = 0x20,

        /// <summary>
        /// Some older DXTn DDS files incorrectly handle mipchain tails for blocks smaller than 4x4
        /// </summary>
        BAD_DXTN_TAILS = 0x40,

        /// <summary>
        /// Always use the 'DX10' header extension for DDS writer (i.e. don't try to write DX9 compatible DDS files)
        /// </summary>
        FORCE_DX10_EXT = 0x10000,

        /// <summary>
        /// FORCE_DX10_EXT including miscFlags2 information (result may not be compatible with D3DX10 or D3DX11)
        /// </summary>
        FORCE_DX10_EXT_MISC2 = 0x20000,

        /// <summary>
        /// Force use of legacy header for DDS writer (will fail if unable to write as such)
        /// </summary>
        FORCE_DX9_LEGACY = 0x40000,

        /// <summary>
        /// Enables the loader to read large dimension .dds files (i.e. greater than known hardware requirements)
        /// </summary>
        ALLOW_LARGE_FILES = 0x1000000,
    }

    [Flags]
    public enum WIC_FLAGS
    {
        NONE = 0x0,

        /// <summary>
        /// Loads DXGI 1.1 BGR formats as DXGI_FORMAT_R8G8B8A8_UNORM to avoid use of optional WDDM 1.1 formats
        /// </summary>
        FORCE_RGB = 0x1,

        /// <summary>
        /// Loads DXGI 1.1 X2 10:10:10:2 format as DXGI_FORMAT_R10G10B10A2_UNORM
        /// </summary>
        NO_X2_BIAS = 0x2,

        /// <summary>
        /// Loads 565, 5551, and 4444 formats as 8888 to avoid use of optional WDDM 1.2 formats
        /// </summary>
        NO_16BPP = 0x4,

        /// <summary>
        /// Loads 1-bit monochrome (black and white) as R1_UNORM rather than 8-bit grayscale
        /// </summary>
        ALLOW_MONO = 0x8,

        /// <summary>
        /// Loads all images in a multi-frame file, converting/resizing to match the first frame as needed, defaults to 0th frame otherwise
        /// </summary>
        ALL_FRAMES = 0x10,

        /// <summary>
        /// Ignores sRGB metadata if present in the file
        /// </summary>
        IGNORE_SRGB = 0x20,

        /// <summary>
        /// Writes sRGB metadata into the file reguardless of format
        /// </summary>
        FORCE_SRGB = 0x40,

        /// <summary>
        /// Writes linear gamma metadata into the file reguardless of format
        /// </summary>
        FORCE_LINEAR = 0x80,

        /// <summary>
        /// If no colorspace is specified, assume sRGB
        /// </summary>
        DEFAULT_SRGB = 0x100,

        /// <summary>
        /// Use ordered 4x4 dithering for any required conversions
        /// </summary>
        DITHER = 0x10000,

        /// <summary>
        /// Use error-diffusion dithering for any required conversions
        /// </summary>
        DITHER_DIFFUSION = 0x20000,

        /// <summary>
        /// Filtering mode to use for any required image resizing (only needed when loading arrays of differently sized images; defaults to Fant)
        /// </summary>
        FILTER_POINT = 0x100000,

        /// <summary>
        /// Filtering mode to use for any required image resizing (only needed when loading arrays of differently sized images; defaults to Fant)
        /// </summary>
        FILTER_LINEAR = 0x200000,

        /// <summary>
        /// Filtering mode to use for any required image resizing (only needed when loading arrays of differently sized images; defaults to Fant)
        /// </summary>
        FILTER_CUBIC = 0x300000,

        /// <summary>
        /// Filtering mode to use for any required image resizing (only needed when loading arrays of differently sized images; defaults to Fant)
        /// Combination of Linear and Box filter
        /// </summary>
        FILTER_FANT = 0x400000
    }

    [Flags]
    public enum TEX_FR_FLAGS
    {
        ROTATE0 = 0x0,
        ROTATE90 = 0x1,
        ROTATE180 = 0x2,
        ROTATE270 = 0x3,
        FLIP_HORIZONTAL = 0x08,
        FLIP_VERTICAL = 0x10,
    }

    [Flags]
    public enum TEX_FILTER_FLAGS
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
        WRAP = (WRAP_U | WRAP_V | WRAP_W),

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
        MIRROR = (MIRROR_U | MIRROR_V | MIRROR_W),

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
        SRGB = (SRGB_IN | SRGB_OUT),

        /// <summary>
        /// Forces use of the non-WIC path when both are an option
        /// </summary>
        FORCE_NON_WIC = 0x10000000,

        /// <summary>
        /// Forces use of the WIC path even when logic would have picked a non-WIC path when both are an option
        /// </summary>
        FORCE_WIC = 0x20000000
    }

    [Flags]
    public enum TEX_PMALPHA_FLAGS
    {
        DEFAULT = 0,

        /// <summary>
        /// ignores sRGB colorspace conversions
        /// </summary>
        IGNORE_SRGB = 0x1,

        /// <summary>
        /// converts from premultiplied alpha back to straight alpha
        /// </summary>
        REVERSE = 0x2,

        SRGB_IN = 0x1000000,
        SRGB_OUT = 0x2000000,

        /// <summary>
        /// if the input format type is IsSRGB(), then SRGB_IN is on by default
        /// if the output format type is IsSRGB(), then SRGB_OUT is on by default
        /// </summary>
        SRGB = (SRGB_IN | SRGB_OUT)
    }

    [Flags]
    public enum TEX_COMPRESS_FLAGS
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
        SRGB = (SRGB_IN | SRGB_OUT),

        /// <summary>
        /// Compress is free to use multithreading to improve performance (by default it does not use multithreading)
        /// </summary>
        PARALLEL = 0x10000000
    }

    [Flags]
    public enum CNMAP_FLAGS
    {
        DEFAULT = 0,

        /// <summary>
        /// Channel selection when evaluting color value for height
        /// </summary>
        CHANNEL_RED = 0x1,

        /// <summary>
        /// Channel selection when evaluting color value for height
        /// </summary>
        CHANNEL_GREEN = 0x2,

        /// <summary>
        /// Channel selection when evaluting color value for height
        /// </summary>
        CHANNEL_BLUE = 0x3,

        /// <summary>
        /// Channel selection when evaluting color value for height
        /// </summary>
        CHANNEL_ALPHA = 0x4,

        /// <summary>
        /// Channel selection when evaluting color value for height
        /// Luminance is a combination of red, green, and blue
        /// </summary>
        CHANNEL_LUMINANCE = 0x5,

        /// <summary>
        /// Use mirror semantics for scanline references (defaults to wrap)
        /// </summary>
        MIRROR_U = 0x1000,

        /// <summary>
        /// Use mirror semantics for scanline references (defaults to wrap)
        /// </summary>
        MIRROR_V = 0x2000,

        /// <summary>
        /// Use mirror semantics for scanline references (defaults to wrap)
        /// </summary>
        MIRROR = 0x3000,

        /// <summary>
        /// Inverts normal sign
        /// </summary>
        INVERT_SIGN = 0x4000,

        /// <summary>
        /// Computes a crude occlusion term stored in the alpha channel
        /// </summary>
        COMPUTE_OCCLUSION = 0x8000
    }

    [Flags]
    public enum CMSE_FLAGS
    {
        DEFAULT = 0,

        /// <summary>
        /// Indicates that image needs gamma correction before comparision
        /// </summary>
        IMAGE1_SRGB = 0x1,

        /// <summary>
        /// Indicates that image needs gamma correction before comparision
        /// </summary>
        IMAGE2_SRGB = 0x2,

        /// <summary>
        /// Ignore the channel when computing MSE
        /// </summary>
        IGNORE_RED = 0x10,

        /// <summary>
        /// Ignore the channel when computing MSE
        /// </summary>
        IGNORE_GREEN = 0x20,

        /// <summary>
        /// Ignore the channel when computing MSE
        /// </summary>
        IGNORE_BLUE = 0x40,

        /// <summary>
        /// Ignore the channel when computing MSE
        /// </summary>
        IGNORE_ALPHA = 0x80,

        /// <summary>
        /// Indicates that image should be scaled and biased before comparison (i.e. UNORM -> SNORM)
        /// </summary>
        IMAGE1_X2_BIAS = 0x100,

        /// <summary>
        /// Indicates that image should be scaled and biased before comparison (i.e. UNORM -> SNORM)
        /// </summary>
        IMAGE2_X2_BIAS = 0x200,
    };

    public enum WICCodecs
    {
        /// <summary>
        /// Windows Bitmap (.bmp)
        /// </summary>
        BMP = 1,

        /// <summary>
        /// Joint Photographic Experts Group (.jpg, .jpeg)
        /// </summary>
        JPEG,

        /// <summary>
        /// Portable Network Graphics (.png)
        /// </summary>
        PNG,

        /// <summary>
        /// Tagged Image File Format  (.tif, .tiff)
        /// </summary>
        TIFF,

        /// <summary>
        /// Graphics Interchange Format  (.gif)
        /// </summary>
        GIF,

        /// <summary>
        /// Windows Media Photo / HD Photo / JPEG XR (.hdp, .jxr, .wdp)
        /// </summary>
        WMP,

        /// <summary>
        /// Windows Icon (.ico)
        /// </summary>
        ICO
    }

    #endregion enums

    #region Delegates

    /// <summary>
    /// The delegate used for the EvaluateImage method.
    /// </summary>
    /// <param name="pixels">The pixels. This a row of Pixels with each pixel normally represented as RBGA in 4x32bit float (0.0-1.0).</param>
    /// <param name="width">The width.</param>
    /// <param name="y">The y/row index.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void EvaluatePixelsDelegate(XMVectorPtr pixels, IntPtr width, IntPtr y);

    /// <summary>
    /// The delegate used for the EvaluateImage method.
    /// </summary>
    /// <param name="outPixels">
    /// The out pixels to write to. This a row of Pixels with each pixel normally represented as RBGA in 4x32bit float
    /// (0.0-1.0).
    /// </param>
    /// <param name="inPixels">The input pixels. This a row of Pixels with each pixel normally represented as RBGA in 4x32bit float (0.0-1.0).</param>
    /// <param name="width">The width.</param>
    /// <param name="y">The y/row index.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void TransformPixelsDelegate(XMVectorPtr outPixels, XMVectorPtr inPixels, IntPtr width, IntPtr y);

    #endregion Delegates

    #region structs

    public struct MseV
    {
        public float V0, V1, V2, V3;
    }

    #endregion structs

    /// <summary>
    /// This is an immutable class representing the native Image struct.
    /// It also keeps a reference to a parent to prevent finalizing of the parent when the image is still used.
    /// But it's still strongly encouraged to manually dispose ScratchImages.
    /// </summary>
    public sealed class Image
    {
        public Size_t Width { get; }

        public Size_t Height { get; }

        public DXGI_FORMAT Format { get; }

        public Size_T RowPitch { get; }

        public Size_T SlicePitch { get; }

        public IntPtr Pixels { get; }

        public object Parent { get; }

        public Image(Size_t width, Size_t height, DXGI_FORMAT format, Size_T rowPitch, Size_T slicePitch, IntPtr pixels, object parent)
        {
            Width = width;
            Height = height;
            Format = format;
            RowPitch = rowPitch;
            SlicePitch = slicePitch;
            Pixels = pixels;
            Parent = parent;
        }
    }

    /// <summary>
    /// This class represents the native TexMetadata struct. A managed class is used to simplify passing it by reference.
    /// </summary>
    public sealed class TexMetadata
    {
        public Size_t Width;

        /// <summary>
        /// The height. Should be 1 for 1D textures.
        /// </summary>
        public Size_t Height;

        /// <summary>
        /// The depth. Should be 1 for 1D or 2D textures.
        /// </summary>
        public Size_t Depth;

        /// <summary>
        /// The array size. For cubemap, this is a multiple of 6.
        /// </summary>
        public Size_t ArraySize;

        public Size_t MipLevels;
        public TEX_MISC_FLAG MiscFlags;
        public TEX_MISC_FLAG2 MiscFlags2;
        public DXGI_FORMAT Format;
        public TEX_DIMENSION Dimension;

        public TexMetadata(
            Size_t width,
            Size_t height,
            Size_t depth,
            Size_t arraySize,
            Size_t mipLevels,
            TEX_MISC_FLAG miscFlags,
            TEX_MISC_FLAG2 miscFlags2,
            DXGI_FORMAT format,
            TEX_DIMENSION dimension)
        {
            Width = width;
            Height = height;
            Depth = depth;
            ArraySize = arraySize;
            MipLevels = mipLevels;
            MiscFlags = miscFlags;
            MiscFlags2 = miscFlags2;
            Format = format;
            Dimension = dimension;
        }

        public bool IsCubemap()
        {
            return (MiscFlags & TEX_MISC_FLAG.TEXTURECUBE) != 0;
        }

        public bool IsPMAlpha()
        {
            return (uint)(MiscFlags2 & TEX_MISC_FLAG2.ALPHA_MODE_MASK) == (uint)TEX_ALPHA_MODE.PREMULTIPLIED;
        }

        public void SetAlphaMode(TEX_ALPHA_MODE mode)
        {
            MiscFlags2 = (TEX_MISC_FLAG2)(((uint)MiscFlags2 & ~(uint)TEX_MISC_FLAG2.ALPHA_MODE_MASK) | (uint)mode);
        }

        public TEX_ALPHA_MODE GetAlphaMode()
        {
            return (TEX_ALPHA_MODE)(MiscFlags2 & TEX_MISC_FLAG2.ALPHA_MODE_MASK);
        }

        public bool IsVolumemap()
        {
            return Dimension == TEX_DIMENSION.TEXTURE3D;
        }

        public bool IsCompressed()
        {
            return
                Format == DXGI_FORMAT.BC1_TYPELESS ||
                Format == DXGI_FORMAT.BC1_UNORM ||
                Format == DXGI_FORMAT.BC1_UNORM_SRGB ||
                Format == DXGI_FORMAT.BC2_TYPELESS ||
                Format == DXGI_FORMAT.BC2_UNORM ||
                Format == DXGI_FORMAT.BC2_UNORM_SRGB ||
                Format == DXGI_FORMAT.BC3_TYPELESS ||
                Format == DXGI_FORMAT.BC3_UNORM ||
                Format == DXGI_FORMAT.BC3_UNORM_SRGB ||
                Format == DXGI_FORMAT.BC4_SNORM ||
                Format == DXGI_FORMAT.BC4_TYPELESS ||
                Format == DXGI_FORMAT.BC4_UNORM ||
                Format == DXGI_FORMAT.BC5_SNORM ||
                Format == DXGI_FORMAT.BC5_TYPELESS ||
                Format == DXGI_FORMAT.BC5_UNORM ||
                Format == DXGI_FORMAT.BC6H_SF16 ||
                Format == DXGI_FORMAT.BC6H_TYPELESS ||
                Format == DXGI_FORMAT.BC6H_UF16 ||
                Format == DXGI_FORMAT.BC7_TYPELESS ||
                Format == DXGI_FORMAT.BC7_UNORM ||
                Format == DXGI_FORMAT.BC7_UNORM_SRGB;
        }
    }

    public abstract class ScratchImage : IDisposable
    {
        //internal ScratchImage() { }

        public abstract bool IsDisposed { get; }

        public abstract Size_t GetImageCount();

        /// <summary>
        /// Computes the image index for the specified values. If the image index is out of range <see cref="TexHelper.IndexOutOfRange" /> is returned.
        /// </summary>
        /// <param name="mip">The mip.</param>
        /// <param name="item">The item.</param>
        /// <param name="slice">The slice.</param>
        /// <returns>The image index. If the image index is out of range <see cref="TexHelper.IndexOutOfRange" /> is returned.</returns>
        public abstract Size_t ComputeImageIndex(Size_t mip, Size_t item, Size_t slice);

        public abstract Image GetImage(Size_t idx);

        public abstract Image GetImage(Size_t mip, Size_t item, Size_t slice);

        public abstract TexMetadata GetMetadata();

        public abstract bool OverrideFormat(DXGI_FORMAT f);

        /// <summary>
        /// Whether this ScratchImage owns the pixel data;
        /// </summary>
        public abstract bool OwnsData();

        /// <summary>
        /// Normally GetImage().pixels should be used instead, because this only returns a pointer to the pixel data if this image owns the pixel data.
        /// </summary>
        public abstract IntPtr GetPixels();

        /// <summary>
        /// This only returns a value if this image owns the pixel data.
        /// </summary>
        public abstract Size_T GetPixelsSize();

        /// <summary>
        /// Determines whether all pixels are opaque. This method is not supported by temporary scratch images.
        /// </summary>
        public abstract bool IsAlphaAllOpaque();

        #region creating image copies

        /// <summary>
        /// Creates a new ScratchImage (deep copy).
        /// </summary>
        /// <param name="imageIndex">Index of the image to make a copy of.</param>
        /// <param name="allow1D">if set to <c>true</c> and the height of the image is 1 a 1D Texture is created instead a 2D Texture.</param>
        /// <param name="flags">The flags.</param>
        public abstract ScratchImage CreateImageCopy(Size_t imageIndex, bool allow1D, CP_FLAGS flags);

        /// <summary>
        /// Creates a new Array ScratchImage (deep copy).
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="nImages">The n images.</param>
        /// <param name="allow1D">if set to <c>true</c> and the height of the image is 1 a 1D Texture is created instead a 2D Texture.</param>
        /// <param name="flags">The flags.</param>
        public abstract ScratchImage CreateArrayCopy(Size_t startIndex, Size_t nImages, bool allow1D, CP_FLAGS flags);

        public abstract ScratchImage CreateCubeCopy(Size_t startIndex, Size_t nImages, CP_FLAGS flags);

        public abstract ScratchImage CreateVolumeCopy(Size_t startIndex, Size_t depth, CP_FLAGS flags);

        /// <summary>
        /// Creates a copy of the image but with empty mip maps (not part of original DirectXTex).
        /// Can be used to generate the mip maps by other means (DirectXTex MipMap Generation is pretty slow).
        /// </summary>
        /// <param name="levels">The levels.</param>
        /// <param name="fmt">The format.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="zeroOutMipMaps">if set to <c>true</c> the mip map levels are zeroed out.</param>
        public abstract ScratchImage CreateCopyWithEmptyMipMaps(Size_t levels, DXGI_FORMAT fmt, CP_FLAGS flags, bool zeroOutMipMaps);

        #endregion creating image copies

        #region saving images to file/memory

        public abstract UnmanagedMemoryStream SaveToDDSMemory(Size_t imageIndex, DDS_FLAGS flags);

        public abstract UnmanagedMemoryStream SaveToDDSMemory(DDS_FLAGS flags);

        public abstract UnmanagedMemoryStream SaveToHDRMemory(Size_t imageIndex);

        public abstract void SaveToHDRFile(Size_t imageIndex, String szFile);

        public abstract UnmanagedMemoryStream SaveToTGAMemory(Size_t imageIndex);

        public abstract void SaveToTGAFile(Size_t imageIndex, String szFile);

        public abstract void SaveToDDSFile(Size_t imageIndex, DDS_FLAGS flags, String szFile);

        public abstract void SaveToDDSFile(DDS_FLAGS flags, String szFile);

        public abstract UnmanagedMemoryStream SaveToWICMemory(Size_t imageIndex, WIC_FLAGS flags, Guid guidContainerFormat);

        public abstract UnmanagedMemoryStream SaveToWICMemory(Size_t startIndex, Size_t nImages, WIC_FLAGS flags, Guid guidContainerFormat);

        public abstract void SaveToWICFile(Size_t imageIndex, WIC_FLAGS flags, Guid guidContainerFormat, String szFile);

        public abstract void SaveToWICFile(Size_t startIndex, Size_t nImages, WIC_FLAGS flags, Guid guidContainerFormat, String szFile);

        public abstract UnmanagedMemoryStream SaveToJPGMemory(Size_t imageIndex, float quality);

        public abstract void SaveToJPGFile(Size_t imageIndex, float quality, String szFile);

        #endregion saving images to file/memory

        #region Texture conversion, resizing, mipmap generation, and block compression

        public abstract ScratchImage FlipRotate(Size_t imageIndex, TEX_FR_FLAGS flags);

        public abstract ScratchImage FlipRotate(TEX_FR_FLAGS flags);

        public abstract ScratchImage Resize(Size_t imageIndex, Size_t width, Size_t height, TEX_FILTER_FLAGS filter);

        /// <summary>
        /// Resize the image to width x height. Defaults to Fant filtering. Note for a complex resize, the result will always have mipLevels == 1.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>The resized image.</returns>
        public abstract ScratchImage Resize(Size_t width, Size_t height, TEX_FILTER_FLAGS filter);

        public abstract ScratchImage Convert(Size_t imageIndex, DXGI_FORMAT format, TEX_FILTER_FLAGS filter, float threshold);

        public abstract ScratchImage Convert(DXGI_FORMAT format, TEX_FILTER_FLAGS filter, float threshold);

        /// <summary>
        /// Converts the image from a planar format to an equivalent non-planar format.
        /// </summary>
        public abstract ScratchImage ConvertToSinglePlane(Size_t imageIndex);

        /// <summary>
        /// Converts the image from a planar format to an equivalent non-planar format.
        /// </summary>
        public abstract ScratchImage ConvertToSinglePlane();

        /// <summary>
        /// Generates the mip maps.
        /// </summary>
        /// <param name="imageIndex">Index of the image.</param>
        /// <param name="filter">The filter. Defaults to Fant filtering which is equivalent to a box filter.</param>
        /// <param name="levels">
        /// Levels of '0' indicates a full mipchain, otherwise is generates that number of total levels (including the source base
        /// image).
        /// </param>
        /// <param name="allow1D">if set to <c>true</c> and the height of the image is 1 a 1D Texture is created instead a 2D Texture.</param>
        public abstract ScratchImage GenerateMipMaps(Size_t imageIndex, TEX_FILTER_FLAGS filter, Size_t levels, bool allow1D);

        /// <summary>
        /// Generates the mip maps.
        /// </summary>
        /// <param name="filter">The filter. Defaults to Fant filtering which is equivalent to a box filter.</param>
        /// <param name="levels">
        /// Levels of '0' indicates a full mipchain, otherwise is generates that number of total levels (including the source base
        /// image).
        /// </param>
        public abstract ScratchImage GenerateMipMaps(TEX_FILTER_FLAGS filter, Size_t levels);

        /// <summary>
        /// Generates the mip maps.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="depth">The depth.</param>
        /// <param name="filter">The filter. Defaults to Fant filtering which is equivalent to a box filter.</param>
        /// <param name="levels">
        /// Levels of '0' indicates a full mipchain, otherwise is generates that number of total levels (including the source base
        /// image).
        /// </param>
        public abstract ScratchImage GenerateMipMaps3D(Size_t startIndex, Size_t depth, TEX_FILTER_FLAGS filter, Size_t levels);

        /// <summary>
        /// Generates the mip maps.
        /// </summary>
        /// <param name="filter">The filter. Defaults to Fant filtering which is equivalent to a box filter.</param>
        /// <param name="levels">
        /// Levels of '0' indicates a full mipchain, otherwise is generates that number of total levels (including the source base
        /// image).
        /// </param>
        public abstract ScratchImage GenerateMipMaps3D(TEX_FILTER_FLAGS filter, Size_t levels);

        /// <summary>
        /// Converts to/from a premultiplied alpha version of the texture.
        /// </summary>
        /// <param name="imageIndex">Index of the image.</param>
        /// <param name="flags">The flags.</param>
        public abstract ScratchImage PremultiplyAlpha(Size_t imageIndex, TEX_PMALPHA_FLAGS flags);

        /// <summary>
        /// Converts to/from a premultiplied alpha version of the texture.
        /// </summary>
        /// <param name="flags">The flags.</param>
        public abstract ScratchImage PremultiplyAlpha(TEX_PMALPHA_FLAGS flags);

        /// <summary>
        /// Compresses the specified source image. Note that threshold is only used by BC1.
        /// </summary>
        /// <param name="imageIndex">Index of the image.</param>
        /// <param name="format">The format.</param>
        /// <param name="compress">The compress.</param>
        /// <param name="threshold">The threshold. Default 0.5</param>
        public abstract ScratchImage Compress(Size_t imageIndex, DXGI_FORMAT format, TEX_COMPRESS_FLAGS compress, float threshold);

        /// <summary>
        /// DirectCompute-based compression
        /// </summary>
        /// <param name="imageIndex">Index of the image.</param>
        /// <param name="pDevice">The device.</param>
        /// <param name="format">The format.</param>
        /// <param name="compress">The compress.</param>
        /// <param name="alphaWeight">The alpha weight (is only used by BC7. 1.0 is the typical value to use).</param>
        public abstract ScratchImage Compress(
            Size_t imageIndex,
            ID3D11DevicePtr pDevice,
            DXGI_FORMAT format,
            TEX_COMPRESS_FLAGS compress,
            float alphaWeight);

        /// <summary>
        /// Compresses the specified source image. Note that threshold is only used by BC1.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="compress">The compress.</param>
        /// <param name="threshold">The threshold. Default 0.5</param>
        public abstract ScratchImage Compress(DXGI_FORMAT format, TEX_COMPRESS_FLAGS compress, float threshold);

        /// <summary>
        /// DirectCompute-based compression
        /// </summary>
        /// <param name="pDevice">The device.</param>
        /// <param name="format">The format.</param>
        /// <param name="compress">The compress.</param>
        /// <param name="alphaWeight">The alpha weight (is only used by BC7. 1.0 is the typical value to use).</param>
        public abstract ScratchImage Compress(ID3D11DevicePtr pDevice, DXGI_FORMAT format, TEX_COMPRESS_FLAGS compress, float alphaWeight);

        public abstract ScratchImage Decompress(Size_t imageIndex, DXGI_FORMAT format);

        public abstract ScratchImage Decompress(DXGI_FORMAT format);

        #endregion Texture conversion, resizing, mipmap generation, and block compression

        #region Normal map operations

        public abstract ScratchImage ComputeNormalMap(Size_t imageIndex, CNMAP_FLAGS flags, float amplitude, DXGI_FORMAT format);

        public abstract ScratchImage ComputeNormalMap(CNMAP_FLAGS flags, float amplitude, DXGI_FORMAT format);

        #endregion Normal map operations

        #region Misc image operations

        public abstract void EvaluateImage(Size_t imageIndex, EvaluatePixelsDelegate pixelFunc);

        public abstract void EvaluateImage(EvaluatePixelsDelegate pixelFunc);

        public abstract ScratchImage TransformImage(Size_t imageIndex, TransformPixelsDelegate pixelFunc);

        public abstract ScratchImage TransformImage(TransformPixelsDelegate pixelFunc);

        #endregion Misc image operations

        #region Direct3D 11 functions

        public abstract ID3D11ResourcePtr CreateTexture(ID3D11DevicePtr pDevice);

        public abstract ID3D11ShaderResourceViewPtr CreateShaderResourceView(ID3D11DevicePtr pDevice);

        public abstract ID3D11ResourcePtr CreateTextureEx(
            ID3D11DevicePtr pDevice,
            D3D11_USAGE usage,
            D3D11_BIND_FLAG bindFlags,
            D3D11_CPU_ACCESS_FLAG cpuAccessFlags,
            D3D11_RESOURCE_MISC_FLAG miscFlags,
            bool forceSRGB);

        public abstract ID3D11ShaderResourceViewPtr CreateShaderResourceViewEx(
            ID3D11DevicePtr pDevice,
            D3D11_USAGE usage,
            D3D11_BIND_FLAG bindFlags,
            D3D11_CPU_ACCESS_FLAG cpuAccessFlags,
            D3D11_RESOURCE_MISC_FLAG miscFlags,
            bool forceSRGB);

        #endregion Direct3D 11 functions

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    public abstract class TexHelper
    {
        public static TexHelper Instance { get; private set; }

        static TexHelper()
        {
            string folder = Environment.CurrentDirectory;
            string fileName = "DirectXTexNetImpl.dll";
            string platform = Environment.Is64BitProcess ? "x64" : "x86";

            foreach (var filePath in new[]
                                     {
                                         Path.Combine(folder, fileName),
                                         Path.Combine(folder, platform, fileName),
                                         Path.Combine(folder, "runtimes", "win-" + platform, "native", fileName)
                                     })
            {
                if (File.Exists(filePath))
                {
                    LoadInstanceFrom(filePath);
                }
            }
        }

        public static void LoadInstanceFrom(string filePath)
        {
            Instance = (TexHelper)Activator.CreateInstance(Assembly.LoadFile(filePath).GetType("DirectXTexNet.TexHelperImpl"));
        }

        public readonly Size_t IndexOutOfRange = unchecked((Size_t)(Environment.Is64BitProcess ? UInt64.MaxValue : UInt32.MaxValue));

        //internal DirectXTexNet() { }

        public abstract void SetOmpMaxThreadCount(int maxThreadCount);

        #region DXGI Format Utilities

        public abstract bool IsValid(DXGI_FORMAT fmt);

        public abstract bool IsCompressed(DXGI_FORMAT fmt);

        public abstract bool IsPacked(DXGI_FORMAT fmt);

        public abstract bool IsVideo(DXGI_FORMAT fmt);

        public abstract bool IsPlanar(DXGI_FORMAT fmt);

        public abstract bool IsPalettized(DXGI_FORMAT fmt);

        public abstract bool IsDepthStencil(DXGI_FORMAT fmt);

        public abstract bool IsSRGB(DXGI_FORMAT fmt);

        public abstract bool IsTypeless(DXGI_FORMAT fmt, bool partialTypeless);

        public abstract bool HasAlpha(DXGI_FORMAT fmt);

        public abstract Size_t BitsPerPixel(DXGI_FORMAT fmt);

        public abstract Size_t BitsPerColor(DXGI_FORMAT fmt);

        public abstract void ComputePitch(
            DXGI_FORMAT fmt,
            Size_t width,
            Size_t height,
            out Size_T rowPitch,
            out Size_T slicePitch,
            CP_FLAGS flags);

        public abstract Size_T ComputeScanlines(DXGI_FORMAT fmt, Size_t height);

        /// <summary>
        /// Computes the image index for the specified values. If the image index is out of range <see cref="TexHelper.IndexOutOfRange" /> is returned.
        /// The ScratchImage provide a ComputeImageIndex method as well, which should be used preferrably.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="mip">The mip.</param>
        /// <param name="item">The item.</param>
        /// <param name="slice">The slice.</param>
        /// <returns>
        /// The image index. If the image index is out of range <see cref="TexHelper.IndexOutOfRange" /> is returned.
        /// </returns>
        public abstract Size_t ComputeImageIndex(TexMetadata metadata, Size_t mip, Size_t item, Size_t slice);

        public abstract DXGI_FORMAT MakeSRGB(DXGI_FORMAT fmt);

        public abstract DXGI_FORMAT MakeTypeless(DXGI_FORMAT fmt);

        public abstract DXGI_FORMAT MakeTypelessUNORM(DXGI_FORMAT fmt);

        public abstract DXGI_FORMAT MakeTypelessFLOAT(DXGI_FORMAT fmt);

        #endregion DXGI Format Utilities

        #region Texture metadata

        public abstract TexMetadata GetMetadataFromDDSMemory(IntPtr pSource, Size_T size, DDS_FLAGS flags);

        public abstract TexMetadata GetMetadataFromDDSFile(String szFile, DDS_FLAGS flags);

        public abstract TexMetadata GetMetadataFromHDRMemory(IntPtr pSource, Size_T size);

        public abstract TexMetadata GetMetadataFromHDRFile(String szFile);

        public abstract TexMetadata GetMetadataFromTGAMemory(IntPtr pSource, Size_T size);

        public abstract TexMetadata GetMetadataFromTGAFile(String szFile);

        public abstract TexMetadata GetMetadataFromWICMemory(IntPtr pSource, Size_T size, WIC_FLAGS flags);

        public abstract TexMetadata GetMetadataFromWICFile(String szFile, WIC_FLAGS flags);

        #endregion Texture metadata

        #region create new ScratchImages

        public abstract ScratchImage Initialize(TexMetadata mdata, CP_FLAGS flags);

        public abstract ScratchImage Initialize1D(DXGI_FORMAT fmt, Size_t length, Size_t arraySize, Size_t mipLevels, CP_FLAGS flags);

        public abstract ScratchImage Initialize2D(
            DXGI_FORMAT fmt,
            Size_t width,
            Size_t height,
            Size_t arraySize,
            Size_t mipLevels,
            CP_FLAGS flags);

        public abstract ScratchImage Initialize3D(DXGI_FORMAT fmt, Size_t width, Size_t height, Size_t depth, Size_t mipLevels, CP_FLAGS flags);

        public abstract ScratchImage InitializeCube(
            DXGI_FORMAT fmt,
            Size_t width,
            Size_t height,
            Size_t nCubes,
            Size_t mipLevels,
            CP_FLAGS flags);

        /// <summary>
        /// Creates a temporary image collection (Not part of the original DirectXTex). This does not copy the data. Be sure to not dispose the original ScratchImages that were combined in this
        /// collection. Alternatively the ownership of the original ScratchImage(s) can be passed to this instance.
        /// </summary>
        /// <param name="images">The images.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="takeOwnershipOf">Optional objects this instance should take ownership of.</param>
        public abstract ScratchImage InitializeTemporary(Image[] images, TexMetadata metadata, params IDisposable[] takeOwnershipOf);

        #endregion create new ScratchImages

        #region Image I/O

        // DDS operations
        public abstract ScratchImage LoadFromDDSMemory(IntPtr pSource, Size_T size, DDS_FLAGS flags);

        public abstract ScratchImage LoadFromDDSFile(String szFile, DDS_FLAGS flags);

        // HDR operations
        public abstract ScratchImage LoadFromHDRMemory(IntPtr pSource, Size_T size);

        public abstract ScratchImage LoadFromHDRFile(String szFile);

        // TGA operations
        public abstract ScratchImage LoadFromTGAMemory(IntPtr pSource, Size_T size);

        public abstract ScratchImage LoadFromTGAFile(String szFile);

        // WIC operations
        public abstract ScratchImage LoadFromWICMemory(IntPtr pSource, Size_T size, WIC_FLAGS flags);

        public abstract ScratchImage LoadFromWICFile(String szFile, WIC_FLAGS flags);

        #endregion Image I/O

        #region Misc image operations

        public abstract void CopyRectangle(
            Image srcImage,
            Size_t srcX,
            Size_t srcY,
            Size_t srcWidth,
            Size_t srcHeight,
            Image dstImage,
            TEX_FILTER_FLAGS filter,
            Size_t xOffset,
            Size_t yOffset);

        public abstract void ComputeMSE(Image image1, Image image2, out float mse, out MseV mseV, CMSE_FLAGS flags);

        #endregion Misc image operations

        #region WIC utility

        public abstract Guid GetWICCodec(WICCodecs codec);

        public abstract IWICImagingFactoryPtr GetWICFactory(bool iswic2);

        public abstract void SetWICFactory(IWICImagingFactoryPtr pWIC);

        #endregion WIC utility

        #region Direct3D 11 functions

        public abstract bool IsSupportedTexture(ID3D11DevicePtr pDevice, TexMetadata metadata);

        public abstract ScratchImage CaptureTexture(ID3D11DevicePtr pDevice, ID3D11DeviceContextPtr pContext, ID3D11ResourcePtr pSource);

        #endregion Direct3D 11 functions
    }
}

#pragma warning restore CA1069 // Enumerationswerte dürfen nicht dupliziert werden