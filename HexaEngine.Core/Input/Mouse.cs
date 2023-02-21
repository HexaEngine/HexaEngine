namespace HexaEngine.Core.Input
{
    using HexaEngine.Core.Input.Events;
    using Silk.NET.SDL;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using Point = Mathematics.Point;

    public static unsafe class Mouse
    {
#nullable disable
        private static Sdl sdl;
#nullable enable

        private static readonly Dictionary<MouseButton, ButtonState> states = new();
        private static readonly MouseMotionEventArgs mouseMotionEventArgs = new();

        private static Point pos;
        private static Vector2 delta;
        private static Vector2 deltaWheel;

        internal static void Init(Sdl sdl)
        {
            Mouse.sdl = sdl;
            pos = default;
            sdl.GetMouseState(ref pos.X, ref pos.Y);

            uint state = sdl.GetMouseState(null, null);
            uint maskLeft = unchecked(1 << ((int)MouseButton.Left - 1));
            uint maskMiddle = unchecked(1 << ((int)MouseButton.Middle - 1));
            uint maskRight = unchecked(1 << ((int)MouseButton.Right - 1));
            uint maskX1 = unchecked(1 << ((int)MouseButton.X1 - 1));
            uint maskX2 = unchecked(1 << ((int)MouseButton.X2 - 1));
            states.Add(MouseButton.Left, (ButtonState)(state & maskLeft));
            states.Add(MouseButton.Middle, (ButtonState)(state & maskMiddle));
            states.Add(MouseButton.Right, (ButtonState)(state & maskRight));
            states.Add(MouseButton.X1, (ButtonState)(state & maskX1));
            states.Add(MouseButton.X2, (ButtonState)(state & maskX2));
        }

        public static Vector2 Position => pos;

        public static Vector2 Delta => delta;

        public static Vector2 DeltaWheel => deltaWheel;

        public static event EventHandler<MouseMotionEventArgs>? Moved;

        public static event EventHandler<MouseButton>? Pressed;

        public static event EventHandler<MouseButton>? Released;

        public static event EventHandler<Vector2>? Wheel;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDown(MouseButton button)
        {
            return states[button] == ButtonState.Pressed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUp(MouseButton button)
        {
            return states[button] == ButtonState.Pressed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Enqueue(MouseButtonEvent mouseButtonEvent)
        {
            ButtonState state = (ButtonState)mouseButtonEvent.State;
            MouseButton button = (MouseButton)mouseButtonEvent.Button;
            states[button] = state;
            if (state == ButtonState.Released)
                Released?.Invoke(null, button);
            else if (state == ButtonState.Pressed)
                Pressed?.Invoke(null, button);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Enqueue(MouseMotionEvent mouseButtonEvent)
        {
            if (mouseButtonEvent.Xrel == 0 && mouseButtonEvent.Yrel == 0)
                return;
            delta += new Vector2(mouseButtonEvent.Xrel, mouseButtonEvent.Yrel);
            mouseMotionEventArgs.RelX = delta.X;
            mouseMotionEventArgs.RelY = delta.Y;
            mouseMotionEventArgs.X = pos.X;
            mouseMotionEventArgs.Y = pos.Y;
            Moved?.Invoke(null, mouseMotionEventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Enqueue(MouseWheelEvent mouseWheelEvent)
        {
            deltaWheel += new Vector2(mouseWheelEvent.X, mouseWheelEvent.Y);
            Wheel?.Invoke(null, deltaWheel);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ClearState()
        {
            sdl.GetMouseState(ref pos.X, ref pos.Y);
            delta = Vector2.Zero;
            deltaWheel = Vector2.Zero;
        }
    }
}