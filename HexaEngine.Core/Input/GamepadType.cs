namespace HexaEngine.Core.Input
{
    /// <summary>
    /// Represents the type of a gamepad controller.
    /// </summary>
    public enum GamepadType
    {
        /// <summary>
        /// The gamepad type is unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The gamepad is an Xbox 360 controller.
        /// </summary>
        Xbox360 = 1,

        /// <summary>
        /// The gamepad is an Xbox One controller.
        /// </summary>
        XboxOne = 2,

        /// <summary>
        /// The gamepad is a PlayStation 3 (PS3) controller.
        /// </summary>
        PS3 = 3,

        /// <summary>
        /// The gamepad is a PlayStation 4 (PS4) controller.
        /// </summary>
        PS4 = 4,

        /// <summary>
        /// The gamepad is a Nintendo Switch Pro controller.
        /// </summary>
        NintendoSwitchPro = 5,

        /// <summary>
        /// The gamepad is a virtual or software-based controller.
        /// </summary>
        Virtual = 6,

        /// <summary>
        /// The gamepad is a PlayStation 5 (PS5) controller.
        /// </summary>
        PS5 = 7
    }
}