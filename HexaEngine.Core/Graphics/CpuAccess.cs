namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Flags that specify GPU access rights.
    /// </summary>
    [Flags]
    public enum GpuAccessFlags : int
    {
        /// <summary>
        /// No GPU access rights.
        /// </summary>
        None = unchecked(0),

        /// <summary>
        /// Read access to the GPU. (SRV)
        /// </summary>
        Read = unchecked(131072),

        /// <summary>
        /// Write access to the GPU. (RTV)
        /// </summary>
        Write = unchecked(65536),

        /// <summary>
        /// Unordered access (UAV) to the GPU.
        /// </summary>
        UA = unchecked(262144),

        /// <summary>
        /// Depth stencil flag (DSV)
        /// </summary>
        DepthStencil = unchecked(524288),

        /// <summary>
        /// Read and write access to the GPU. (RTV, SRV)
        /// </summary>
        RW = Read | Write,

        /// <summary>
        /// All access rights to the GPU. (RTV, SRV, UAV)
        /// </summary>
        All = Read | Write | UA,
    }

    /// <summary>
    /// Flags that specify CPU access rights.
    /// </summary>
    [Flags]
    public enum CpuAccessFlags : int
    {
        /// <summary>
        /// No CPU access rights.
        /// </summary>
        None = unchecked(0),

        /// <summary>
        /// Read access to the CPU.
        /// </summary>
        Read = unchecked(131072),

        /// <summary>
        /// Write access to the CPU.
        /// </summary>
        Write = unchecked(65536),

        /// <summary>
        /// Read and write access to the CPU.
        /// </summary>
        RW = Read | Write,
    }
}