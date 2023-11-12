namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents the dimension of a shader resource view.
    /// </summary>
    public enum ShaderResourceViewDimension : int
    {
        /// <summary>
        /// Unknown dimension.
        /// </summary>
        Unknown = unchecked(0),

        /// <summary>
        /// Buffer dimension.
        /// </summary>
        Buffer = unchecked(1),

        /// <summary>
        /// 1D texture dimension.
        /// </summary>
        Texture1D = unchecked(2),

        /// <summary>
        /// 1D texture array dimension.
        /// </summary>
        Texture1DArray = unchecked(3),

        /// <summary>
        /// 2D texture dimension.
        /// </summary>
        Texture2D = unchecked(4),

        /// <summary>
        /// 2D texture array dimension.
        /// </summary>
        Texture2DArray = unchecked(5),

        /// <summary>
        /// 2D multisampled texture dimension.
        /// </summary>
        Texture2DMultisampled = unchecked(6),

        /// <summary>
        /// 2D multisampled texture array dimension.
        /// </summary>
        Texture2DMultisampledArray = unchecked(7),

        /// <summary>
        /// 3D texture dimension.
        /// </summary>
        Texture3D = unchecked(8),

        /// <summary>
        /// Cube texture dimension.
        /// </summary>
        TextureCube = unchecked(9),

        /// <summary>
        /// Cube texture array dimension.
        /// </summary>
        TextureCubeArray = unchecked(10),

        /// <summary>
        /// Extended buffer dimension.
        /// </summary>
        BufferExtended = unchecked(11)
    }
}