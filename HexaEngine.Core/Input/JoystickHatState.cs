namespace HexaEngine.Core.Input
{
    /// <summary>
    /// Represents the state of a joystick hat or D-pad.
    /// </summary>
    public enum JoystickHatState
    {
        /// <summary>
        /// The hat or D-pad is in the centered position.
        /// </summary>
        Centered,

        /// <summary>
        /// The hat or D-pad is pushed in the "Up" direction.
        /// </summary>
        Up,

        /// <summary>
        /// The hat or D-pad is pushed in the "Right" direction.
        /// </summary>
        Right,

        /// <summary>
        /// The hat or D-pad is pushed in the "Down" direction.
        /// </summary>
        Down,

        /// <summary>
        /// The hat or D-pad is pushed in the "Left" direction.
        /// </summary>
        Left,

        /// <summary>
        /// The hat or D-pad is pushed in the "Right-Up" diagonal direction.
        /// </summary>
        RightUp,

        /// <summary>
        /// The hat or D-pad is pushed in the "Right-Down" diagonal direction.
        /// </summary>
        RightDown,

        /// <summary>
        /// The hat or D-pad is pushed in the "Left-Up" diagonal direction.
        /// </summary>
        LeftUp,

        /// <summary>
        /// The hat or D-pad is pushed in the "Left-Down" diagonal direction.
        /// </summary>
        LeftDown,
    }
}