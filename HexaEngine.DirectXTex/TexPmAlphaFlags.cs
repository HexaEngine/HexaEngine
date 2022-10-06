namespace HexaEngine.DirectXTex
{
    [Flags]
    public enum TexPmAlphaFlags
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
        SRGB = SRGB_IN | SRGB_OUT
    }
}