namespace HexaEngine.Core.Input.Events
{
    using HexaEngine.Core.Windows.Events;

    /// <summary>
    /// Provides a base class for mouse-related event arguments.
    /// </summary>
    public class MouseEventArgs : RoutedEventArgs
    {
        public MouseEventArgs()
        {
        }

        public MouseEventArgs(RoutedEvent routedEvent)
        {
            RoutedEvent = routedEvent;
        }

        public MouseEventArgs(object? source, RoutedEvent routedEvent)
        {
            Source = source;
            RoutedEvent = routedEvent;
        }

        /// <summary>
        /// Gets the identifier associated with the mouse device.
        /// </summary>
        public uint MouseId { get; internal set; }
    }
}