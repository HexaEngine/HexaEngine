namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Specifies fence options.
    /// </summary>
    [Flags]
    public enum FenceFlags
    {
        /// <summary>
        /// No options are specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// The fence is shared.
        /// </summary>
        Shared = 2,

        /// <summary>
        /// The fence is shared with another GPU adapter.
        /// </summary>
        SharedCrossAdapter = 4,

        /// <summary>
        /// Value: 0x8
        /// </summary>
        NonMonitored = 8
    }
}