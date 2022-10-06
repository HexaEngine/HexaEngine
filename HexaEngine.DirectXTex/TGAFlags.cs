namespace HexaEngine.DirectXTex
{
    [Flags]
    public enum TGAFlags : ulong
    {
        TGA_FLAGS_NONE = 0x0,

        /// <summary>
        /// 24bpp files are returned as BGRX; 32bpp files are returned as BGRA
        /// </summary>
        TGA_FLAGS_BGR = 0x1,

        /// <summary>
        /// If the loaded image has an all zero alpha channel, normally we assume it should be opaque. This flag leaves it alone.
        /// </summary>
        TGA_FLAGS_ALLOW_ALL_ZERO_ALPHA = 0x2,

        /// <summary>
        /// Ignores sRGB TGA 2.0 metadata if present in the file
        /// </summary>
        TGA_FLAGS_IGNORE_SRGB = 0x10,

        /// <summary>
        /// Writes sRGB metadata into the file reguardless of format (TGA 2.0 only)
        /// </summary>
        TGA_FLAGS_FORCE_SRGB = 0x20,

        /// <summary>
        /// Writes linear gamma metadata into the file reguardless of format (TGA 2.0 only)
        /// </summary>
        TGA_FLAGS_FORCE_LINEAR = 0x40,

        /// <summary>
        /// If no colorspace is specified in TGA 2.0 metadata, assume sRGB
        /// </summary>
        TGA_FLAGS_DEFAULT_SRGB = 0x80,
    }
}