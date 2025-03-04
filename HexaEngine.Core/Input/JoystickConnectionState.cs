namespace HexaEngine.Core.Input
{
    /// <summary>
    /// Represents the connection state of a joystick.
    /// </summary>
    public enum JoystickConnectionState
    {
        /// <summary>
        /// The state is invalid.
        /// </summary>
        Invalid = -1,

        /// <summary>
        /// The state is unknown.
        /// </summary>
        Unknown = 0x0,

        /// <summary>
        /// The joystick is wired, meaning it's connected to a power source.
        /// </summary>
        Wired = 0x1,

        /// <summary>
        /// The joystick is connected via a wireless connection.
        /// </summary>
        Wireless = 0x2
    }
}