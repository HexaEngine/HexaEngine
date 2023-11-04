namespace HexaEngine.Core.Input.Events
{
    /// <summary>
    /// Provides data for a joystick button event.
    /// </summary>
    public class JoystickButtonEventArgs : JoystickEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JoystickButtonEventArgs"/> class.
        /// </summary>
        public JoystickButtonEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JoystickButtonEventArgs"/> class with button event data.
        /// </summary>
        /// <param name="button">The ID of the joystick button that triggered the event.</param>
        /// <param name="state">The state of the joystick button (pressed or released).</param>
        public JoystickButtonEventArgs(int button, JoystickButtonState state)
        {
            Button = button;
            State = state;
        }

        /// <summary>
        /// Gets the ID of the joystick button that triggered the event.
        /// </summary>
        public int Button { get; internal set; }

        /// <summary>
        /// Gets the state of the joystick button (pressed or released).
        /// </summary>
        public JoystickButtonState State { get; internal set; }
    }
}