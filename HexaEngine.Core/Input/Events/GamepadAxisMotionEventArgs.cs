namespace HexaEngine.Core.Input.Events
{
    /// <summary>
    /// Provides data for the event that occurs when a gamepad axis motion is detected.
    /// </summary>
    public class GamepadAxisMotionEventArgs : GamepadEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GamepadAxisMotionEventArgs"/> class.
        /// </summary>
        public GamepadAxisMotionEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GamepadAxisMotionEventArgs"/> class with specified axis and value.
        /// </summary>
        /// <param name="axis">The gamepad axis associated with the motion.</param>
        /// <param name="value">The value of the axis motion.</param>
        public GamepadAxisMotionEventArgs(GamepadAxis axis, short value)
        {
            Axis = axis;
            Value = value;
        }

        /// <summary>
        /// Gets the gamepad axis associated with the motion.
        /// </summary>
        public GamepadAxis Axis { get; internal set; }

        /// <summary>
        /// Gets the value of the axis motion.
        /// </summary>
        public short Value { get; internal set; }
    }
}