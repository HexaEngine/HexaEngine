namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Specifies the scanline order for an swapchain.
    /// </summary>
    public enum ModeScanlineOrder
    {
        /// <summary>
        /// The scanline order is unspecified.
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// The operation uses a progressive scanline order.
        /// </summary>
        Progressive = 1,

        /// <summary>
        /// The operation uses an upper field first scanline order.
        /// </summary>
        UpperFieldFirst = 2,

        /// <summary>
        /// The operation uses a lower field first scanline order.
        /// </summary>
        LowerFieldFirst = 3
    }
}