namespace HexaEngine.Core
{
    public enum DisplayPixelFormat
    {
        Unknown = 0,

        Index1Lsb = 0x11100100,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_INDEX1, SDL_BITMAPORDER_4321, 0, 1, 0),
        Index1Msb = 0x11200100,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_INDEX1, SDL_BITMAPORDER_1234, 0, 1, 0),
        Index2Lsb = 0x1C100200,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_INDEX2, SDL_BITMAPORDER_4321, 0, 2, 0),
        Index2Msb = 0x1C200200,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_INDEX2, SDL_BITMAPORDER_1234, 0, 2, 0),
        Index4Lsb = 0x12100400,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_INDEX4, SDL_BITMAPORDER_4321, 0, 4, 0),
        Index4Msb = 0x12200400,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_INDEX4, SDL_BITMAPORDER_1234, 0, 4, 0),
        Index8 = 0x13000801,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_INDEX8, 0, 0, 8, 1),
        Rgb332 = 0x14110801,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED8, SDL_PACKEDORDER_XRGB, SDL_PACKEDLAYOUT_332,
        //     8, 1),
        Xrgb4444 = 0x15120C02,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED16, SDL_PACKEDORDER_XRGB, SDL_PACKEDLAYOUT_4444,
        //     12, 2),
        Xbgr4444 = 0x15520C02,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED16, SDL_PACKEDORDER_XBGR, SDL_PACKEDLAYOUT_4444,
        //     12, 2),
        Xrgb1555 = 0x15130F02,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED16, SDL_PACKEDORDER_XRGB, SDL_PACKEDLAYOUT_1555,
        //     15, 2),
        Xbgr1555 = 0x15530F02,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED16, SDL_PACKEDORDER_XBGR, SDL_PACKEDLAYOUT_1555,
        //     15, 2),
        Argb4444 = 0x15321002,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED16, SDL_PACKEDORDER_ARGB, SDL_PACKEDLAYOUT_4444,
        //     16, 2),
        Rgba4444 = 0x15421002,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED16, SDL_PACKEDORDER_RGBA, SDL_PACKEDLAYOUT_4444,
        //     16, 2),
        Abgr4444 = 0x15721002,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED16, SDL_PACKEDORDER_ABGR, SDL_PACKEDLAYOUT_4444,
        //     16, 2),
        Bgra4444 = 0x15821002,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED16, SDL_PACKEDORDER_BGRA, SDL_PACKEDLAYOUT_4444,
        //     16, 2),
        Argb1555 = 0x15331002,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED16, SDL_PACKEDORDER_ARGB, SDL_PACKEDLAYOUT_1555,
        //     16, 2),
        Rgba5551 = 0x15441002,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED16, SDL_PACKEDORDER_RGBA, SDL_PACKEDLAYOUT_5551,
        //     16, 2),
        Abgr1555 = 0x15731002,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED16, SDL_PACKEDORDER_ABGR, SDL_PACKEDLAYOUT_1555,
        //     16, 2),
        Bgra5551 = 0x15841002,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED16, SDL_PACKEDORDER_BGRA, SDL_PACKEDLAYOUT_5551,
        //     16, 2),
        Rgb565 = 0x15151002,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED16, SDL_PACKEDORDER_XRGB, SDL_PACKEDLAYOUT_565,
        //     16, 2),
        Bgr565 = 0x15551002,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED16, SDL_PACKEDORDER_XBGR, SDL_PACKEDLAYOUT_565,
        //     16, 2),
        Rgb24 = 0x17101803,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_ARRAYU8, SDL_ARRAYORDER_RGB, 0, 24, 3),
        Bgr24 = 0x17401803,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_ARRAYU8, SDL_ARRAYORDER_BGR, 0, 24, 3),
        Xrgb8888 = 0x16161804,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED32, SDL_PACKEDORDER_XRGB, SDL_PACKEDLAYOUT_8888,
        //     24, 4),
        Rgbx8888 = 0x16261804,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED32, SDL_PACKEDORDER_RGBX, SDL_PACKEDLAYOUT_8888,
        //     24, 4),
        Xbgr8888 = 0x16561804,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED32, SDL_PACKEDORDER_XBGR, SDL_PACKEDLAYOUT_8888,
        //     24, 4),
        Bgrx8888 = 0x16661804,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED32, SDL_PACKEDORDER_BGRX, SDL_PACKEDLAYOUT_8888,
        //     24, 4),
        Argb8888 = 0x16362004,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED32, SDL_PACKEDORDER_ARGB, SDL_PACKEDLAYOUT_8888,
        //     32, 4),
        Rgba8888 = 0x16462004,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED32, SDL_PACKEDORDER_RGBA, SDL_PACKEDLAYOUT_8888,
        //     32, 4),
        Abgr8888 = 0x16762004,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED32, SDL_PACKEDORDER_ABGR, SDL_PACKEDLAYOUT_8888,
        //     32, 4),
        Bgra8888 = 0x16862004,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED32, SDL_PACKEDORDER_BGRA, SDL_PACKEDLAYOUT_8888,
        //     32, 4),
        Xrgb2101010 = 0x16172004,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED32, SDL_PACKEDORDER_XRGB, SDL_PACKEDLAYOUT_2101010,
        //     32, 4),
        Xbgr2101010 = 0x16572004,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED32, SDL_PACKEDORDER_XBGR, SDL_PACKEDLAYOUT_2101010,
        //     32, 4),
        Argb2101010 = 0x16372004,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED32, SDL_PACKEDORDER_ARGB, SDL_PACKEDLAYOUT_2101010,
        //     32, 4),
        Abgr2101010 = 0x16772004,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_PACKED32, SDL_PACKEDORDER_ABGR, SDL_PACKEDLAYOUT_2101010,
        //     32, 4),
        Rgb48 = 0x18103006,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_ARRAYU16, SDL_ARRAYORDER_RGB, 0, 48, 6),
        Bgr48 = 0x18403006,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_ARRAYU16, SDL_ARRAYORDER_BGR, 0, 48, 6),
        Rgba64 = 0x18204008,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_ARRAYU16, SDL_ARRAYORDER_RGBA, 0, 64, 8),
        Argb64 = 0x18304008,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_ARRAYU16, SDL_ARRAYORDER_ARGB, 0, 64, 8),
        Bgra64 = 0x18504008,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_ARRAYU16, SDL_ARRAYORDER_BGRA, 0, 64, 8),
        Abgr64 = 0x18604008,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_ARRAYU16, SDL_ARRAYORDER_ABGR, 0, 64, 8),
        Rgb48Float = 0x1A103006,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_ARRAYF16, SDL_ARRAYORDER_RGB, 0, 48, 6),
        Bgr48Float = 0x1A403006,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_ARRAYF16, SDL_ARRAYORDER_BGR, 0, 48, 6),
        Rgba64Float = 0x1A204008,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_ARRAYF16, SDL_ARRAYORDER_RGBA, 0, 64, 8),
        Argb64Float = 0x1A304008,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_ARRAYF16, SDL_ARRAYORDER_ARGB, 0, 64, 8),
        Bgra64Float = 0x1A504008,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_ARRAYF16, SDL_ARRAYORDER_BGRA, 0, 64, 8),
        Abgr64Float = 0x1A604008,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_ARRAYF16, SDL_ARRAYORDER_ABGR, 0, 64, 8),
        Rgb96Float = 0x1B10600C,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_ARRAYF32, SDL_ARRAYORDER_RGB, 0, 96, 12),
        Bgr96Float = 0x1B40600C,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_ARRAYF32, SDL_ARRAYORDER_BGR, 0, 96, 12),
        Rgba128Float = 0x1B208010,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_ARRAYF32, SDL_ARRAYORDER_RGBA, 0, 128, 16),
        Argb128Float = 0x1B308010,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_ARRAYF32, SDL_ARRAYORDER_ARGB, 0, 128, 16),
        Bgra128Float = 0x1B508010,

        //
        // Summary:
        //     SDL_DEFINE_PIXELFORMAT(SDL_PIXELTYPE_ARRAYF32, SDL_ARRAYORDER_BGRA, 0, 128, 16),
        Abgr128Float = 0x1B608010,

        //
        // Summary:
        //     Planar mode: Y + V + U (3 planes)
        Yv12 = 0x32315659,

        //
        // Summary:
        //     Planar mode: Y + U + V (3 planes)
        Iyuv = 0x56555949,

        //
        // Summary:
        //     Packed mode: Y0+U0+Y1+V0 (1 plane)
        Yuy2 = 0x32595559,

        //
        // Summary:
        //     Packed mode: U0+Y0+V0+Y1 (1 plane)
        Uyvy = 0x59565955,

        //
        // Summary:
        //     Packed mode: Y0+V0+Y1+U0 (1 plane)
        Yvyu = 0x55595659,

        //
        // Summary:
        //     Planar mode: Y + U/V interleaved (2 planes)
        Nv12 = 0x3231564E,

        //
        // Summary:
        //     Planar mode: Y + V/U interleaved (2 planes)
        Nv21 = 0x3132564E,

        //
        // Summary:
        //     Planar mode: Y + U/V interleaved (2 planes)
        P010 = 0x30313050,

        //
        // Summary:
        //     Android video texture format
        ExternalOes = 0x2053454F,

        //
        // Summary:
        //     Motion JPEG
        Mjpg = 0x47504A4D,

        Rgba32 = 0x16762004,
        Argb32 = 0x16862004,
        Bgra32 = 0x16362004,
        Abgr32 = 0x16462004,
        Rgbx32 = 0x16561804,
        Xrgb32 = 0x16661804,
        Bgrx32 = 0x16161804,
        Xbgr32 = 0x16261804
    }
}