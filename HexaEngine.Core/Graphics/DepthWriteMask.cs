namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Specifies whether and how to modify the depth component of a depth-stencil buffer.
    /// </summary>
    public enum DepthWriteMask : int
    {
        /// <summary>
        /// Indicates that no modifications should be made to the depth component.
        /// </summary>
        Zero = unchecked(0),

        /// <summary>
        /// Indicates that all modifications to the depth component should be allowed.
        /// </summary>
        All = unchecked(1)
    }
}