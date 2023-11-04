namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Defines comparison functions used in depth and stencil operations.
    /// </summary>
    public enum ComparisonFunction : int
    {
        /// <summary>
        /// Always return false. No comparison is made.
        /// </summary>
        Never = unchecked(1),

        /// <summary>
        /// Return true if the source data is less than the destination data.
        /// </summary>
        Less = unchecked(2),

        /// <summary>
        /// Return true if the source data is equal to the destination data.
        /// </summary>
        Equal = unchecked(3),

        /// <summary>
        /// Return true if the source data is less than or equal to the destination data.
        /// </summary>
        LessEqual = unchecked(4),

        /// <summary>
        /// Return true if the source data is greater than the destination data.
        /// </summary>
        Greater = unchecked(5),

        /// <summary>
        /// Return true if the source data is not equal to the destination data.
        /// </summary>
        NotEqual = unchecked(6),

        /// <summary>
        /// Return true if the source data is greater than or equal to the destination data.
        /// </summary>
        GreaterEqual = unchecked(7),

        /// <summary>
        /// Always return true. No comparison is made, and all pixels pass.
        /// </summary>
        Always = unchecked(8)
    }
}