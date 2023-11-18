namespace HexaEngine.Windows
{
    /// <summary>
    /// Flags representing various rendering options for the application.
    /// </summary>
    [Flags]
    public enum RendererFlags
    {
        /// <summary>
        /// No specific rendering flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// High Dynamic Range (HDR) with 10 bits per channel.
        /// </summary>
        HDR10 = 1,

        /// <summary>
        /// High Dynamic Range (HDR) with 16 bits per channel.
        /// </summary>
        HDR16 = 2,

        /// <summary>
        /// Flag indicating the forceful use of forward rendering.
        /// </summary>
        ForceForward = 4,
    }
}