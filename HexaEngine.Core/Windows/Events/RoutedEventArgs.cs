namespace HexaEngine.Core.Windows.Events
{
    public delegate void RoutedEventHandler(object? sender, RoutedEventArgs e);

    public delegate void RoutedEventHandler<T>(object? sender, T e) where T : RoutedEventArgs;

    /// <summary>
    /// Represents event arguments for routed events.
    /// </summary>
    public class RoutedEventArgs
    {
        public RoutedEventArgs()
        {
        }

        public RoutedEventArgs(RoutedEvent routedEvent)
        {
            RoutedEvent = routedEvent;
        }

        public RoutedEventArgs(object? source, RoutedEvent routedEvent)
        {
            Source = source;
            RoutedEvent = routedEvent;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the event is handled.
        /// </summary>
        /// <remarks>
        /// Setting this property to <see langword="true"/> indicates that the event has been handled
        /// and no further action is required. Setting it to <see langword="false"/> or leaving it unchanged
        /// indicates that the event should be processed normally.
        /// </remarks>
        public bool Handled { get; set; }

        public object? Source { get; set; }

        public RoutedEvent? RoutedEvent { get; set; }
    }
}