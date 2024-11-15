namespace HexaEngine.Core.Windows.UI
{
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;

    public class MinimizeWindowRequest : RoutedEventArgs
    {
        public MinimizeWindowRequest(CoreWindow window)
        {
            Window = window;
        }

        public CoreWindow Window { get; }
    }
}