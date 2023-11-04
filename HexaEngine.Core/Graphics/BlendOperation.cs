namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Specifies different types of blending operations for color data.
    /// </summary>
    public enum BlendOperation : int
    {
        /// <summary>
        /// Add source and destination data.
        /// </summary>
        Add = unchecked(1),

        /// <summary>
        /// Subtract destination data from source data.
        /// </summary>
        Subtract = unchecked(2),

        /// <summary>
        /// Subtract source data from destination data.
        /// </summary>
        ReverseSubtract = unchecked(3),

        /// <summary>
        /// Select the minimum value between source and destination data.
        /// </summary>
        Min = unchecked(4),

        /// <summary>
        /// Select the maximum value between source and destination data.
        /// </summary>
        Max = unchecked(5)
    }
}