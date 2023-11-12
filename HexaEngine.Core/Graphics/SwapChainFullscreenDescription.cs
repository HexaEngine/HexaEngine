namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;

    /// <summary>
    /// Describes the fullscreen parameters for a swap chain.
    /// </summary>
    public struct SwapChainFullscreenDescription
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
    }
}