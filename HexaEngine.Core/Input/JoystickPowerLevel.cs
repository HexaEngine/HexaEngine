namespace HexaEngine.Core.Input
{
    /// <summary>
    /// Represents the power level of a joystick.
    /// </summary>
    public enum JoystickPowerLevel
    {
        /// <summary>
        /// The power level is unknown.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// The joystick is empty, indicating no power.
        /// </summary>
        Empty = 0x0,

        /// <summary>
        /// The joystick has low power.
        /// </summary>
        Low = 0x1,

        /// <summary>
        /// The joystick has medium power.
        /// </summary>
        Medium = 0x2,

        /// <summary>
        /// The joystick has a full charge.
        /// </summary>
        Full = 0x3,

        /// <summary>
        /// The joystick is wired, meaning it's connected to a power source.
        /// </summary>
        Wired = 0x4,

        /// <summary>
        /// The joystick has the maximum power level.
        /// </summary>
        Max = 0x5
    }
}