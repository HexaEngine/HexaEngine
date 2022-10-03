namespace HexaEngine.DirectXTex
{
    [Flags]
    public enum CMSEFlags
    {
        DEFAULT = 0,

        /// <summary>
        /// Indicates that image needs gamma correction before comparision
        /// </summary>
        IMAGE1_SRGB = 0x1,

        /// <summary>
        /// Indicates that image needs gamma correction before comparision
        /// </summary>
        IMAGE2_SRGB = 0x2,

        /// <summary>
        /// Ignore the channel when computing MSE
        /// </summary>
        IGNORE_RED = 0x10,

        /// <summary>
        /// Ignore the channel when computing MSE
        /// </summary>
        IGNORE_GREEN = 0x20,

        /// <summary>
        /// Ignore the channel when computing MSE
        /// </summary>
        IGNORE_BLUE = 0x40,

        /// <summary>
        /// Ignore the channel when computing MSE
        /// </summary>
        IGNORE_ALPHA = 0x80,

        /// <summary>
        /// Indicates that image should be scaled and biased before comparison (i.e. UNORM -> SNORM)
        /// </summary>
        IMAGE1_X2_BIAS = 0x100,

        /// <summary>
        /// Indicates that image should be scaled and biased before comparison (i.e. UNORM -> SNORM)
        /// </summary>
        IMAGE2_X2_BIAS = 0x200,
    };
}