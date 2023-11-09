namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents an image with information about its properties and access to pixel data.
    /// </summary>
    public unsafe interface IImage
    {
        /// <summary>
        /// Gets the width of the image in pixels.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the height of the image in pixels.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets the format of the image data.
        /// </summary>
        public Format Format { get; }

        /// <summary>
        /// Gets the row pitch (number of bytes per row) of the image data.
        /// </summary>
        public int RowPitch { get; }

        /// <summary>
        /// Gets the slice pitch (number of bytes per slice) of the image data.
        /// </summary>
        public int SlicePitch { get; }

        /// <summary>
        /// Gets a pointer to the pixel data of the image.
        /// </summary>
        public byte* Pixels { get; }
    }
}