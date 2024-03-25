namespace HexaEngine.UI.Controls.Primitives
{
    using HexaEngine.Core.Windows.Events;

    public class DragDeltaEventArgs : RoutedEventArgs
    {
        public DragDeltaEventArgs(RoutedEvent routedEvent) : base(routedEvent)
        {
        }

        public DragDeltaEventArgs(float horizontalChange, float verticalChange)
        {
            HorizontalChange = horizontalChange;
            VerticalChange = verticalChange;
        }

        public float HorizontalChange { get; internal set; }

        public float VerticalChange { get; internal set; }
    }
}