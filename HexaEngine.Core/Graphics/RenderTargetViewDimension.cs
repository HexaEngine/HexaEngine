namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Identifies the dimension of a render target view.
    /// </summary>
    public enum RenderTargetViewDimension : int
    {
        /// <summary>
        /// Unknown dimension.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Render target view for a buffer.
        /// </summary>
        Buffer = 1,

        /// <summary>
        /// Render target view for a 1D texture.
        /// </summary>
        Texture1D = 2,

        /// <summary>
        /// Render target view for a 1D texture array.
        /// </summary>
        Texture1DArray = 3,

        /// <summary>
        /// Render target view for a 2D texture.
        /// </summary>
        Texture2D = 4,

        /// <summary>
        /// Render target view for a 2D texture array.
        /// </summary>
        Texture2DArray = 5,

        /// <summary>
        /// Render target view for a 2D multisampled texture.
        /// </summary>
        Texture2DMultisampled = 6,

        /// <summary>
        /// Render target view for a 2D multisampled texture array.
        /// </summary>
        Texture2DMultisampledArray = 7,

        /// <summary>
        /// Render target view for a 3D texture.
        /// </summary>
        Texture3D = 8
    }
}