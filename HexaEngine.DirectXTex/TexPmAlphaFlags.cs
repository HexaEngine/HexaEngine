namespace HexaEngine.DirectXTex
{
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
}