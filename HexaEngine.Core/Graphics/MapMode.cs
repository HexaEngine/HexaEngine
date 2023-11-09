namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Flags for resource mapping types.
    /// </summary>
    [Flags]
    public enum MapMode : int
    {
        /// <summary>
        /// Read access.
        /// </summary>
        Read = 0x1,

        /// <summary>
        /// Write access.
        /// </summary>
        Write = 0x2,

        /// <summary>
        /// Read and write access.
        /// </summary>
        ReadWrite = 0x3,

        /// <summary>
        /// Write access with discard of previous contents.
        /// </summary>
        WriteDiscard = 0x4,

        /// <summary>
        /// Write access without discarding previous contents.
        /// </summary>
        WriteNoOverwrite = 0x5
    }
}