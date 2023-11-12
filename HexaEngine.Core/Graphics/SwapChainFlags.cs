namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Defines flags for swap chain creation.
    /// </summary>
    [Flags]
    public enum SwapChainFlags
    {
        /// <summary>
        /// Represents no specific swap chain flags.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Indicates that the swap chain is non-prerotated.
        /// </summary>
        Nonprerotated = 0x1,

        /// <summary>
        /// Allows the mode switch for the swap chain.
        /// </summary>
        AllowModeSwitch = 0x2,

        /// <summary>
        /// Indicates compatibility with GDI (Graphics Device Interface).
        /// </summary>
        GdiCompatible = 0x4,

        /// <summary>
        /// Indicates restricted content usage for the swap chain.
        /// </summary>
        RestrictedContent = 0x8,

        /// <summary>
        /// Restricts the shared resource driver for the swap chain.
        /// </summary>
        RestrictSharedResourceDriver = 0x10,

        /// <summary>
        /// Indicates that the swap chain is for display only.
        /// </summary>
        DisplayOnly = 0x20,

        /// <summary>
        /// Specifies a waitable object for frame latency.
        /// </summary>
        FrameLatencyWaitableObject = 0x40,

        /// <summary>
        /// Specifies the swap chain as a foreground layer.
        /// </summary>
        ForegroundLayer = 0x80,

        /// <summary>
        /// Indicates that the swap chain is for fullscreen video playback.
        /// </summary>
        FullscreenVideo = 0x100,

        /// <summary>
        /// Indicates YUV (YCbCr color space) video support for the swap chain.
        /// </summary>
        YuvVideo = 0x200,

        /// <summary>
        /// Indicates hardware protection for the swap chain.
        /// </summary>
        HWProtected = 0x400,

        /// <summary>
        /// Allows tearing in the swap chain.
        /// </summary>
        AllowTearing = 0x800,

        /// <summary>
        /// Restricts the swap chain to all holographic displays.
        /// </summary>
        RestrictedToAllHolographicDisplays = 0x1000
    }
}