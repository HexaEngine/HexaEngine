namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Specifies the alpha mode of the swap chain.
    /// </summary>
    public enum SwapChainAlphaMode
    {
        /// <summary>
        /// The alpha mode is unspecified.
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// The alpha channel is premultiplied.
        /// </summary>
        Premultiplied = 1,

        /// <summary>
        /// The alpha channel is straight.
        /// </summary>
        Straight = 2,

        /// <summary>
        /// The alpha mode is ignored.
        /// </summary>
        Ignore = 3,

        /// <summary>
        /// Forces the enumeration to a DWORD size.
        /// </summary>
        ForceDword = -1
    }
}