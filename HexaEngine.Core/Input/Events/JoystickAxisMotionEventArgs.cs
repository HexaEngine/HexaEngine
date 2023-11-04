namespace HexaEngine.Core.Input.Events
{
    /// <summary>
    /// Provides data for a joystick axis motion event.
    /// </summary>
    public class JoystickAxisMotionEventArgs : JoystickEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JoystickAxisMotionEventArgs"/> class.
        /// </summary>
        public JoystickAxisMotionEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JoystickAxisMotionEventArgs"/> class with axis motion data.
        /// </summary>
        /// <param name="axis">The ID of the joystick axis that triggered the motion event.</param>
        /// <param name="value">The value representing the motion on the joystick axis.</param>
        public JoystickAxisMotionEventArgs(int axis, short value)
        {
            Axis = axis;
            Value = value;
        }

        /// <summary>
        /// Gets the ID of the joystick axis that triggered the motion event.
        /// </summary>
        public int Axis { get; internal set; }

        /// <summary>
        /// Gets the value representing the motion on the joystick axis.
        /// </summary>
        public short Value { get; internal set; }
    }
}