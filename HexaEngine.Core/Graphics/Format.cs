namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Specifies pixel format for textures and resources.
    /// </summary>
    public enum Format
    {
        /// <summary>
        /// Unknown format.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 32-bit typeless format with 4 components (red, green, blue, alpha).
        /// </summary>
        R32G32B32A32Typeless = 1,

        /// <summary>
        /// 32-bit floating-point format with 4 components (red, green, blue, alpha).
        /// </summary>
        R32G32B32A32Float = 2,

        /// <summary>
        /// 32-bit unsigned integer format with 4 components (red, green, blue, alpha).
        /// </summary>
        R32G32B32A32UInt = 3,

        /// <summary>
        /// 32-bit signed integer format with 4 components (red, green, blue, alpha).
        /// </summary>
        R32G32B32A32SInt = 4,

        /// <summary>
        /// 96-bit typeless format with 3 components (red, green, blue).
        /// </summary>
        R32G32B32Typeless = 5,

        /// <summary>
        /// 96-bit floating-point format with 3 components (red, green, blue).
        /// </summary>
        R32G32B32Float = 6,

        /// <summary>
        /// 96-bit unsigned integer format with 3 components (red, green, blue).
        /// </summary>
        R32G32B32UInt = 7,

        /// <summary>
        /// 96-bit signed integer format with 3 components (red, green, blue).
        /// </summary>
        R32G32B32SInt = 8,

        /// <summary>
        /// 64-bit typeless format with 4 components (red, green, blue, alpha).
        /// </summary>
        R16G16B16A16Typeless = 9,

        /// <summary>
        /// 64-bit floating-point format with 4 components (red, green, blue, alpha).
        /// </summary>
        R16G16B16A16Float = 10,

        /// <summary>
        /// 64-bit normalized format with 4 components (red, green, blue, alpha).
        /// </summary>
        R16G16B16A16UNorm = 11,

        /// <summary>
        /// 64-bit unsigned integer format with 4 components (red, green, blue, alpha).
        /// </summary>
        R16G16B16A16UInt = 12,

        /// <summary>
        /// 64-bit normalized format with 4 components (red, green, blue, alpha).
        /// </summary>
        R16G16B16A16SNorm = 13,

        /// <summary>
        /// 64-bit signed integer format with 4 components (red, green, blue, alpha).
        /// </summary>
        R16G16B16A16Sint = 14,

        /// <summary>
        /// 64-bit typeless format with 2 components (red, green).
        /// </summary>
        R32G32Typeless = 0xF,

        /// <summary>
        /// 64-bit floating-point format with 2 components (red, green).
        /// </summary>
        R32G32Float = 0x10,

        /// <summary>
        /// 64-bit unsigned integer format with 2 components (red, green).
        /// </summary>
        R32G32UInt = 17,

        /// <summary>
        /// 64-bit signed integer format with 2 components (red, green).
        /// </summary>
        R32G32SInt = 18,

        /// <summary>
        /// 32-bit typeless format with 32 bits for depth and 8 bits for stencil.
        /// </summary>
        R32G8X24Typeless = 19,

        /// <summary>
        /// 32-bit floating-point format with 32 bits for depth and 8 bits for stencil.
        /// </summary>
        D32FloatS8X24UInt = 20,

        /// <summary>
        /// 32-bit typeless format with 32 bits for depth and 8 bits for stencil.
        /// </summary>
        R32FloatX8X24Typeless = 21,

        /// <summary>
        /// 32-bit typeless format with 8 bits for stencil and 32 bits for depth.
        /// </summary>
        X32TypelessG8X24UInt = 22,

        /// <summary>
        /// 32-bit typeless format with 10 bits for each color channel and 2 bits for alpha.
        /// </summary>
        R10G10B10A2Typeless = 23,

        /// <summary>
        /// 32-bit normalized format with 10 bits for each color channel and 2 bits for alpha.
        /// </summary>
        R10G10B10A2UNorm = 24,

        /// <summary>
        /// 32-bit unsigned integer format with 10 bits for each color channel and 2 bits for alpha.
        /// </summary>
        R10G10B10A2UInt = 25,

        /// <summary>
        /// 32-bit floating-point format with 10 bits for red and green channels and 11 bits for blue.
        /// </summary>
        R11G11B10Float = 26,

        /// <summary>
        /// 32-bit typeless format with 8 bits for each color channel and alpha.
        /// </summary>
        R8G8B8A8Typeless = 27,

        /// <summary>
        /// 32-bit normalized format with 8 bits for each color channel and alpha.
        /// </summary>
        R8G8B8A8UNorm = 28,

        /// <summary>
        /// 32-bit normalized sRGB format with 8 bits for each color channel and alpha.
        /// </summary>
        R8G8B8A8UNormSRGB = 29,

        /// <summary>
        /// 32-bit unsigned integer format with 8 bits for each color channel and alpha.
        /// </summary>
        R8G8B8A8UInt = 30,

        /// <summary>
        /// 32-bit normalized format with 8 bits for each color channel and alpha.
        /// </summary>
        R8G8B8A8SNorm = 0x1F,

        /// <summary>
        /// 32-bit signed integer format with 8 bits for each color channel and alpha.
        /// </summary>
        R8G8B8A8SInt = 0x20,

        /// <summary>
        /// 32-bit typeless format with 2 components (red, green).
        /// </summary>
        R16G16Typeless = 33,

        /// <summary>
        /// 32-bit floating-point format with 2 components (red, green).
        /// </summary>
        R16G16Float = 34,

        /// <summary>
        /// 32-bit normalized format with 2 components (red, green).
        /// </summary>
        R16G16UNorm = 35,

        /// <summary>
        /// 32-bit unsigned integer format with 2 components (red, green).
        /// </summary>
        R16G16UInt = 36,

        /// <summary>
        /// 32-bit normalized format with 2 components (red, green).
        /// </summary>
        R16G16SNorm = 37,

        /// <summary>
        /// 32-bit signed integer format with 2 components (red, green).
        /// </summary>
        R16G16Sint = 38,

        /// <summary>
        /// 32-bit typeless format.
        /// </summary>
        R32Typeless = 39,

        /// <summary>
        /// 32-bit floating-point format.
        /// </summary>
        D32Float = 40,

        /// <summary>
        /// 32-bit floating-point format.
        /// </summary>
        R32Float = 41,

        /// <summary>
        /// 32-bit unsigned integer format.
        /// </summary>
        R32UInt = 42,

        /// <summary>
        /// 32-bit signed integer format.
        /// </summary>
        R32SInt = 43,

        /// <summary>
        /// 64-bit typeless format with 24 bits for red and 8 bits for stencil.
        /// </summary>
        R24G8Typeless = 44,

        /// <summary>
        /// 24-bit normalized format for depth with 8 bits for stencil.
        /// </summary>
        D24UNormS8UInt = 45,

        /// <summary>
        /// 24-bit normalized format for depth with 8 bits for reserved data.
        /// </summary>
        R24UNormX8Typeless = 46,

        /// <summary>
        /// 24-bit typeless format with 8 bits for stencil.
        /// </summary>
        X24TypelessG8UInt = 47,

        /// <summary>
        /// 16-bit typeless format with 2 components (red, green).
        /// </summary>
        R8G8Typeless = 48,

        /// <summary>
        /// 16-bit normalized format with 2 components (red, green).
        /// </summary>
        R8G8UNorm = 49,

        /// <summary>
        /// 16-bit unsigned integer format with 2 components (red, green).
        /// </summary>
        R8G8UInt = 50,

        /// <summary>
        /// 16-bit normalized format with 2 components (red, green).
        /// </summary>
        R8G8SNorm = 51,

        /// <summary>
        /// 16-bit signed integer format with 2 components (red, green).
        /// </summary>
        R8G8Sint = 52,

        /// <summary>
        /// 16-bit typeless format.
        /// </summary>
        R16Typeless = 53,

        /// <summary>
        /// 16-bit floating-point format.
        /// </summary>
        R16Float = 54,

        /// <summary>
        /// 16-bit normalized format for depth.
        /// </summary>
        D16UNorm = 55,

        /// <summary>
        /// 16-bit normalized format.
        /// </summary>
        R16UNorm = 56,

        /// <summary>
        /// 16-bit unsigned integer format.
        /// </summary>
        R16UInt = 57,

        /// <summary>
        /// 16-bit normalized format.
        /// </summary>
        R16SNorm = 58,

        /// <summary>
        /// 16-bit signed integer format.
        /// </summary>
        R16Sint = 59,

        /// <summary>
        /// 8-bit typeless format.
        /// </summary>
        R8Typeless = 60,

        /// <summary>
        /// 8-bit normalized format.
        /// </summary>
        R8UNorm = 61,

        /// <summary>
        /// 8-bit unsigned integer format.
        /// </summary>
        R8UInt = 62,

        /// <summary>
        /// 8-bit normalized format.
        /// </summary>
        R8SNorm = 0x3F,

        /// <summary>
        /// 8-bit signed integer format.
        /// </summary>
        R8SInt = 0x40,

        /// <summary>
        /// 8-bit normalized format.
        /// </summary>
        A8UNorm = 65,

        /// <summary>
        /// 1-bit normalized format.
        /// </summary>
        R1UNorm = 66,

        /// <summary>
        /// 32-bit format with shared exponent for red, green, blue, and 5 bits for exponent.
        /// </summary>
        R9G9B9E5SharedExp = 67,

        /// <summary>
        /// 32-bit normalized format with 8 bits for each color channel.
        /// </summary>
        R8G8B8G8UNorm = 68,

        /// <summary>
        /// 32-bit normalized format with 8 bits for each color channel.
        /// </summary>
        G8R8G8B8UNorm = 69,

        /// <summary>
        /// Typeless block-compressed format.
        /// </summary>
        BC1Typeless = 70,

        /// <summary>
        /// Block-compressed format with 1-bit alpha and 5-bit color channels (UNorm).
        /// </summary>
        BC1UNorm = 71,

        /// <summary>
        /// Block-compressed format with 1-bit alpha and 5-bit color channels (UNorm SRGB).
        /// </summary>
        BC1UNormSRGB = 72,

        /// <summary>
        /// Typeless block-compressed format with 4-bit alpha and 4-bit color channels.
        /// </summary>
        BC2Typeless = 73,

        /// <summary>
        /// Block-compressed format with 4-bit alpha and 4-bit color channels (UNorm).
        /// </summary>
        BC2UNorm = 74,

        /// <summary>
        /// Block-compressed format with 4-bit alpha and 4-bit color channels (UNorm SRGB).
        /// </summary>
        BC2UNormSRGB = 75,

        /// <summary>
        /// Typeless block-compressed format with 8-bit alpha.
        /// </summary>
        BC3Typeless = 76,

        /// <summary>
        /// Block-compressed format with 8-bit alpha (UNorm).
        /// </summary>
        BC3UNorm = 77,

        /// <summary>
        /// Block-compressed format with 8-bit alpha (UNorm SRGB).
        /// </summary>
        BC3UNormSRGB = 78,

        /// <summary>
        /// Typeless block-compressed format with 4-bit alpha.
        /// </summary>
        BC4Typeless = 79,

        /// <summary>
        /// Block-compressed format with 4-bit alpha (UNorm).
        /// </summary>
        BC4UNorm = 80,

        /// <summary>
        /// Block-compressed format with 4-bit alpha (SNorm).
        /// </summary>
        BC4SNorm = 81,

        /// <summary>
        /// Typeless block-compressed format with 8-bit alpha.
        /// </summary>
        BC5Typeless = 82,

        /// <summary>
        /// Block-compressed format with 8-bit alpha (UNorm).
        /// </summary>
        BC5UNorm = 83,

        /// <summary>
        /// Block-compressed format with 8-bit alpha (SNorm).
        /// </summary>
        BC5SNorm = 84,

        /// <summary>
        /// 16-bit normalized format with 5 bits for blue, 6 bits for green, and 5 bits for red channels.
        /// </summary>
        B5G6R5UNorm = 85,

        /// <summary>
        /// 16-bit normalized format with 1 bit for alpha, 5 bits for blue, 5 bits for green, and 5 bits for red channels.
        /// </summary>
        B5G5R5A1UNorm = 86,

        /// <summary>
        /// 32-bit normalized format with 8 bits for each channel (blue, green, red, alpha).
        /// </summary>
        B8G8R8A8UNorm = 87,

        /// <summary>
        /// 32-bit normalized format with 8 bits for each channel (blue, green, red), no alpha.
        /// </summary>
        B8G8R8X8UNorm = 88,

        /// <summary>
        /// 32-bit format with 10 bits for red, green, blue channels, and 2 bits for bias alpha channel (UNorm).
        /// </summary>
        R10G10B10XRBiasA2UNorm = 89,

        /// <summary>
        /// Typeless block-compressed format with 8 bits for each channel (blue, green, red, alpha).
        /// </summary>
        B8G8R8A8Typeless = 90,

        /// <summary>
        /// Block-compressed format with 8 bits for each channel (blue, green, red, alpha) (UNorm SRGB).
        /// </summary>
        B8G8R8A8UNormSRGB = 91,

        /// <summary>
        /// Typeless block-compressed format with 8 bits for each channel (blue, green, red), no alpha.
        /// </summary>
        B8G8R8X8Typeless = 92,

        /// <summary>
        /// Block-compressed format with 8 bits for each channel (blue, green, red), no alpha (UNorm SRGB).
        /// </summary>
        B8G8R8X8UNormSRGB = 93,

        /// <summary>
        /// Typeless block-compressed format for high dynamic range (HDR) images.
        /// </summary>
        BC6HTypeless = 94,

        /// <summary>
        /// Block-compressed format for high dynamic range (HDR) images with unsigned float 16-bit per channel.
        /// </summary>
        BC6HUF16 = 95,

        /// <summary>
        /// Block-compressed format for high dynamic range (HDR) images with signed float 16-bit per channel.
        /// </summary>
        BC6HSF16 = 96,

        /// <summary>
        /// Typeless block-compressed format.
        /// </summary>
        BC7Typeless = 97,

        /// <summary>
        /// Block-compressed format with 8 bits per channel (UNorm).
        /// </summary>
        BC7UNorm = 98,

        /// <summary>
        /// Block-compressed format with 8 bits per channel (UNorm SRGB).
        /// </summary>
        BC7UNormSRGB = 99,

        /// <summary>
        /// Planar 4:4:4:4 format with alpha, Y, U, and V components (UNorm).
        /// </summary>
        AYUV = 100,

        /// <summary>
        /// Planar 4:1:0 format with Y and UV components (10-bit UNorm).
        /// </summary>
        Y410 = 101,

        /// <summary>
        /// Planar 4:1:1 format with Y and UV components (16-bit UNorm).
        /// </summary>
        Y416 = 102,

        /// <summary>
        /// 4:2:0 format with a single luminance channel and a single chroma channel interleaved (NV12).
        /// </summary>
        NV12 = 103,

        /// <summary>
        /// Planar 4:2:0 format with Y, U, and V components (10-bit UNorm).
        /// </summary>
        P010 = 104,

        /// <summary>
        /// Planar 4:2:2 format with Y and UV components (16-bit UNorm).
        /// </summary>
        P016 = 105,

        /// <summary>
        /// 4:2:0 format with a single luminance channel and a single chroma channel interleaved (Opaque).
        /// </summary>
        Opaque420 = 106,

        /// <summary>
        /// Packed format with 4:2:2 chrominance and 4:2:2 luminance (YUY2).
        /// </summary>
        YUY2 = 107,

        /// <summary>
        /// Planar 4:2:2 format with Y and UV components (10-bit UNorm).
        /// </summary>
        Y210 = 108,

        /// <summary>
        /// Planar 4:2:2 format with Y and UV components (16-bit UNorm).
        /// </summary>
        Y216 = 109,

        /// <summary>
        /// 4:1:1 format with a single luminance channel and a single chroma channel interleaved (NV11).
        /// </summary>
        NV11 = 110,

        /// <summary>
        /// 4-bit format with single channel (Alpha) (4 bits).
        /// </summary>
        AI44 = 111,

        /// <summary>
        /// 4-bit format with two channels (Intensity and Alpha) (4 bits each).
        /// </summary>
        IA44 = 112,

        /// <summary>
        /// 8-bit format with a single channel (Palette) (8 bits).
        /// </summary>
        P8 = 113,

        /// <summary>
        /// 8-bit format with 8 bits of alpha and 8 bits of palette (8 bits each).
        /// </summary>
        A8P8 = 114,

        /// <summary>
        /// 4-bit format with four channels (Blue, Green, Red, Alpha) (4 bits each).
        /// </summary>
        B4G4R4A4UNorm = 115,

        /// <summary>
        /// Planar 2:0 format with Y and UV components (8-bit UNorm).
        /// </summary>
        P208 = 130,

        /// <summary>
        /// Planar 4:4:4 format with Y, U, and V components (8-bit UNorm).
        /// </summary>
        V208 = 131,

        /// <summary>
        /// Planar 4:4:4 format with Y, U, and V components (8-bit UNorm).
        /// </summary>
        V408 = 132,

        /// <summary>
        /// The format used for sampler feedback with a minimum mip level and opaque textures.
        /// </summary>
        SamplerFeedbackMinMipOpaque = 189,

        /// <summary>
        /// The format used for sampler feedback indicating mip regions used with opaque textures.
        /// </summary>
        SamplerFeedbackMipRegionUsedOpaque = 190,

        /// <summary>
        /// Force an unsigned integer format.
        /// </summary>
        ForceUInt = -1,
    }
}