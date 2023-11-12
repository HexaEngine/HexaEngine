namespace HexaEngine.Core.Graphics.Textures
{
    /// <summary>
    /// Specifies the file format for a texture.
    /// </summary>
    public enum TexFileFormat
    {
        /// <summary>
        /// Automatically determine the file format.
        /// </summary>
        Auto,

        /// <summary>
        /// DirectDraw Surface file format.
        /// </summary>
        DDS,

        /// <summary>
        /// Truevision Targa file format.
        /// </summary>
        TGA,

        /// <summary>
        /// High Dynamic Range file format.
        /// </summary>
        HDR,

        /// <summary>
        /// Windows Bitmap file format.
        /// </summary>
        BMP,

        /// <summary>
        /// Joint Photographic Experts Group file format.
        /// </summary>
        JPEG,

        /// <summary>
        /// Portable Network Graphics file format.
        /// </summary>
        PNG,

        /// <summary>
        /// Tagged Image File Format.
        /// </summary>
        TIFF,

        /// <summary>
        /// Graphics Interchange Format.
        /// </summary>
        GIF,

        /// <summary>
        /// Windows Media Photo file format.
        /// </summary>
        WMP,

        /// <summary>
        /// Windows Icon file format.
        /// </summary>
        ICO,

        /// <summary>
        /// Raw image data format.
        /// </summary>
        RAW
    }
}