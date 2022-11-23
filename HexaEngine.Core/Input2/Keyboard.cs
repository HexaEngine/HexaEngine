namespace HexaEngine.Core.Input2
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Input2.Events;
    using Silk.NET.SDL;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;

    public static class Keyboard
    {
        private static readonly Sdl sdl = Sdl.GetApi();
        private static readonly ConcurrentQueue<KeyboardEvent> events = new();
        private static readonly ConcurrentQueue<TextInputEvent> eventsText = new();
        private static readonly Dictionary<KeyCode, KeyState> states = new();
        private static readonly List<KeyCode> pushed = new();
        private static readonly KeyboardEventArgs keyboardEventArgs = new();
        private static readonly KeyboardCharEventArgs keyboardCharEventArgs = new();

        static unsafe Keyboard()
        {
            int numkeys;
            byte* keys = sdl.GetKeyboardState(&numkeys);
            foreach (Scancode scancode in Enum.GetValues(typeof(Scancode)))
            {
                var key = (KeyCode)sdl.GetKeyFromScancode(scancode);
                states.Add(key, (KeyState)(keys[(int)scancode]));
            }
        }

        internal static void EnqueueEvent(KeyboardEvent keyboardEvent)
        {
            events.Enqueue(keyboardEvent);
        }

        internal static void EnqueueText(TextInputEvent textInputEvent)
        {
            eventsText.Enqueue(textInputEvent);
        }

        internal static void ProcessInput()
        {
            pushed.Clear();
            while (events.TryDequeue(out var evnt))
            {
                KeyState state = (KeyState)evnt.State;
                KeyCode keyCode = (KeyCode)sdl.GetKeyFromScancode(evnt.Keysym.Scancode);
                keyboardEventArgs.KeyState = state;
                keyboardEventArgs.KeyCode = keyCode;
                if (state == KeyState.Released)
                {
                    OnKeyUp?.Invoke(null, keyboardEventArgs);
                }
                else if (state == KeyState.Pressed)
                {
                    OnKeyDown?.Invoke(null, keyboardEventArgs);
                }
                else
                {
                    ImGuiConsole.Log(keyCode.ToString());
                }
            }

            while (eventsText.TryDequeue(out var evnt))
            {
                unsafe
                {
                    keyboardCharEventArgs.Char = Encoding.UTF8.GetString(evnt.Text, 1)[0];
                    OnChar?.Invoke(null, keyboardCharEventArgs);
                }
            }
        }

        public static event EventHandler<KeyboardEventArgs>? OnKeyDown;

        public static event EventHandler<KeyboardEventArgs>? OnKeyUp;

        public static event EventHandler<KeyboardCharEventArgs>? OnChar;

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