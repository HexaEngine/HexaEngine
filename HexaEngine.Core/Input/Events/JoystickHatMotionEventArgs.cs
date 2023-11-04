namespace HexaEngine.Core.Input.Events
{
    /// <summary>
    /// Provides data for joystick hat motion events.
    /// </summary>
    public class JoystickHatMotionEventArgs : JoystickEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JoystickHatMotionEventArgs"/> class.
        /// </summary>
        public JoystickHatMotionEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JoystickHatMotionEventArgs"/> class with specified hat and state.
        /// </summary>
        /// <param name="hat">The hat index.</param>
        /// <param name="state">The hat state.</param>
        public JoystickHatMotionEventArgs(int hat, JoystickHatState state)
        {
            Hat = hat;
            State = state;
        }

        /// <summary>
        /// Gets the index of the hat associated with the event.
        /// </summary>
        public int Hat { get; internal set; }

        /// <summary>
        /// Gets the state of the hat.
        /// </summary>
        public JoystickHatState State { get; internal set; }
    }
}