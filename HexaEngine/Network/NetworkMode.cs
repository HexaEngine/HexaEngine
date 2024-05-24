namespace HexaEngine.Network
{
    public enum NetworkMode
    {
        /// <summary>
        /// None (disabled)
        /// </summary>
        None = 0,

        /// <summary>
        /// Server mode.
        /// </summary>
        Server = 1,

        /// <summary>
        /// Client mode.
        /// </summary>
        Client = 2,

        /// <summary>
        /// Dual mode.
        /// </summary>
        ClientServer = 3,
    }
}