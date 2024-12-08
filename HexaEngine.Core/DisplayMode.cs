namespace HexaEngine.Core
{
    using System;

    /// <summary>
    /// Represents a display mode, which specifies the format, width, height, refresh rate, and driver data of a display.
    /// </summary>
    public unsafe struct DisplayMode : IEquatable<DisplayMode>
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
        public void* DriverData;

        public override readonly bool Equals(object? obj)
        {
            return obj is DisplayMode mode && Equals(mode);
        }

        public readonly bool Equals(DisplayMode other)
        {
            return Format == other.Format &&
                   W == other.W &&
                   H == other.H &&
                   RefreshRate == other.RefreshRate &&
                   DriverData == other.DriverData;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Format, W, H, RefreshRate, (nint)DriverData);
        }

        public static bool operator ==(DisplayMode left, DisplayMode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DisplayMode left, DisplayMode right)
        {
            return !(left == right);
        }
    }
}