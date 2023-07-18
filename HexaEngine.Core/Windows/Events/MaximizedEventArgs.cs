namespace HexaEngine.Core.Windows.Events
{
    using HexaEngine.Core.Windows;

    /// <summary>
    /// Provides event arguments for the maximized event of a window.
    /// </summary>
    public class MaximizedEventArgs : RoutedEventArgs
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