namespace HexaEngine.UI.Controls.Primitives
{
    using HexaEngine.Core.Windows.Events;

    public class DragCompletedEventArgs : RoutedEventArgs
    {
        public DragCompletedEventArgs(RoutedEvent routedEvent) : base(routedEvent)
        {
        }

        public DragCompletedEventArgs(float horizontalChange, float verticalChange, bool canceled)
        {
            HorizontalChange = horizontalChange;
            VerticalChange = verticalChange;
            Canceled = canceled;
        }

        public float HorizontalChange { get; internal set; }

        public float VerticalChange { get; internal set; }

        public bool Canceled { get; internal set; }
    }
}