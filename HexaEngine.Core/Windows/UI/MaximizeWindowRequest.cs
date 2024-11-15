namespace HexaEngine.Core.Windows.UI
{
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;

    public class MaximizeWindowRequest : RoutedEventArgs
    {
        public MaximizeWindowRequest(CoreWindow window)
        {
            Window = window;
        }

        public CoreWindow Window { get; }
    }
}