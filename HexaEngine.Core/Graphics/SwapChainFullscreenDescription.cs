namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;
    using System;

    /// <summary>
    /// Describes the fullscreen parameters for a swap chain.
    /// </summary>
    public struct SwapChainFullscreenDescription : IEquatable<SwapChainFullscreenDescription>
    {
        /// <summary>
        /// The refresh rate of the display in Hz.
        /// </summary>
        public Rational RefreshRate;

        /// <summary>
        /// The scanline ordering of the display.
        /// </summary>
        public ModeScanlineOrder ScanlineOrdering;

        /// <summary>
        /// The scaling mode of the display.
        /// </summary>
        public ModeScaling Scaling;

        /// <summary>
        /// Indicates whether the swap chain is in windowed mode.
        /// </summary>
        public bool Windowed;

        public override readonly bool Equals(object? obj)
        {
            return obj is SwapChainFullscreenDescription description && Equals(description);
        }

        public readonly bool Equals(SwapChainFullscreenDescription other)
        {
            return RefreshRate.Equals(other.RefreshRate) &&
                   ScanlineOrdering == other.ScanlineOrdering &&
                   Scaling == other.Scaling &&
                   Windowed == other.Windowed;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(RefreshRate, ScanlineOrdering, Scaling, Windowed);
        }

        public static bool operator ==(SwapChainFullscreenDescription left, SwapChainFullscreenDescription right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SwapChainFullscreenDescription left, SwapChainFullscreenDescription right)
        {
            return !(left == right);
        }
    }
}