namespace HexaEngine.UI
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;
    using HexaEngine.Core.Windows.Events;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class TextCompositionEventArgs : InputEventArgs
    {
    }

    public sealed class TextCompositionManager
    {
        private static StringBuilder composedText = new();

        static TextCompositionManager()
        {
            Keyboard.TextInput += KeyboardTextInput;
            Keyboard.TextEditing += KeyboardTextEditing;
        }

        private static unsafe void KeyboardTextEditing(object? sender, TextEditingEventArgs e)
        {
            var pText = e.Text;
            while (*pText != '\0')
            {
                char c = (char)*pText;
                composedText.Append(c);
                pText++;
            }

            // IME
        }

        private static unsafe void KeyboardTextInput(object? sender, TextInputEventArgs e)
        {
            var pText = e.Text;
            while (*pText != '\0')
            {
                char c = (char)*pText;
                composedText.Append(c);
                pText++;
            }

            // non-IME
        }
    }
}