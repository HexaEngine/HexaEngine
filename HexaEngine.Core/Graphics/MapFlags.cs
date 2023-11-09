namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Flags for mapping resources.
    /// </summary>
    [Flags]
    public enum MapFlags : int
    {
        /// <summary>
        /// Specifies that the mapping should not wait.
        /// </summary>
        DoNotWait = unchecked(1048576),

        /// <summary>
        /// No special mapping flags.
        /// </summary>
        None = unchecked(0)
    }
}