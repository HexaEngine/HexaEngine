namespace HexaEngine.Configuration
{
    /// <summary>
    /// Specifies the mode for generating soft shadows.
    /// </summary>
    public enum SoftShadowMode
    {
        /// <summary>
        /// No soft shadows are applied.
        /// </summary>
        None,

        /// <summary>
        /// Applies Percentage Closer Filtering (PCF) for soft shadows.
        /// </summary>
        PCF,

        /// <summary>
        /// Applies Percentage Closer Soft Shadows (PCSS) for soft shadows.
        /// </summary>
        PCSS,

        /// <summary>
        /// Applies Exponential Shadow Maps (ESM) for soft shadows. (Needs pre-filtering)
        /// </summary>
        ESM,

        /// <summary>
        /// Applies Variance Shadow Maps (VSM) for soft shadows. (Needs pre-filtering)
        /// </summary>
        VSM,

        /// <summary>
        /// Applies Exponential Variance Shadow Maps (EVSM) for soft shadows. (Needs pre-filtering)
        /// </summary>
        EVSM,

        /// <summary>
        /// Applies Summed Area Variance Shadow Maps (SAVSM) for soft shadows. (Needs pre-filtering)
        /// </summary>
        SAVSM,

        /// <summary>
        /// Applies Moment Shadow Maps (MSM) for soft shadows. (Needs pre-filtering)
        /// </summary>
        MSM,
    }
}