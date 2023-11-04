namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Flags that describe the behavior of an unordered access view for a buffer resource.
    /// </summary>
    [Flags]
    public enum BufferUnorderedAccessViewFlags : int
    {
        /// <summary>
        /// No special flags are applied.
        /// </summary>
        None = unchecked(0),

        /// <summary>
        /// The view is used for raw unordered access.
        /// </summary>
        Raw = unchecked(1),

        /// <summary>
        /// The view is used for append operations.
        /// </summary>
        Append = unchecked(2),

        /// <summary>
        /// The view is used to perform counter operations.
        /// </summary>
        Counter = unchecked(4),
    }
}