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
        /// Read access to the GPU.
        /// </summary>
        Read = unchecked(131072),

        /// <summary>
        /// Write access to the GPU.
        /// </summary>
        Write = unchecked(65536),

        /// <summary>
        /// Unordered access (UA) to the GPU.
        /// </summary>
        UA = unchecked(262144),

        /// <summary>
        /// Read and write access to the GPU.
        /// </summary>
        RW = Read | Write,

        /// <summary>
        /// All access rights to the GPU.
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