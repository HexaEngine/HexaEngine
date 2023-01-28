namespace HexaEngine.Core.Input
{
    using HexaEngine.Core.Input.Events;
    using Silk.NET.SDL;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;

    public static class Keyboard
    {
#nullable disable
        private static Sdl sdl;
#nullable enable

        private static readonly Dictionary<KeyCode, KeyState> states = new();
        private static readonly List<KeyCode> pushed = new();
        private static readonly KeyboardEventArgs keyboardEventArgs = new();
        private static readonly KeyboardCharEventArgs keyboardCharEventArgs = new();

        internal static unsafe void Init(Sdl sdl)
        {
            Keyboard.sdl = sdl;
            int numkeys;
            byte* keys = sdl.GetKeyboardState(&numkeys);
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                var scancode = (KeyCode)sdl.GetScancodeFromKey((int)key);
                states.Add(key, (KeyState)keys[(int)scancode]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Enqueue(KeyboardEvent keyboardEvent)
        {
            KeyState state = (KeyState)keyboardEvent.State;
            KeyCode keyCode = (KeyCode)sdl.GetKeyFromScancode(keyboardEvent.Keysym.Scancode);
            states[keyCode] = state;
            keyboardEventArgs.KeyState = state;
            keyboardEventArgs.KeyCode = keyCode;
            if (state == KeyState.Released)
            {
                Released?.Invoke(null, keyboardEventArgs);
                pushed.Add(keyCode);
            }
            else if (state == KeyState.Pressed)
            {
                Pressed?.Invoke(null, keyboardEventArgs);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Enqueue(TextInputEvent textInputEvent)
        {
            unsafe
            {
                keyboardCharEventArgs.Char = Encoding.UTF8.GetString(textInputEvent.Text, 1)[0];
            }
            Text?.Invoke(null, keyboardCharEventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ClearState()
        {
            pushed.Clear();
        }

        public static event EventHandler<KeyboardEventArgs>? Pressed;

        public static event EventHandler<KeyboardEventArgs>? Released;

        public static event EventHandler<KeyboardCharEventArgs>? Text;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUp(KeyCode n)
        {
            return states[n] == KeyState.Released;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDown(KeyCode n)
        {
            return states[n] == KeyState.Pressed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WasPushed(KeyCode key)
        {
            return pushed.Contains(key);
        }
    }
}