namespace HexaEngine.Core.Input.Events
{
    /// <summary>
    /// Provides base data for joystick-related events.
    /// </summary>
    public class JoystickEventArgs : InputEventArgs
    {
        /// <summary>
        /// Gets the ID of the joystick associated with the event.
        /// </summary>
        public int JoystickId { get; internal set; }

        /// <summary>
        /// Gets the joystick object associated with the event.
        /// </summary>
        public Joystick Joystick => Joysticks.GetById(JoystickId);
    }
}