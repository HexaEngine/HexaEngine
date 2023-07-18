namespace HexaEngine.Core.Windows.Events
{
    /// <summary>
    /// Provides event arguments for the moved event of a window.
    /// </summary>
    public class MovedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Gets the old X position of the window.
        /// </summary>
        public int OldX { get; internal set; }

        /// <summary>
        /// Gets the old Y position of the window.
        /// </summary>
        public int OldY { get; internal set; }

        /// <summary>
        /// Gets the new X position of the window.
        /// </summary>
        public int NewX { get; internal set; }

        /// <summary>
        /// Gets the new Y position of the window.
        /// </summary>
        public int NewY { get; internal set; }
    }
}