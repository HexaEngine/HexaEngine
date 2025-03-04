namespace HexaEngine.Core
{
    using Hexa.NET.SDL3;
    using System;

    /// <summary>
    /// Represents a display mode, which specifies the format, width, height, refresh rate, and driver data of a display.
    /// </summary>
    public unsafe struct DisplayMode : IEquatable<DisplayMode>
    {
        public uint DisplayID;

        /// <summary>
        /// The pixel format of the display mode.
        /// </summary>
        public DisplayPixelFormat Format;

        /// <summary>
        /// The width of the display mode in pixels.
        /// </summary>
        public int W;

        /// <summary>
        /// The height of the display mode in pixels.
        /// </summary>
        public int H;

        public float PixelDensity;

        /// <summary>
        /// The refresh rate of the display mode in Hz.
        /// </summary>
        public float RefreshRate;

        public int RefreshRateNumerator;

        public int RefreshRateDenominator;

        /// <summary>
        /// A pointer to driver-specific data associated with the display mode.
        /// </summary>
        private void* Internal;

        public override readonly bool Equals(object? obj)
        {
            return obj is DisplayMode mode && Equals(mode);
        }

        public readonly bool Equals(DisplayMode other)
        {
            return DisplayID == other.DisplayID &&
                   Format == other.Format &&
                   W == other.W &&
                   H == other.H &&
                   PixelDensity == other.PixelDensity &&
                   RefreshRate == other.RefreshRate &&
                   RefreshRateNumerator == other.RefreshRateNumerator &&
                   RefreshRateDenominator == other.RefreshRateDenominator &&
                   Internal == other.Internal;
        }

        public override readonly int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(DisplayID);
            hash.Add(Format);
            hash.Add(W);
            hash.Add(H);
            hash.Add(PixelDensity);
            hash.Add(RefreshRate);
            hash.Add(RefreshRateNumerator);
            hash.Add(RefreshRateDenominator);
            hash.Add((nint)Internal);
            return hash.ToHashCode();
        }

        public static bool operator ==(DisplayMode left, DisplayMode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DisplayMode left, DisplayMode right)
        {
            return !(left == right);
        }

        public static implicit operator DisplayMode(SDLDisplayMode mode)
        {
            return new()
            {
                DisplayID = mode.DisplayID,
                Format = (DisplayPixelFormat)mode.Format,
                W = mode.W,
                H = mode.H,
                PixelDensity = mode.PixelDensity,
                RefreshRate = mode.RefreshRate,
                RefreshRateNumerator = mode.RefreshRateNumerator,
                RefreshRateDenominator = mode.RefreshRateDenominator,
                Internal = mode.Internal
            };
        }

        public static implicit operator SDLDisplayMode(DisplayMode mode)
        {
            return new()
            {
                DisplayID = mode.DisplayID,
                Format = (SDLPixelFormat)mode.Format,
                W = mode.W,
                H = mode.H,
                PixelDensity = mode.PixelDensity,
                RefreshRate = mode.RefreshRate,
                RefreshRateNumerator = mode.RefreshRateNumerator,
                RefreshRateDenominator = mode.RefreshRateDenominator,
                Internal = (SDLDisplayModeData*)mode.Internal
            };
        }
    }
}