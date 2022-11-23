namespace HexaEngine.Core.Input
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Input.Events;
    using Silk.NET.SDL;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using Point = Mathematics.Point;

    public static unsafe class Mouse
    {
#nullable disable
        private static Sdl sdl;
#nullable enable
        private static readonly ConcurrentQueue<MouseButtonEvent> buttonEvents = new();

        private static readonly ConcurrentQueue<MouseMotionEvent> motionEvents = new();
        private static readonly ConcurrentQueue<MouseWheelEvent> wheelEvents = new();
        private static readonly Dictionary<MouseButton, ButtonState> states = new();
        private static readonly MouseMotionEventArgs mouseMotionEventArgs = new();

        private static Point* pos;
        private static Vector2 delta;
        private static Vector2 deltaWheel;

        internal static void Init(Sdl sdl)
        {
            Mouse.sdl = sdl;
            pos = Utilities.Alloc<Point>();
            sdl.GetMouseState(&pos->X, &pos->Y);

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

        public static Vector2 Position => *pos;

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
            buttonEvents.Enqueue(mouseButtonEvent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Enqueue(MouseMotionEvent mouseButtonEvent)
        {
            if (mouseButtonEvent.Xrel == 0 && mouseButtonEvent.Yrel == 0)
                return;
            motionEvents.Enqueue(mouseButtonEvent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Enqueue(MouseWheelEvent mouseButtonEvent)
        {
            wheelEvents.Enqueue(mouseButtonEvent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ProcessInput()
        {
            sdl.GetMouseState(&pos->X, &pos->Y);

            Vector2 del = new();
            while (motionEvents.TryDequeue(out var evnt))
            {
                del += new Vector2(evnt.Xrel, evnt.Yrel);
            }
            delta = del;

            if (del != Vector2.Zero)
            {
                mouseMotionEventArgs.RelX = del.X;
                mouseMotionEventArgs.RelY = del.Y;
                mouseMotionEventArgs.X = pos->X;
                mouseMotionEventArgs.Y = pos->Y;
                Moved?.Invoke(null, mouseMotionEventArgs);
            }

            while (buttonEvents.TryDequeue(out var evnt))
            {
                ButtonState state = (ButtonState)evnt.State;
                MouseButton button = (MouseButton)evnt.Button;
                states[button] = state;
                if (state == ButtonState.Released)
                    Released?.Invoke(null, button);
                else if (state == ButtonState.Pressed)
                    Pressed?.Invoke(null, button);
            }

            Vector2 delWheel = new();
            while (wheelEvents.TryDequeue(out var evnt))
            {
                delWheel += new Vector2(evnt.X, evnt.Y);
            }
            deltaWheel = delWheel;

            if (delWheel != Vector2.Zero)
            {
                Wheel?.Invoke(null, delWheel);
            }
        }
    }
}