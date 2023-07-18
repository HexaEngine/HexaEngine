namespace HexaEngine.Core.Windows.Events
{
    /// <summary>
    /// Event arguments for the hidden event of a window.
    /// </summary>
    public class HiddenEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Gets the old state of the window.
        /// </summary>
        public WindowState OldState { get; internal set; }

        /// <summary>
        /// Gets the new state of the window.
        /// </summary>
        public WindowState NewState { get; internal set; }
    }
}