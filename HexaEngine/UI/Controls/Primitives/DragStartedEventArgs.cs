namespace HexaEngine.UI.Controls.Primitives
{
    using HexaEngine.Core.Windows.Events;

    public class DragStartedEventArgs : RoutedEventArgs
    {
        public DragStartedEventArgs(RoutedEvent routedEvent) : base(routedEvent)
        {
        }

        public DragStartedEventArgs(float horizontalOffset, float verticalOffset)
        {
            HorizontalOffset = horizontalOffset;
            VerticalOffset = verticalOffset;
        }

        public float HorizontalOffset { get; internal set; }

        public float VerticalOffset { get; internal set; }
    }
}