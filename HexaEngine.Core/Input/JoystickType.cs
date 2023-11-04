namespace HexaEngine.Core.Input
{
    /// <summary>
    /// Enumerates the different types of joysticks.
    /// </summary>
    public enum JoystickType
    {
        /// <summary>
        /// Unknown or generic joystick type.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Game controller joystick type.
        /// </summary>
        GameController = 1,

        /// <summary>
        /// Wheel-type joystick.
        /// </summary>
        Wheel = 2,

        /// <summary>
        /// Arcade stick joystick type.
        /// </summary>
        ArcadeStick = 3,

        /// <summary>
        /// Flight stick joystick type.
        /// </summary>
        FlightStick = 4,

        /// <summary>
        /// Dance pad joystick type.
        /// </summary>
        DancePad = 5,

        /// <summary>
        /// Guitar controller joystick type.
        /// </summary>
        Guitar = 6,

        /// <summary>
        /// Drum kit joystick type.
        /// </summary>
        DrumKit = 7,

        /// <summary>
        /// Arcade pad joystick type.
        /// </summary>
        ArcadePad = 8,

        /// <summary>
        /// Throttle joystick type.
        /// </summary>
        Throttle = 9
    }
}