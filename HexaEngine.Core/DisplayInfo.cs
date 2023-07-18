namespace HexaEngine.Core
{
    /// <summary>
    /// Represents a display mode, which specifies the format, width, height, refresh rate, and driver data of a display.
    /// </summary>
    public struct DisplayMode
    {
        /// <summary>
        /// The pixel format of the display mode.
        /// </summary>
        public uint Format;

        /// <summary>
        /// The width of the display mode in pixels.
        /// </summary>
        public int W;

        /// <summary>
        /// The height of the display mode in pixels.
        /// </summary>
        public int H;

        /// <summary>
        /// The refresh rate of the display mode in Hz.
        /// </summary>
        public int RefreshRate;

        /// <summary>
        /// A pointer to driver-specific data associated with the display mode.
        /// </summary>
        public unsafe void* DriverData;
    }
}