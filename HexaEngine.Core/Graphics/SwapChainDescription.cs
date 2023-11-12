namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Describes a swap chain.
    /// </summary>
    public struct SwapChainDescription
    {
        /// <summary>
        /// The width of the swap chain.
        /// </summary>
        public uint Width;

        /// <summary>
        /// The height of the swap chain.
        /// </summary>
        public uint Height;

        /// <summary>
        /// The display format.
        /// </summary>
        public Format Format;

        /// <summary>
        /// Indicates whether the swap chain is stereo.
        /// </summary>
        public bool Stereo;

        /// <summary>
        /// The sample description.
        /// </summary>
        public SampleDescription SampleDesc;

        /// <summary>
        /// The buffer usage flags.
        /// </summary>
        public uint BufferUsage;

        /// <summary>
        /// The number of back buffers in the swap chain.
        /// </summary>
        public uint BufferCount;

        /// <summary>
        /// The scaling mode of the swap chain.
        /// </summary>
        public Scaling Scaling;

        /// <summary>
        /// The swap effect.
        /// </summary>
        public SwapEffect SwapEffect;

        /// <summary>
        /// The alpha mode of the swap chain.
        /// </summary>
        public SwapChainAlphaMode AlphaMode;

        /// <summary>
        /// Flags for the swap chain.
        /// </summary>
        public SwapChainFlags Flags;
    }
}