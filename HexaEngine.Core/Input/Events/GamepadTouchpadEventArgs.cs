namespace HexaEngine.Core.Input.Events
{
    /// <summary>
    /// Provides data for a gamepad touchpad event.
    /// </summary>
    public class GamepadTouchpadEventArgs : GamepadEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GamepadTouchpadEventArgs"/> class.
        /// </summary>
        public GamepadTouchpadEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GamepadTouchpadEventArgs"/> class with touchpad data.
        /// </summary>
        /// <param name="finger">The finger ID associated with the touchpad event.</param>
        /// <param name="state">The state of the touchpad.</param>
        /// <param name="x">The X-coordinate of the touchpad position.</param>
        /// <param name="y">The Y-coordinate of the touchpad position.</param>
        /// <param name="pressure">The pressure applied to the touchpad.</param>
        public GamepadTouchpadEventArgs(int finger, FingerState state, float x, float y, float pressure)
        {
            Finger = finger;
            State = state;
            X = x;
            Y = y;
            Pressure = pressure;
        }

        /// <summary>
        /// Gets the ID of the touchpad associated with this event.
        /// </summary>
        public int TouchpadId { get; internal set; }

        /// <summary>
        /// Gets the corresponding touchpad object for this touchpad event.
        /// </summary>
        public GamepadTouchpad Touchpad => Gamepad.Touchpads[TouchpadId];

        /// <summary>
        /// Gets the finger ID associated with the touchpad event.
        /// </summary>
        public int Finger { get; internal set; }

        /// <summary>
        /// Gets the state of the touchpad.
        /// </summary>
        public FingerState State { get; internal set; }

        /// <summary>
        /// Gets the X-coordinate of the touchpad position.
        /// </summary>
        public float X { get; internal set; }

        /// <summary>
        /// Gets the Y-coordinate of the touchpad position.
        /// </summary>
        public float Y { get; internal set; }

        /// <summary>
        /// Gets the pressure applied to the touchpad.
        /// </summary>
        public float Pressure { get; internal set; }
    }
}