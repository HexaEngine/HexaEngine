namespace HexaEngine.Core.IO.Binary.Materials
{
    /// <summary>
    /// Enumeration representing different texture map modes.
    /// </summary>
    public enum TextureMapMode
    {
        /// <summary>
        /// Tile the texture at every integer junction.
        /// </summary>
        Wrap = unchecked(1),

        /// <summary>
        /// Flip the texture at every integer junction.
        /// </summary>
        Mirror = unchecked(2),

        /// <summary>
        /// Texture coordinates outside the range [0, 1] are clamped to the border.
        /// </summary>
        Clamp = unchecked(3),

        /// <summary>
        /// Texture coordinates outside the range [0, 1] are set to the border color.
        /// </summary>
        Border = unchecked(4),

        /// <summary>
        /// Similar to <see cref="Mirror"/>, but only mirrors once.
        /// </summary>
        MirrorOnce = unchecked(5)
    }
}