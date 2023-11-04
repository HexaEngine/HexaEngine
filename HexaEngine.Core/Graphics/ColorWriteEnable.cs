namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Flags that determine which color components can be written to the render target.
    /// </summary>
    [Flags]
    public enum ColorWriteEnable : byte
    {
        /// <summary>
        /// No color components are written to the render target.
        /// </summary>
        None = unchecked(0),

        /// <summary>
        /// The red color component is allowed to be written to the render target.
        /// </summary>
        Red = unchecked(1),

        /// <summary>
        /// The green color component is allowed to be written to the render target.
        /// </summary>
        Green = unchecked(2),

        /// <summary>
        /// The blue color component is allowed to be written to the render target.
        /// </summary>
        Blue = unchecked(4),

        /// <summary>
        /// The alpha color component is allowed to be written to the render target.
        /// </summary>
        Alpha = unchecked(8),

        /// <summary>
        /// All color components (red, green, blue, and alpha) are allowed to be written to the render target.
        /// </summary>
        All = unchecked(15),
    }
}