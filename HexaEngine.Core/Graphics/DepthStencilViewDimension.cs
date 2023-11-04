namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents the dimension of a depth-stencil view.
    /// </summary>
    public enum DepthStencilViewDimension : int
    {
        /// <summary>
        /// The dimension of the depth-stencil view is unknown.
        /// </summary>
        Unknown = unchecked(0),

        /// <summary>
        /// The depth-stencil view is associated with a 1D texture.
        /// </summary>
        Texture1D = unchecked(1),

        /// <summary>
        /// The depth-stencil view is associated with an array of 1D textures.
        /// </summary>
        Texture1DArray = unchecked(2),

        /// <summary>
        /// The depth-stencil view is associated with a 2D texture.
        /// </summary>
        Texture2D = unchecked(3),

        /// <summary>
        /// The depth-stencil view is associated with an array of 2D textures.
        /// </summary>
        Texture2DArray = unchecked(4),

        /// <summary>
        /// The depth-stencil view is associated with a multisampled 2D texture.
        /// </summary>
        Texture2DMultisampled = unchecked(5),

        /// <summary>
        /// The depth-stencil view is associated with an array of multisampled 2D textures.
        /// </summary>
        Texture2DMultisampledArray = unchecked(6)
    }
}