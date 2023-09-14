namespace D3D12Testing.Events
{
    public class SizeChangedEventArgs : RoutedEventArgs
    {
        public int OldWidth;
        public int OldHeight;
        public int Width;
        public int Height;
    }
}