namespace HexaEngine.Core.Input
{
    using HexaEngine.Core.Input.Events;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public static class Keyboard
    {
        private static readonly List<Keys> pushed = new();

        static Keyboard()
        {
            foreach (Keys button in Enum.GetValues(typeof(Keys)))
            {
                if (!States.ContainsKey(button))
                    States.Add(button, KeyState.Released);
            }
        }

        public static Dictionary<Keys, KeyState> States { get; } = new();

        public static bool GlobalInput { get; set; }

        public static event EventHandler<KeyboardEventArgs>? OnKeyDown;

        public static event EventHandler<KeyboardEventArgs>? OnKeyUp;

        public static event EventHandler<KeyboardCharEventArgs>? OnChar;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Update(KeyboardEventArgs args)
        {
            KeyState state = States[args.KeyCode] = args.KeyState;
            if (state == KeyState.Pressed)
            {
                OnKeyDown?.Invoke(null, args);
                pushed.Add(args.KeyCode);
            }
            else if (state == KeyState.Released)
            {
                OnKeyUp?.Invoke(null, args);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FrameUpdate()
        {
            pushed.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Update(KeyboardCharEventArgs args)
        {
            OnChar?.Invoke(null, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDown(Keys n)
        {
            return States[n] == KeyState.Pressed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WasPushed(Keys key)
        {
            return pushed.Contains(key);
        }
    }
}