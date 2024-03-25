namespace HexaEngine.UI.Controls
{
    using HexaEngine.Core.Windows.Events;

    public class ScrollChangedEventArgs : RoutedEventArgs
    {
        public ScrollChangedEventArgs(float extentHeight, float extentHeightChange, float extentWidth, float extentWidthChange, float horizontalChange, float horizontalOffset, float verticalChange, float verticalOffset, float viewportHeight, float viewportHeightChange, float viewportWidth, float viewportWidthChange)
        {
            ExtentHeight = extentHeight;
            ExtentHeightChange = extentHeightChange;
            ExtentWidth = extentWidth;
            ExtentWidthChange = extentWidthChange;
            HorizontalChange = horizontalChange;
            HorizontalOffset = horizontalOffset;
            VerticalChange = verticalChange;
            VerticalOffset = verticalOffset;
            ViewportHeight = viewportHeight;
            ViewportHeightChange = viewportHeightChange;
            ViewportWidth = viewportWidth;
            ViewportWidthChange = viewportWidthChange;
        }

        public float ExtentHeight { get; }

        public float ExtentHeightChange { get; }

        public float ExtentWidth { get; }

        public float ExtentWidthChange { get; }

        public float HorizontalChange { get; }

        public float HorizontalOffset { get; }

        public float VerticalChange { get; }

        public float VerticalOffset { get; }

        public float ViewportHeight { get; }

        public float ViewportHeightChange { get; }

        public float ViewportWidth { get; }

        public float ViewportWidthChange { get; }
    }
}