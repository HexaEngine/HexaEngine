namespace HexaEngine.Core
{
    using Hexa.NET.SDL2;

    /// <summary>
    /// A static class for interacting with the system clipboard.
    /// </summary>
    public static unsafe class Clipboard
    {
        /// <summary>
        /// Retrieves the clipboard text as an unsafe pointer to a character array.
        /// </summary>
        /// <returns>An unsafe pointer to the clipboard text.</returns>
        public static char* GetClipboardTextUnsafe()
        {
            return (char*)SDL.SDLGetClipboardText();
        }

        /// <summary>
        /// Retrieves the clipboard text as a string.
        /// </summary>
        /// <returns>The clipboard text as a string.</returns>
        public static string GetClipboardText()
        {
            return SDL.SDLGetClipboardTextS();
        }

        /// <summary>
        /// Sets the clipboard text using an unsafe pointer to a character array.
        /// </summary>
        /// <param name="text">An unsafe pointer to the text to be set in the clipboard.</param>
        public static void SetClipboardTextUnsafe(char* text)
        {
            SDL.SDLSetClipboardText((byte*)text);
        }

        /// <summary>
        /// Sets the clipboard text using a string.
        /// </summary>
        /// <param name="text">The text to be set in the clipboard.</param>
        public static void SetClipboardText(string text)
        {
            SDL.SDLSetClipboardText(text);
        }

        /// <summary>
        /// Frees the memory associated with an unsafe pointer to a character array obtained from the clipboard.
        /// </summary>
        /// <param name="text">An unsafe pointer to the text to be freed.</param>
        public static void Free(char* text)
        {
            SDL.SDLFree(text);
        }
    }
}