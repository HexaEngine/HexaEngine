namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Specifies flags that control the behavior of a depth-stencil view.
    /// </summary>
    [Flags]
    public enum DepthStencilViewFlags : int
    {
        /// <summary>
        /// The depth-stencil view allows only read access to the depth component.
        /// </summary>
        ReadOnlyDepth = unchecked(1),

        /// <summary>
        /// The depth-stencil view allows only read access to the stencil component.
        /// </summary>
        ReadOnlyStencil = unchecked(2),

        /// <summary>
        /// The depth-stencil view has no special flags set.
        /// </summary>
        None = unchecked(0)
    }
}