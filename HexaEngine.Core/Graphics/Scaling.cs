namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Enumerates different scaling options.
    /// </summary>
    public enum Scaling
    {
        /// <summary>
        /// Stretch the content to fill the target area.
        /// </summary>
        Stretch = 0,

        /// <summary>
        /// Do not apply any scaling to the content.
        /// </summary>
        None = 1,

        /// <summary>
        /// Stretch the content while preserving its aspect ratio.
        /// </summary>
        AspectRatioStretch = 2
    }
}