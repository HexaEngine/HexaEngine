namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Specifies the dimension of a texture in graphics programming.
    /// </summary>
    public enum TextureDimension : int
    {
        /// <summary>
        /// The dimension of the texture is unknown.
        /// </summary>
        Unknown = unchecked(0),

        /// <summary>
        /// The texture has a 1D dimension.
        /// </summary>
        Texture1D = unchecked(1),

        /// <summary>
        /// The texture has a 2D dimension.
        /// </summary>
        Texture2D = unchecked(2),

        /// <summary>
        /// The texture has a 3D dimension.
        /// </summary>
        Texture3D = unchecked(3),

        /// <summary>
        /// The texture is a cube texture.
        /// </summary>
        TextureCube = unchecked(4)
    }
}