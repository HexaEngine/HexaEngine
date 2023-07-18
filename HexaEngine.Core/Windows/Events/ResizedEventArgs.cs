namespace HexaEngine.Core.Windows.Events
{
    /// <summary>
    /// Provides event arguments for the resized event of a window.
    /// </summary>
    public class ResizedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResizedEventArgs"/> class.
        /// </summary>
        public ResizedEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResizedEventArgs"/> class with the specified dimensions.
        /// </summary>
        /// <param name="oldWidth">The old width of the window.</param>
        /// <param name="oldHeight">The old height of the window.</param>
        /// <param name="newWidth">The new width of the window.</param>
        /// <param name="newHeight">The new height of the window.</param>
        public ResizedEventArgs(int oldWidth, int oldHeight, int newWidth, int newHeight)
        {
            OldWidth = oldWidth;
            OldHeight = oldHeight;
            NewWidth = newWidth;
            NewHeight = newHeight;
        }

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
        public int NewWidth { get; internal set; }

        /// <summary>
        /// Gets the new height of the window.
        /// </summary>
        public int NewHeight { get; internal set; }
    }
}