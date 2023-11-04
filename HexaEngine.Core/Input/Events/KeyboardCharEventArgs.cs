namespace HexaEngine.Core.Input.Events
{
    using HexaEngine.Core.Windows.Events;

    /// <summary>
    /// Provides data for keyboard character input events.
    /// </summary>
    public class KeyboardCharEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardCharEventArgs"/> class.
        /// </summary>
        public KeyboardCharEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardCharEventArgs"/> class.
        /// </summary>
        /// <param name="char">The character.</param>
        public KeyboardCharEventArgs(char @char)
        {
            Char = @char;
        }

        /// <summary>
        /// Gets the character associated with the event.
        /// </summary>
        public char Char { get; internal set; }
    }
}