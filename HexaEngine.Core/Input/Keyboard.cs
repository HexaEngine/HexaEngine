namespace HexaEngine.Core.Input
{
    using HexaEngine.Core.Input.Events;
    using Silk.NET.SDL;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides access to keyboard input events and states.
    /// </summary>
    public static class Keyboard
    {
#nullable disable
        private static Sdl sdl;
#nullable enable

        private static readonly Key[] keys = Enum.GetValues<Key>();
        private static readonly string[] keyNames = new string[keys.Length];
        private static readonly Dictionary<Key, KeyState> states = [];
        private static readonly KeyboardEventArgs keyboardEventArgs = new();
        private static readonly TextInputEventArgs keyboardTextInputEventArgs = new();
        private static readonly TextEditingEventArgs keyboardTextEditingEventArgs = new();

        /// <summary>
        /// Gets a read-only list of available keyboard keys.
        /// </summary>
        public static IReadOnlyList<Key> Keys => keys;

        /// <summary>
        /// Gets a read-only list of human-readable names for keyboard keys.
        /// </summary>
        public static IReadOnlyList<string> KeyNames => keyNames;

        /// <summary>
        /// Gets a read-only dictionary representing the current state of keyboard keys.
        /// </summary>
        public static IReadOnlyDictionary<Key, KeyState> States => states;

        /// <summary>
        /// Initializes the keyboard input system.
        /// </summary>
        internal static unsafe void Init()
        {
            sdl = Application.sdl;
            int numkeys;
            byte* pKeys = sdl.GetKeyboardState(&numkeys);

            for (int i = 0; i < keys.Length; i++)
            {
                Key key = keys[i];
                keyNames[i] = sdl.GetKeyNameS((int)key);
                var scancode = (Key)sdl.GetScancodeFromKey((int)key);
                states.Add(key, (KeyState)pKeys[(int)scancode]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnKeyDown(KeyboardEvent keyboardEvent)
        {
            Key keyCode = (Key)sdl.GetKeyFromScancode(keyboardEvent.Keysym.Scancode);
            states[keyCode] = KeyState.Down;
            keyboardEventArgs.Timestamp = keyboardEvent.Timestamp;
            keyboardEventArgs.Handled = false;
            keyboardEventArgs.State = KeyState.Down;
            keyboardEventArgs.KeyCode = keyCode;
            keyboardEventArgs.ScanCode = (ScanCode)keyboardEvent.Keysym.Scancode;
            KeyDown?.Invoke(null, keyboardEventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnKeyUp(KeyboardEvent keyboardEvent)
        {
            Key keyCode = (Key)sdl.GetKeyFromScancode(keyboardEvent.Keysym.Scancode);
            states[keyCode] = KeyState.Up;
            keyboardEventArgs.Timestamp = keyboardEvent.Timestamp;
            keyboardEventArgs.Handled = false;
            keyboardEventArgs.State = KeyState.Up;
            keyboardEventArgs.KeyCode = keyCode;
            keyboardEventArgs.ScanCode = (ScanCode)keyboardEvent.Keysym.Scancode;
            KeyUp?.Invoke(null, keyboardEventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void OnTextInput(TextInputEvent textInputEvent)
        {
            keyboardTextInputEventArgs.Timestamp = textInputEvent.Timestamp;
            keyboardTextInputEventArgs.Handled = false;
            keyboardTextInputEventArgs.Text = textInputEvent.Text;
            TextInput?.Invoke(null, keyboardTextInputEventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void OnTextEditing(TextEditingEvent textInputEvent)
        {
            keyboardTextEditingEventArgs.Timestamp = textInputEvent.Timestamp;
            keyboardTextEditingEventArgs.Handled = false;
            keyboardTextEditingEventArgs.Text = textInputEvent.Text;
            keyboardTextEditingEventArgs.Start = textInputEvent.Start;
            keyboardTextEditingEventArgs.Length = textInputEvent.Length;
            TextEditing?.Invoke(null, keyboardTextEditingEventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void OnTextEditing(TextEditingExtEvent textInputEvent)
        {
            keyboardTextEditingEventArgs.Timestamp = textInputEvent.Timestamp;
            keyboardTextEditingEventArgs.Handled = false;
            keyboardTextEditingEventArgs.Text = textInputEvent.Text;
            keyboardTextEditingEventArgs.Start = textInputEvent.Start;
            keyboardTextEditingEventArgs.Length = textInputEvent.Length;
            TextEditing?.Invoke(null, keyboardTextEditingEventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Flush()
        {
        }

        /// <summary>
        /// Event raised when a key is pressed down.
        /// </summary>
        public static event EventHandler<KeyboardEventArgs>? KeyDown;

        /// <summary>
        /// Event raised when a key is released.
        /// </summary>
        public static event EventHandler<KeyboardEventArgs>? KeyUp;

        /// <summary>
        /// Event raised when text input is received from the keyboard. (non-IME)
        /// </summary>
        public static event EventHandler<TextInputEventArgs>? TextInput;

        /// <summary>
        /// Event raised when text input is received from the keyboard. (IME)
        /// </summary>
        public static event EventHandler<TextEditingEventArgs>? TextEditing;

        /// <summary>
        /// Checks if a specific key is in the "up" state.
        /// </summary>
        /// <param name="n">The key to check.</param>
        /// <returns><c>true</c> if the key is in the "up" state, <c>false</c> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUp(Key n)
        {
            return states[n] == KeyState.Up;
        }

        /// <summary>
        /// Checks if a specific key is in the "down" state.
        /// </summary>
        /// <param name="n">The key to check.</param>
        /// <returns><c>true</c> if the key is in the "down" state, <c>false</c> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDown(Key n)
        {
            return states[n] == KeyState.Down;
        }

        /// <summary>
        /// Gets the current keyboard modifier state.
        /// </summary>
        /// <returns>The current keyboard modifier state.</returns>
        public static KeyMod GetModState()
        {
            return (KeyMod)sdl.GetModState();
        }
    }
}