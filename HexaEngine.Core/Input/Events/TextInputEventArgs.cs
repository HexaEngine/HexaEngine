namespace HexaEngine.Core.Input.Events
{
    /// <summary>
    /// Provides data for keyboard text input events (non-IME).
    /// </summary>
    public class TextInputEventArgs : InputEventArgs
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
        /// <param name="text">The text.</param>
        public unsafe TextInputEventArgs(byte* text)
        {
            Text = text;
        }

        /// <summary>
        /// The null-terminated input text in UTF-8 encoding.
        /// </summary>
        public unsafe byte* Text { get; internal set; }
    }
}