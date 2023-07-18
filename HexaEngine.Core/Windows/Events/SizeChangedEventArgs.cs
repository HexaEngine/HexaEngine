namespace HexaEngine.Core.Windows.Events
{
    /// <summary>
    /// Provides event arguments for the size changed event of a window.
    /// </summary>
    public class SizeChangedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Gets the old width of the window.
        /// </summary>
        public int OldWidth { get; internal set; }

        /// <summary>
        /// Gets the old height of the window.
        /// </summary>
        public int OldHeight { get; internal set; }

        /// <summary>
        /// Gets the new width of the window.
        /// </summary>
        public int Width { get; internal set; }

        /// <summary>
        /// Gets the new height of the window.
        /// </summary>
        public int Height { get; internal set; }
    }
}