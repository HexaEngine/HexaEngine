namespace HexaEngine.Configuration
{
    /// <summary>
    /// Specifies the pre-filtering mode for shadows.
    /// </summary>
    public enum ShadowPreFilterMode
    {
        /// <summary>
        /// No pre-filtering is applied to shadows.
        /// </summary>
        None,

        /// <summary>
        /// Applies multisampling pre-filtering to shadows.
        /// </summary>
        Multisampling,

        /// <summary>
        /// Applies Gaussian pre-filtering to shadows.
        /// </summary>
        Gaussian,
    }
}