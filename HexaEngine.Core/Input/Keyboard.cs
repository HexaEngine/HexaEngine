namespace HexaEngine.Core.Input
{
    using HexaEngine.Core.Input.Events;
    using Silk.NET.SDL;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;

    public static class Keyboard
    {
#nullable disable
        private static Sdl sdl;
#nullable enable
        private static readonly ConcurrentQueue<KeyboardEvent> events = new();
        private static readonly ConcurrentQueue<TextInputEvent> eventsText = new();
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
            events.Enqueue(keyboardEvent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Enqueue(TextInputEvent textInputEvent)
        {
            eventsText.Enqueue(textInputEvent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ProcessInput()
        {
            pushed.Clear();
            while (events.TryDequeue(out var evnt))
            {
                KeyState state = (KeyState)evnt.State;
                KeyCode keyCode = (KeyCode)sdl.GetKeyFromScancode(evnt.Keysym.Scancode);
                states[keyCode] = state;
                keyboardEventArgs.KeyState = state;
                keyboardEventArgs.KeyCode = keyCode;
                if (state == KeyState.Released)
                {
                    Released?.Invoke(null, keyboardEventArgs);
                }
                else if (state == KeyState.Pressed)
                {
                    Pressed?.Invoke(null, keyboardEventArgs);
                }
            }

            while (eventsText.TryDequeue(out var evnt))
            {
                unsafe
                {
                    keyboardCharEventArgs.Char = Encoding.UTF8.GetString(evnt.Text, 1)[0];
                    Text?.Invoke(null, keyboardCharEventArgs);
                }
            }
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