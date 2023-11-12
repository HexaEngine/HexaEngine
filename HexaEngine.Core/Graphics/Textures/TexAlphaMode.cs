namespace HexaEngine.Core.Graphics.Textures
{
    /// <summary>
    /// Matches DDS_ALPHA_MODE, encoded in MISC_FLAGS2
    /// </summary>
    public enum TexAlphaMode
    {
        /// <summary>
        /// The alpha mode is unknown or unspecified.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Straight alpha, where alpha values represent premultiplied color values.
        /// </summary>
        Straight = 1,

        /// <summary>
        /// Premultiplied alpha, where color values are premultiplied by the alpha values.
        /// </summary>
        Premultiplied = 2,

        /// <summary>
        /// Opaque alpha, where alpha values are ignored, and the texture is treated as fully opaque.
        /// </summary>
        Opaque = 3,

        /// <summary>
        /// Custom alpha mode, for cases not covered by the standard modes.
        /// </summary>
        Custom = 4,
    }
}