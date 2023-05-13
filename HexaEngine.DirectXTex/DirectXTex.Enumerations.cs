using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexaEngine.DirectXTex
{
    [Flags]
    public enum CMSEFlags
    {
        Default = 0,

        /// <summary>
        /// Indicates that image needs gamma correction before comparision
        /// </summary>
        Image1SRGB = 0x1,

        /// <summary>
        /// Indicates that image needs gamma correction before comparision
        /// </summary>
        Image2SRGB = 0x2,

        /// <summary>
        /// Ignore the channel when computing MSE
        /// </summary>
        IgnoreRed = 0x10,

        /// <summary>
        /// Ignore the channel when computing MSE
        /// </summary>
        IgnoreGreen = 0x20,

        /// <summary>
        /// Ignore the channel when computing MSE
        /// </summary>
        IgnoreBlue = 0x40,

        /// <summary>
        /// Ignore the channel when computing MSE
        /// </summary>
        IgnoreAlpha = 0x80,

        /// <summary>
        /// Indicates that image should be scaled and biased before comparison (i.e. UNORM -> SNORM)
        /// </summary>
        Image1x2Bias = 0x100,

        /// <summary>
        /// Indicates that image should be scaled and biased before comparison (i.e. UNORM -> SNORM)
        /// </summary>
        Image2x2Bias = 0x200,
    };

    [Flags]
    public enum CNMAPFlags
    {
        Default = 0,

        /// <summary>
        /// Channel selection when evaluting color value for height
        /// </summary>
        ChannelRed = 0x1,

        /// <summary>
        /// Channel selection when evaluting color value for height
        /// </summary>
        ChannelGreen = 0x2,

        /// <summary>
        /// Channel selection when evaluting color value for height
        /// </summary>
        ChannelBlue = 0x3,

        /// <summary>
        /// Channel selection when evaluting color value for height
        /// </summary>
        ChannelAlpha = 0x4,

        /// <summary>
        /// Channel selection when evaluting color value for height
        /// Luminance is a combination of red, green, and blue
        /// </summary>
        ChannelLuminance = 0x5,

        /// <summary>
        /// Use mirror semantics for scanline references (defaults to wrap)
        /// </summary>
        MirrorU = 0x1000,

        /// <summary>
        /// Use mirror semantics for scanline references (defaults to wrap)
        /// </summary>
        MirrorV = 0x2000,

        /// <summary>
        /// Use mirror semantics for scanline references (defaults to wrap)
        /// </summary>
        Mirror = 0x3000,

        /// <summary>
        /// Inverts normal sign
        /// </summary>
        InvertSign = 0x4000,

        /// <summary>
        /// Computes a crude occlusion term stored in the alpha channel
        /// </summary>
        ComputeOcclusion = 0x8000
    }

    [Flags]
    public enum CPFlags
    {
        /// <summary>
        /// Normal operation
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Assume pitch is DWORD aligned instead of BYTE aligned
        /// </summary>
        LegacyDWORD = 0x1,

        /// <summary>
        /// Assume pitch is 16-byte aligned instead of BYTE aligned
        /// </summary>
        Paragraph = 0x2,

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
        Page4K = 0x200,

        /// <summary>
        /// BC formats with malformed mipchain blocks smaller than 4x4
        /// </summary>
        BadDXTNTails = 0x1000,

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

    [Flags]
    public enum CreateTexFlags : uint
    {
        Default = 0,
        ForceSRGB = 0x1,
        IgnoreSRGB = 0x2,
    }

    [Flags]
    public enum DDSFlags
    {
        None = 0x0,

        /// <summary>
        /// Assume pitch is DWORD aligned instead of BYTE aligned (used by some legacy DDS files)
        /// </summary>
        LegacyDWORD = 0x1,

        /// <summary>
        /// Do not implicitly convert legacy formats that result in larger pixel sizes (24 bpp, 3:3:2, A8L8, A4L4, P8, A8P8)
        /// </summary>
        NoLegacyExpansion = 0x2,

        /// <summary>
        /// Do not use work-around for long-standing D3DX DDS file format issue which reversed the 10:10:10:2 color order masks
        /// </summary>
        NoR10B10G10A2Fixup = 0x4,

        /// <summary>
        /// Convert DXGI 1.1 BGR formats to DXGI_FORMAT_R8G8B8A8_UNORM to avoid use of optional WDDM 1.1 formats
        /// </summary>
        ForceRGB = 0x8,

        /// <summary>
        /// Conversions avoid use of 565, 5551, and 4444 formats and instead expand to 8888 to avoid use of optional WDDM 1.2 formats
        /// </summary>
        No16BPP = 0x10,

        /// <summary>
        /// When loading legacy luminance formats expand replicating the color channels rather than leaving them packed (L8, L16, A8L8)
        /// </summary>
        ExpandLuminance = 0x20,

        /// <summary>
        /// Some older DXTn DDS files incorrectly handle mipchain tails for blocks smaller than 4x4
        /// </summary>
        BadDXTNTails = 0x40,

        /// <summary>
        /// Always use the 'DX10' header extension for DDS writer (i.e. don't try to write DX9 compatible DDS files)
        /// </summary>
        ForceDX10Ext = 0x10000,

        /// <summary>
        /// FORCE_DX10_EXT including miscFlags2 information (result may not be compatible with D3DX10 or D3DX11)
        /// </summary>
        ForceDX10ExtMisc2 = 0x20000,

        /// <summary>
        /// Force use of legacy header for DDS writer (will fail if unable to write as such)
        /// </summary>
        ForceDX9Legacy = 0x40000,

        /// <summary>
        /// Enables the loader to read large dimension .dds files (i.e. greater than known hardware requirements)
        /// </summary>
        AllowLargeFiles = 0x1000000,
    }

    [Flags]
    public enum FormatType
    {
        Typeless,
        Float,
        UNorm,
        SNorm,
        UInt,
        SInt,
    }

    /// <summary>
    /// Matches DDS_ALPHA_MODE, encoded in MISC_FLAGS2
    /// </summary>
    public enum TexAlphaMode
    {
        Unknown = 0,
        Straight = 1,
        Premultiplied = 2,
        Opaque = 3,
        Custom = 4,
    }

    [Flags]
    public enum TexCompressFlags
    {
        Default = 0,

        /// <summary>
        /// Enables dithering RGB colors for BC1-3 compression
        /// </summary>
        DitherRGB = 0x10000,

        /// <summary>
        /// Enables dithering alpha for BC1-3 compression
        /// </summary>
        DitherA = 0x20000,

        /// <summary>
        /// Enables both RGB and alpha dithering for BC1-3 compression
        /// </summary>
        Dither = 0x30000,

        /// <summary>
        /// Uniform color weighting for BC1-3 compression; by default uses perceptual weighting
        /// </summary>
        Uniform = 0x40000,

        /// <summary>
        /// Enables exhaustive search for BC7 compress for mode 0 and 2; by default skips trying these modes
        /// </summary>
        BC7Use3Sunsets = 0x80000,

        /// <summary>
        /// Minimal modes (usually mode 6) for BC7 compression
        /// </summary>
        BC7Quick = 0x100000,

        SRGBIn = 0x1000000,
        SRGBOut = 0x2000000,

        /// <summary>
        /// if the input format type is IsSRGB(), then SRGB_IN is on by default
        /// if the output format type is IsSRGB(), then SRGB_OUT is on by default
        /// </summary>
        SRGB = SRGBIn | SRGBOut,

        /// <summary>
        /// Compress is free to use multithreading to improve performance (by default it does not use multithreading)
        /// </summary>
        Parallel = 0x10000000
    }

    public enum TexDimension
    {
        Texture1D = 2,
        Texture2D = 3,
        Texture3D = 4,
    }

    [Flags]
    public enum TexFilterFlags
    {
        Default = 0,

        /// <summary>
        /// Wrap vs. Mirror vs. Clamp filtering options
        /// </summary>
        WrapU = 0x1,

        /// <summary>
        /// Wrap vs. Mirror vs. Clamp filtering options
        /// </summary>
        WrapV = 0x2,

        /// <summary>
        /// Wrap vs. Mirror vs. Clamp filtering options
        /// </summary>
        WrapW = 0x4,

        /// <summary>
        /// Wrap vs. Mirror vs. Clamp filtering options
        /// </summary>
        Wrap = WrapU | WrapV | WrapW,

        MirrorU = 0x10,

        /// <summary>
        /// Wrap vs. Mirror vs. Clamp filtering options
        /// </summary>
        MirrorV = 0x20,

        /// <summary>
        /// Wrap vs. Mirror vs. Clamp filtering options
        /// </summary>
        MirrorW = 0x40,

        /// <summary>
        /// Wrap vs. Mirror vs. Clamp filtering options
        /// </summary>
        Mirror = MirrorU | MirrorV | MirrorW,

        /// <summary>
        /// Resize color and alpha channel independently
        /// </summary>
        SeparateAlpha = 0x100,

        /// <summary>
        /// Enable *2 - 1 conversion cases for unorm to/from float and positive-only float formats
        /// </summary>
        FloatX2Bias = 0x200,

        /// <summary>
        /// When converting RGB to R, defaults to using grayscale. These flags indicate copying a specific channel instead
        /// When converting RGB to RG, defaults to copying RED | GREEN. These flags control which channels are selected instead.
        /// </summary>
        RGBCopyRed = 0x1000,

        /// <summary>
        /// When converting RGB to R, defaults to using grayscale. These flags indicate copying a specific channel instead
        /// When converting RGB to RG, defaults to copying RED | GREEN. These flags control which channels are selected instead.
        /// </summary>
        RGBCopyGreen = 0x2000,

        /// <summary>
        /// When converting RGB to R, defaults to using grayscale. These flags indicate copying a specific channel instead
        /// When converting RGB to RG, defaults to copying RED | GREEN. These flags control which channels are selected instead.
        /// </summary>
        RGBCopyBlue = 0x4000,

        /// <summary>
        /// Use ordered 4x4 dithering for any required conversions
        /// </summary>
        Dither = 0x10000,

        /// <summary>
        /// Use error-diffusion dithering for any required conversions
        /// </summary>
        DitherDiffusion = 0x20000,

        /// <summary>
        /// Filtering mode to use for any required image resizing
        /// </summary>
        Point = 0x100000,

        /// <summary>
        /// Filtering mode to use for any required image resizing
        /// </summary>
        Linear = 0x200000,

        /// <summary>
        /// Filtering mode to use for any required image resizing
        /// </summary>
        Cubic = 0x300000,

        /// <summary>
        /// Filtering mode to use for any required image resizing
        /// </summary>
        Box = 0x400000,

        /// <summary>
        /// Filtering mode to use for any required image resizing
        /// Equiv to Box filtering for mipmap generation
        /// </summary>
        Fant = 0x400000,

        /// <summary>
        /// Filtering mode to use for any required image resizing
        /// </summary>
        Triangle = 0x500000,

        SRGBIn = 0x1000000,
        SRGBOut = 0x2000000,

        /// <summary>
        /// sRGB to/from RGB for use in conversion operations
        /// if the input format type is IsSRGB(), then SRGB_IN is on by default
        /// if the output format type is IsSRGB(), then SRGB_OUT is on by default
        /// </summary>
        SRGB = SRGBIn | SRGBOut,

        /// <summary>
        /// Forces use of the non-WIC path when both are an option
        /// </summary>
        ForceNonWIC = 0x10000000,

        /// <summary>
        /// Forces use of the WIC path even when logic would have picked a non-WIC path when both are an option
        /// </summary>
        ForceWIC = 0x20000000
    }

    [Flags]
    public enum TexFrFlags
    {
        Rotate0 = 0x0,
        Rotate90 = 0x1,
        Rotate180 = 0x2,
        Rotate270 = 0x3,
        FlipHorizontal = 0x08,
        FlipVertical = 0x10,
    }

    /// <summary>
    /// Subset here matches D3D10_RESOURCE_MISC_FLAG and D3D11_RESOURCE_MISC_FLAG
    /// </summary>
    [Flags]
    public enum TexMiscFlags : uint
    {
        GenerateMips = unchecked(1),
        Shared = unchecked(2),
        TextureCube = unchecked(4),
        DrawIndirectArguments = unchecked(16),
        BufferAllowRawViews = unchecked(32),
        BufferStructured = unchecked(64),
        ResourceClamp = unchecked(128),
        SharedKeyedMutex = unchecked(256),
        GdiCompatible = unchecked(512),
        SharedNTHandle = unchecked(2048),
        RestrictedContent = unchecked(4096),
        RestrictSharedResource = unchecked(8192),
        RestrictSharedResourceDriver = unchecked(16384),
        Guarded = unchecked(32768),
        TilePool = unchecked(131072),
        Tiled = unchecked(262144),
        HardwareProtected = unchecked(524288),
        SharedDisplayable = unchecked(1048576),
        SharedExclusiveWriter = unchecked(2097152),
        None = unchecked(0)
    }

    [Flags]
    public enum TexMiscFlags2 : uint
    {
        AlphaModeMask = 0x7,
    }

    [Flags]
    public enum TexPmAlphaFlags
    {
        Default = 0,

        /// <summary>
        /// ignores sRGB colorspace conversions
        /// </summary>
        IgnoreSRGB = 0x1,

        /// <summary>
        /// converts from premultiplied alpha back to straight alpha
        /// </summary>
        Reverse = 0x2,

        SRGBIn = 0x1000000,
        SRGBOut = 0x2000000,

        /// <summary>
        /// if the input format type is IsSRGB(), then SRGB_IN is on by default
        /// if the output format type is IsSRGB(), then SRGB_OUT is on by default
        /// </summary>
        SRGB = SRGBIn | SRGBOut
    }

    [Flags]
    public enum TGAFlags : ulong
    {
        None = 0x0,

        /// <summary>
        /// 24bpp files are returned as BGRX; 32bpp files are returned as BGRA
        /// </summary>
        BGR = 0x1,

        /// <summary>
        /// If the loaded image has an all zero alpha channel, normally we assume it should be opaque. This flag leaves it alone.
        /// </summary>
        AllowAllZeroAlpha = 0x2,

        /// <summary>
        /// Ignores sRGB TGA 2.0 metadata if present in the file
        /// </summary>
        IgnoreSRGB = 0x10,

        /// <summary>
        /// Writes sRGB metadata into the file reguardless of format (TGA 2.0 only)
        /// </summary>
        ForceSRGB = 0x20,

        /// <summary>
        /// Writes linear gamma metadata into the file reguardless of format (TGA 2.0 only)
        /// </summary>
        ForceLinear = 0x40,

        /// <summary>
        /// If no colorspace is specified in TGA 2.0 metadata, assume sRGB
        /// </summary>
        DefaultSRGB = 0x80,
    }

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

    [Flags]
    public enum WICFlags
    {
        None = 0x0,

        /// <summary>
        /// Loads DXGI 1.1 BGR formats as DXGI_FORMAT_R8G8B8A8_UNORM to avoid use of optional WDDM 1.1 formats
        /// </summary>
        ForceRGB = 0x1,

        /// <summary>
        /// Loads DXGI 1.1 X2 10:10:10:2 format as DXGI_FORMAT_R10G10B10A2_UNORM
        /// </summary>
        NoX2Bias = 0x2,

        /// <summary>
        /// Loads 565, 5551, and 4444 formats as 8888 to avoid use of optional WDDM 1.2 formats
        /// </summary>
        No16BPP = 0x4,

        /// <summary>
        /// Loads 1-bit monochrome (black and white) as R1_UNORM rather than 8-bit grayscale
        /// </summary>
        AllowMono = 0x8,

        /// <summary>
        /// Loads all images in a multi-frame file, converting/resizing to match the first frame as needed, defaults to 0th frame otherwise
        /// </summary>
        AllFrames = 0x10,

        /// <summary>
        /// Ignores sRGB metadata if present in the file
        /// </summary>
        IgnoreSRGB = 0x20,

        /// <summary>
        /// Writes sRGB metadata into the file reguardless of format
        /// </summary>
        ForceSRGB = 0x40,

        /// <summary>
        /// Writes linear gamma metadata into the file reguardless of format
        /// </summary>
        ForceLinear = 0x80,

        /// <summary>
        /// If no colorspace is specified, assume sRGB
        /// </summary>
        DefaultSRGB = 0x100,

        /// <summary>
        /// Use ordered 4x4 dithering for any required conversions
        /// </summary>
        Dither = 0x10000,

        /// <summary>
        /// Use error-diffusion dithering for any required conversions
        /// </summary>
        DitherDiffusion = 0x20000,

        /// <summary>
        /// Filtering mode to use for any required image resizing (only needed when loading arrays of differently sized images; defaults to Fant)
        /// </summary>
        FilterPoint = 0x100000,

        /// <summary>
        /// Filtering mode to use for any required image resizing (only needed when loading arrays of differently sized images; defaults to Fant)
        /// </summary>
        FilterLinear = 0x200000,

        /// <summary>
        /// Filtering mode to use for any required image resizing (only needed when loading arrays of differently sized images; defaults to Fant)
        /// </summary>
        FilterCubic = 0x300000,

        /// <summary>
        /// Filtering mode to use for any required image resizing (only needed when loading arrays of differently sized images; defaults to Fant)
        /// Combination of Linear and Box filter
        /// </summary>
        FilterFant = 0x400000
    }
}