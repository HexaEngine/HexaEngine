namespace HexaEngine.Core.Input
{
    /// <summary>
    /// Represents the available axes on a gamepad controller.
    /// </summary>
    public enum GamepadAxis
    {
        Invalid = -1,

        /// <summary>
        /// The left stick's X-axis.
        /// </summary>
        LeftX = 0,

        /// <summary>
        /// The left stick's Y-axis.
        /// </summary>
        LeftY = 1,

        /// <summary>
        /// The right stick's X-axis.
        /// </summary>
        RightX = 2,

        /// <summary>
        /// The right stick's Y-axis.
        /// </summary>
        RightY = 3,

        /// <summary>
        /// The left trigger button.
        /// </summary>
        LeftTrigger = 4,

        /// <summary>
        /// The right trigger button.
        /// </summary>
        RightTrigger = 5,

        Count = 6
    }
}