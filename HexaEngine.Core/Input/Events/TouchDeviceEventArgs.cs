namespace HexaEngine.Core.Input.Events
{
    using HexaEngine.Core.Windows.Events;

    /// <summary>
    /// Provides a base class for touch-device-related event arguments.
    /// </summary>
    public class TouchDeviceEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TouchDeviceEventArgs"/> class.
        /// </summary>
        public TouchDeviceEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchDeviceEventArgs"/> class with a specified touch device ID.
        /// </summary>
        /// <param name="touchDeviceId">The unique identifier of the touch device.</param>
        public TouchDeviceEventArgs(long touchDeviceId)
        {
            TouchDeviceId = touchDeviceId;
        }

        /// <summary>
        /// Gets the unique identifier of the touch device.
        /// </summary>
        public long TouchDeviceId { get; internal set; }

        /// <summary>
        /// Gets the touch device associated with the event.
        /// </summary>
        public TouchDevice TouchDevice => TouchDevices.GetById(TouchDeviceId);
    }
}