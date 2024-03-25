namespace HexaEngine.Core.Input.Events
{
    /// <summary>
    /// Provides data for the text input events (IME).
    /// </summary>
    public class TextEditingEventArgs : InputEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextEditingEventArgs"/> class.
        /// </summary>
        public TextEditingEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextEditingEventArgs"/> class with the specified text, start index, and length.
        /// </summary>
        /// <param name="text">The text being edited.</param>
        /// <param name="start">The starting index of the edited text.</param>
        /// <param name="length">The length of the edited text.</param>
        public unsafe TextEditingEventArgs(byte* text, int start, int length)
        {
            Text = text;
            Start = start;
            Length = length;
        }

        /// <summary>
        /// The null-terminated editing text in UTF-8 encoding.
        /// </summary>
        public unsafe byte* Text { get; internal set; }

        /// <summary>
        /// The location to begin editing from.
        /// </summary>
        public int Start { get; internal set; }

        /// <summary>
        /// The number of characters to edit from the start point.
        /// </summary>
        public int Length { get; internal set; }
    }
}