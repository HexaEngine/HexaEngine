namespace HexaEngine.Core.Input.Events
{
    using HexaEngine.Core.Windows.Events;

    /// <summary>
    /// Provides data for keyboard character input events.
    /// </summary>
    public class TextInputEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextInputEventArgs"/> class.
        /// </summary>
        public TextInputEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextInputEventArgs"/> class.
        /// </summary>
        /// <param name="char">The character.</param>
        public TextInputEventArgs(char @char)
        {
            Char = @char;
        }

        /// <summary>
        /// Gets the character associated with the event.
        /// </summary>
        public char Char { get; internal set; }
    }
}