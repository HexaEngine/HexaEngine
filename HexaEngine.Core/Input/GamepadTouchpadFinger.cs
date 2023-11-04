namespace HexaEngine.Core.Input
{
    /// <summary>
    /// Represents the state of a finger on a gamepad touchpad.
    /// </summary>
    public struct GamepadTouchpadFinger
    {
        /// <summary>
        /// Gets the state of the finger (e.g., Up or Down).
        /// </summary>
        public FingerState State;

        /// <summary>
        /// Gets the X-coordinate of the finger's position on the touchpad.
        /// </summary>
        public float X;

        /// <summary>
        /// Gets the Y-coordinate of the finger's position on the touchpad.
        /// </summary>
        public float Y;

        /// <summary>
        /// Gets the pressure applied by the finger on the touchpad.
        /// </summary>
        public float Pressure;

        /// <summary>
        /// Initializes a new instance of the <see cref="GamepadTouchpadFinger"/> struct.
        /// </summary>
        /// <param name="state">The state of the finger (e.g., Up or Down).</param>
        /// <param name="x">The X-coordinate of the finger's position on the touchpad.</param>
        /// <param name="y">The Y-coordinate of the finger's position on the touchpad.</param>
        /// <param name="pressure">The pressure applied by the finger on the touchpad.</param>
        public GamepadTouchpadFinger(FingerState state, float x, float y, float pressure)
        {
            State = state;
            X = x;
            Y = y;
            Pressure = pressure;
        }
    }
}