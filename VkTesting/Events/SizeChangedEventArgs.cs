namespace HexaEngine.Core.Windows.Events
{
    public class SizeChangedEventArgs : RoutedEventArgs
    {
        public int OldWidth;
        public int OldHeight;
        public int Width;
        public int Height;
    }
}