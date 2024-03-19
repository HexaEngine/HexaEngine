namespace HexaEngine.Core.Input.Events
{
    /// <summary>
    /// Provides data for touch motion events.
    /// </summary>
    public class TouchMoveEventArgs : TouchDeviceEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TouchMoveEventArgs"/> class.
        /// </summary>
        public TouchMoveEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchMoveEventArgs"/> class with touch motion event data.
        /// </summary>
        /// <param name="touchDeviceId">The unique identifier of the touch device.</param>
        /// <param name="fingerId">The unique identifier of the finger.</param>
        /// <param name="x">The x-coordinate of the touch event.</param>
        /// <param name="y">The y-coordinate of the touch event.</param>
        /// <param name="pressure">The pressure of the touch event.</param>
        public TouchMoveEventArgs(long touchDeviceId, long fingerId, float x, float y, float pressure) : base(touchDeviceId)
        {
            FingerId = fingerId;
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
        /// Gets the x-coordinate of the touch event.
        /// </summary>
        public float X { get; internal set; }

        /// <summary>
        /// Gets the y-coordinate of the touch event.
        /// </summary>
        public float Y { get; internal set; }

        /// <summary>
        /// Gets the change in the x-coordinate (delta x) of the touch event.
        /// </summary>
        public float Dx { get; internal set; }

        /// <summary>
        /// Gets the change in the y-coordinate (delta y) of the touch event.
        /// </summary>
        public float Dy { get; internal set; }

        /// <summary>
        /// Gets the pressure of the touch event.
        /// </summary>
        public float Pressure { get; internal set; }
    }
}