namespace HexaEngine.DirectXTex
{
    [Flags]
    public enum CNMAPFlags
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
}