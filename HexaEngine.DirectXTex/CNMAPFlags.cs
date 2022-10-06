namespace HexaEngine.DirectXTex
{
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
}