namespace HexaEngine.Core.Input.Events
{
    using HexaEngine.Core.Windows.Events;

    /// <summary>
    /// An event with a Timestamp.
    /// </summary>
    /// <seealso cref="RoutedEventArgs" />
    public class InputEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Gets the timestamp of an event.
        /// </summary>
        public ulong Timestamp { get; set; }
    }
}