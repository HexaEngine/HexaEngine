namespace HexaEngine.Core.Windows.UI
{
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;

    public class CloseWindowRequest : RoutedEventArgs
    {
        public CloseWindowRequest(CoreWindow window)
        {
            Window = window;
        }

        public CoreWindow Window { get; }
    }
}