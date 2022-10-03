namespace HexaEngine.DirectXTex
{
    [Flags]
    public enum DDSFlags
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
}