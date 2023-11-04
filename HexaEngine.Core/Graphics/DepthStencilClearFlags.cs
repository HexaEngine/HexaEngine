namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Flags used to specify which parts of a depth stencil buffer should be cleared when performing a clear operation.
    /// </summary>
    [Flags]
    public enum DepthStencilClearFlags
    {
        /// <summary>
        /// No parts of the depth stencil buffer will be cleared.
        /// </summary>
        None = 0,

        /// <summary>
        /// Clear the depth portion of the depth stencil buffer.
        /// </summary>
        Depth = 1,

        /// <summary>
        /// Clear the stencil portion of the depth stencil buffer.
        /// </summary>
        Stencil = 2,

        /// <summary>
        /// Clear both the depth and stencil portions of the depth stencil buffer.
        /// </summary>
        All = Depth | Stencil
    }
}