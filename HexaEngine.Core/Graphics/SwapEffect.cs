namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Describes the presentation model of the swap chain.
    /// </summary>
    public enum SwapEffect
    {
        /// <summary>
        /// Discard the previous contents of the back buffer.
        /// </summary>
        Discard = 0,

        /// <summary>
        /// Use a sequential buffer presentation model.
        /// </summary>
        Sequential = 1,

        /// <summary>
        /// Use a flip sequential buffer presentation model.
        /// </summary>
        FlipSequential = 3,

        /// <summary>
        /// Use a flip discard buffer presentation model.
        /// </summary>
        FlipDiscard = 4
    }
}