namespace HexaEngine.Core
{
    using Hexa.NET.SDL2;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// A static class for interacting with the system clipboard.
    /// </summary>
    public static unsafe class Clipboard
    {
        /// <summary>
        /// Retrieves the clipboard text as an unsafe pointer to a character array.
        /// </summary>
        /// <returns>An unsafe pointer to the clipboard text.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte* GetTextUnsafe()
        {
            return SDL.GetClipboardText();
        }

        /// <summary>
        /// Retrieves the clipboard text as a string.
        /// </summary>
        /// <returns>The clipboard text as a string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetText()
        {
            var text = GetTextUnsafe();

            var result = HexaGen.Runtime.Utils.DecodeStringUTF8(text);

            Free(text);

            return result;
        }

        /// <summary>
        /// Sets the clipboard text using an unsafe pointer to a character array.
        /// </summary>
        /// <param name="text">An unsafe pointer to the text to be set in the clipboard.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetTextUnsafe(byte* text)
        {
            SDL.SetClipboardText(text);
        }

        /// <summary>
        /// Sets the clipboard text using a string.
        /// </summary>
        /// <param name="text">The text to be set in the clipboard.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetText(string text)
        {
            SDL.SetClipboardText(text);
        }

        /// <summary>
        /// Frees the memory associated with an unsafe pointer to a character array obtained from the clipboard.
        /// </summary>
        /// <param name="text">An unsafe pointer to the text to be freed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(byte* text)
        {
            SDL.Free(text);
        }
    }
}