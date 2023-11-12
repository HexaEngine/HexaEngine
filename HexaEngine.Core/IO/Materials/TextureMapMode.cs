namespace HexaEngine.Core.IO.Materials
{
    /// <summary>
    /// Enumeration representing different texture map modes.
    /// </summary>
    public enum TextureMapMode
    {
        /// <summary>
        /// Wraps the texture coordinates.
        /// </summary>
        Wrap = 0x0,

        /// <summary>
        /// Clamps the texture coordinates.
        /// </summary>
        Clamp = 0x1,

        /// <summary>
        /// Mirrors the texture coordinates.
        /// </summary>
        Mirror = 0x2,

        /// <summary>
        /// Applies decal mapping.
        /// </summary>
        Decal = 0x3,
    }
}