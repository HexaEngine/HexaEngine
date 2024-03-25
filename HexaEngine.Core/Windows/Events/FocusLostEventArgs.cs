namespace HexaEngine.Core.Windows.Events
{
    /// <summary>
    /// Event arguments for the focus lost event of a window.
    /// </summary>
    public class FocusLostEventArgs : RoutedEventArgs
    {
        // No additional members or properties in this class
        public FocusLostEventArgs()
        {
        }

        public FocusLostEventArgs(RoutedEvent routedEvent) : base(routedEvent)
        {
        }
    }
}