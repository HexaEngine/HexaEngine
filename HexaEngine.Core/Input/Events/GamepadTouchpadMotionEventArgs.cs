namespace HexaEngine.Core.Input.Events
{
    /// <summary>
    /// Provides data for a gamepad touchpad motion event.
    /// </summary>
    public class GamepadTouchpadMotionEventArgs : GamepadEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GamepadTouchpadMotionEventArgs"/> class.
        /// </summary>
        public GamepadTouchpadMotionEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GamepadTouchpadMotionEventArgs"/> class with touchpad motion data.
        /// </summary>
        /// <param name="finger">The finger ID associated with the touchpad motion event.</param>
        /// <param name="x">The X-coordinate of the touchpad position during motion.</param>
        /// <param name="y">The Y-coordinate of the touchpad position during motion.</param>
        /// <param name="pressure">The pressure applied to the touchpad during motion.</param>
        public GamepadTouchpadMotionEventArgs(int finger, float x, float y, float pressure)
        {
            Finger = finger;
            X = x;
            Y = y;
            Pressure = pressure;
        }

        /// <summary>
        /// Gets the ID of the touchpad associated with this event.
        /// </summary>
        public int TouchpadId { get; internal set; }

        /// <summary>
        /// Gets the corresponding touchpad object for this touchpad motion event.
        /// </summary>
        public GamepadTouchpad Touchpad => Gamepad.Touchpads[TouchpadId];

        /// <summary>
        /// Gets the finger ID associated with the touchpad motion event.
        /// </summary>
        public int Finger { get; internal set; }

        /// <summary>
        /// Gets the X-coordinate of the touchpad position during motion.
        /// </summary>
        public float X { get; internal set; }

        /// <summary>
        /// Gets the Y-coordinate of the touchpad position during motion.
        /// </summary>
        public float Y { get; internal set; }

        /// <summary>
        /// Gets the pressure applied to the touchpad during motion.
        /// </summary>
        public float Pressure { get; internal set; }
    }
}