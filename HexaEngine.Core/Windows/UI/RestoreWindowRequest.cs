namespace HexaEngine.Core.Windows.UI
{
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;

    public class RestoreWindowRequest : RoutedEventArgs
    {
        public RestoreWindowRequest(CoreWindow window)
        {
            Window = window;
        }

        public CoreWindow Window { get; }
    }
}