namespace HexaEngine.Core.Windows.Events
{
    public class RoutedEventArgs : TimestampEventArgs
    {
        /// <summary>
        /// Gets or sets a _value indicating whether the event is handled.
        /// </summary>
        /// <remarks>
        /// Setting this property to <see langword="true"/> indicates that the event has been handled
        /// and no further action is required. Setting it to <see langword="false"/> or leaving it unchanged
        /// indicates that the event should be processed normally.
        /// </remarks>
        public bool Handled { get; set; }
    }
}