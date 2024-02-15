namespace HexaEngine.Core.IO.Binary.Materials
{
    /// <summary>
    /// Enumeration representing different texture flags.
    /// </summary>
    public enum TextureFlags
    {
        /// <summary>
        /// No texture flags.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Inverts the texture.
        /// </summary>
        Invert = 0x1,

        /// <summary>
        /// Uses alpha channel in the texture.
        /// </summary>
        UseAlpha = 0x2,

        /// <summary>
        /// Ignores alpha channel in the texture.
        /// </summary>
        IgnoreAlpha = 0x4
    }
}