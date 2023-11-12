namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Defines stencil operations.
    /// </summary>
    public enum StencilOperation : int
    {
        /// <summary>
        /// Keep the existing stencil value.
        /// </summary>
        Keep = unchecked(1),

        /// <summary>
        /// Set the stencil value to zero.
        /// </summary>
        Zero = unchecked(2),

        /// <summary>
        /// Replace the stencil value with the reference value.
        /// </summary>
        Replace = unchecked(3),

        /// <summary>
        /// Increment the stencil value, clamping to the maximum representable unsigned value.
        /// </summary>
        IncrementSaturate = unchecked(4),

        /// <summary>
        /// Decrement the stencil value, clamping to zero.
        /// </summary>
        DecrementSaturate = unchecked(5),

        /// <summary>
        /// Invert the bits of the stencil value.
        /// </summary>
        Invert = unchecked(6),

        /// <summary>
        /// Increment the stencil value, wrapping to zero when the maximum value is reached.
        /// </summary>
        Increment = unchecked(7),

        /// <summary>
        /// Decrement the stencil value, wrapping to the maximum representable unsigned value when zero is reached.
        /// </summary>
        Decrement = unchecked(8)
    }
}