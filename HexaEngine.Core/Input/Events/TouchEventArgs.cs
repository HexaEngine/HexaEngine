namespace HexaEngine.Core.Input.Events
{
    /// <summary>
    /// Provides data for touch events.
    /// </summary>
    public class TouchEventArgs : TouchDeviceEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TouchEventArgs"/> class.
        /// </summary>
        public TouchEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchEventArgs"/> class with touch event data.
        /// </summary>
        /// <param name="touchDeviceId">The unique identifier of the touch device.</param>
        /// <param name="fingerId">The unique identifier of the finger.</param>
        /// <param name="state">The state of the finger (e.g., Up or Down).</param>
        /// <param name="x">The x-coordinate of the touch event.</param>
        /// <param name="y">The y-coordinate of the touch event.</param>
        /// <param name="pressure">The pressure of the touch event.</param>
        public TouchEventArgs(int touchDeviceId, long fingerId, FingerState state, float x, float y, float pressure) : base(touchDeviceId)
        {
            FingerId = fingerId;
            State = state;
            X = x;
            Y = y;
            Pressure = pressure;
        }

        /// <summary>
        /// Gets the unique identifier of the finger associated with the touch event.
        /// </summary>
        public long FingerId { get; internal set; }

        /// <summary>
        /// Gets the finger associated with the touch event.
        /// </summary>
        public Finger Finger => TouchDevice.GetFingerById(FingerId);

        /// <summary>
        /// Gets the state of the finger (e.g., Up or Down).
        /// </summary>
        public FingerState State { get; internal set; }

        /// <summary>
        /// Gets the x-coordinate of the touch event.
        /// </summary>
        public float X { get; internal set; }

        /// <summary>
        /// Gets the y-coordinate of the touch event.
        /// </summary>
        public float Y { get; internal set; }

        /// <summary>
        /// Gets the pressure of the touch event.
        /// </summary>
        public float Pressure { get; internal set; }
    }
}