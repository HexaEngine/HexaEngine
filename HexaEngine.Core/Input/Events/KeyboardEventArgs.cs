namespace HexaEngine.Core.Input.Events
{
    using HexaEngine.Core.Input;

    /// <summary>
    /// Provides data for keyboard input events.
    /// </summary>
    public class KeyboardEventArgs : InputEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardEventArgs"/> class.
        /// </summary>
        public KeyboardEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardEventArgs"/> class with specified key code, key state, and scan code.
        /// </summary>
        /// <param name="keyCode">The key code associated with the event.</param>
        /// <param name="keyState">The state of the key (Up or Down).</param>
        /// <param name="scanCode">The scan code of the key.</param>
        public KeyboardEventArgs(Key keyCode, KeyState keyState, ScanCode scanCode)
        {
            KeyCode = keyCode;
            State = keyState;
            ScanCode = scanCode;
        }

        /// <summary>
        /// Gets the key code associated with the event.
        /// </summary>
        public Key KeyCode { get; internal set; }

        /// <summary>
        /// Gets the state of the key (Up or Down).
        /// </summary>
        public KeyState State { get; internal set; }

        /// <summary>
        /// Gets the scan code of the key.
        /// </summary>
        public ScanCode ScanCode { get; internal set; }
    }
}