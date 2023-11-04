namespace HexaEngine.Core.Input
{
    /// <summary>
    /// Enumerates the different types of touch input devices.
    /// </summary>
    public enum TouchDeviceType
    {
        /// <summary>
        /// Invalid or unknown touch device type.
        /// </summary>
        Invalid = -1,

        /// <summary>
        /// A direct touch input device that provides absolute touch coordinates.
        /// </summary>
        Direct = 0,

        /// <summary>
        /// An indirect touch input device that provides absolute touch coordinates.
        /// </summary>
        IndirectAbsolute = 1,

        /// <summary>
        /// An indirect touch input device that provides relative touch coordinates.
        /// </summary>
        IndirectRelative = 2
    }
}