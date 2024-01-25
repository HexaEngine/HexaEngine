namespace HexaEngine.PostFx
{
    /// <summary>
    /// Enumerates the color spaces used in post-processing effects.
    /// </summary>
    public enum PostFxColorSpace
    {
        /// <summary>
        /// No specific color space. Used when color space is not applicable.
        /// </summary>
        None,

        /// <summary>
        /// Standard Dynamic Range color space. Typically used for non-HDR content.
        /// </summary>
        SDR,

        /// <summary>
        /// High Dynamic Range color space. Used for content with extended dynamic range, suitable for HDR displays.
        /// </summary>
        HDR
    }
}