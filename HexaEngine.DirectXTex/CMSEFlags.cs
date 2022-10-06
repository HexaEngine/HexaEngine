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
}