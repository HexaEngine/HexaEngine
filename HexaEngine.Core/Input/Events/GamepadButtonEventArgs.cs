namespace HexaEngine.Core.Input.Events
{
    /// <summary>
    /// Provides data for the event that occurs when a gamepad button is pressed or released.
    /// </summary>
    public class GamepadButtonEventArgs : GamepadEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GamepadButtonEventArgs"/> class.
        /// </summary>
        public GamepadButtonEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GamepadButtonEventArgs"/> class with specified button and state.
        /// </summary>
        /// <param name="button">The gamepad button associated with the event.</param>
        /// <param name="state">The state of the gamepad button (pressed or released).</param>
        public GamepadButtonEventArgs(GamepadButton button, GamepadButtonState state)
        {
            Button = button;
            State = state;
        }

        /// <summary>
        /// Gets the gamepad button associated with the event.
        /// </summary>
        public GamepadButton Button { get; internal set; }

        /// <summary>
        /// Gets the state of the gamepad button (pressed or released).
        /// </summary>
        public GamepadButtonState State { get; internal set; }
    }
}