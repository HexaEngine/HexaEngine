namespace HexaEngine.Mathematics
{
    /// <summary>
    /// Enum representing different viewport scaling modes.
    /// </summary>
    public enum ViewportScaling
    {
        /// <summary>
        /// Stretch the content to fill the entire viewport, possibly distorting the aspect ratio.
        /// </summary>
        Stretch,

        /// <summary>
        /// Display the content in its original size, without any scaling.
        /// </summary>
        None,

        /// <summary>
        /// Stretch the content to fit the viewport while maintaining the original aspect ratio.
        /// </summary>
        AspectRatioStretch
    }
}