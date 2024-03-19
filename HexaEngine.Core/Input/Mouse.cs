namespace HexaEngine.Core.Input
{
    using HexaEngine.Core.Input.Events;
    using HexaEngine.Mathematics;
    using Silk.NET.SDL;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using Point2 = Mathematics.Point2;

    /// <summary>
    /// Provides functionality for interacting with the mouse input.
    /// </summary>
    public static unsafe class Mouse
    {
        private static readonly MouseButton[] buttons = Enum.GetValues<MouseButton>();
        private static readonly string[] buttonNames = Enum.GetNames<MouseButton>();

#nullable disable
        private static Sdl sdl;
#nullable enable

        private static readonly Dictionary<MouseButton, MouseButtonState> states = new();
        private static readonly MouseMoveEventArgs motionEventArgs = new();
        private static readonly MouseButtonEventArgs buttonEventArgs = new();
        private static readonly MouseWheelEventArgs wheelEventArgs = new();

        private static Point2 pos;
        private static Vector2 delta;
        private static Vector2 deltaWheel;

        /// <summary>
        /// Initializes the mouse input system.
        /// </summary>
        internal static void Init()
        {
            sdl = Application.sdl;
            pos = default;
            sdl.GetMouseState(ref pos.X, ref pos.Y);

            uint state = sdl.GetMouseState(null, null);
            uint maskLeft = unchecked(1 << ((int)MouseButton.Left - 1));
            uint maskMiddle = unchecked(1 << ((int)MouseButton.Middle - 1));
            uint maskRight = unchecked(1 << ((int)MouseButton.Right - 1));
            uint maskX1 = unchecked(1 << ((int)MouseButton.X1 - 1));
            uint maskX2 = unchecked(1 << ((int)MouseButton.X2 - 1));
            states.Add(MouseButton.Left, (MouseButtonState)(state & maskLeft));
            states.Add(MouseButton.Middle, (MouseButtonState)(state & maskMiddle));
            states.Add(MouseButton.Right, (MouseButtonState)(state & maskRight));
            states.Add(MouseButton.X1, (MouseButtonState)(state & maskX1));
            states.Add(MouseButton.X2, (MouseButtonState)(state & maskX2));
        }

        /// <summary>
        /// Gets the global mouse position.
        /// </summary>
        public static Vector2 Global
        {
            get
            {
                int x, y;
                sdl.GetGlobalMouseState(&x, &y);
                return new Vector2(x, y);
            }
        }

        /// <summary>
        /// Gets the current mouse position.
        /// </summary>
        public static Vector2 Position => pos;

        /// <summary>
        /// Gets the mouse movement delta.
        /// </summary>
        public static Vector2 Delta => delta;

        /// <summary>
        /// Gets the mouse wheel movement delta.
        /// </summary>
        public static Vector2 DeltaWheel => deltaWheel;

        /// <summary>
        /// Gets a list of available mouse buttons.
        /// </summary>
        public static IReadOnlyList<MouseButton> Buttons => buttons;

        /// <summary>
        /// Gets a list of mouse button names.
        /// </summary>
        public static IReadOnlyList<string> ButtonNames => buttonNames;

        /// <summary>
        /// Gets the current state of mouse buttons.
        /// </summary>
        public static IDictionary<MouseButton, MouseButtonState> States => states;

        /// <summary>
        /// Event triggered when the mouse is moved.
        /// </summary>
        public static event EventHandler<MouseMoveEventArgs>? Moved;

        /// <summary>
        /// Event triggered when a mouse button is pressed.
        /// </summary>
        public static event EventHandler<MouseButtonEventArgs>? ButtonDown;

        /// <summary>
        /// Event triggered when a mouse button is released.
        /// </summary>
        public static event EventHandler<MouseButtonEventArgs>? ButtonUp;

        /// <summary>
        /// Event triggered when the mouse wheel is scrolled.
        /// </summary>
        public static event EventHandler<MouseWheelEventArgs>? Wheel;

        /// <summary>
        /// Checks if a mouse button is in the "Down" state.
        /// </summary>
        /// <param name="button">The mouse button to check.</param>
        /// <returns><c>true</c> if the button is in the "Down" state, otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDown(MouseButton button)
        {
            return states[button] == MouseButtonState.Down;
        }

        /// <summary>
        /// Checks if a mouse button is in the "Up" state.
        /// </summary>
        /// <param name="button">The mouse button to check.</param>
        /// <returns><c>true</c> if the button is in the "Up" state, otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUp(MouseButton button)
        {
            return states[button] == MouseButtonState.Down;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnButtonDown(MouseButtonEvent mouseButtonEvent)
        {
            MouseButton button = (MouseButton)mouseButtonEvent.Button;
            states[button] = MouseButtonState.Down;
            buttonEventArgs.Timestamp = mouseButtonEvent.Timestamp;
            buttonEventArgs.Handled = false;
            buttonEventArgs.MouseId = mouseButtonEvent.Which;
            buttonEventArgs.Button = button;
            buttonEventArgs.State = MouseButtonState.Down;
            buttonEventArgs.Clicks = mouseButtonEvent.Clicks;
            ButtonDown?.Invoke(null, buttonEventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnButtonUp(MouseButtonEvent mouseButtonEvent)
        {
            MouseButton button = (MouseButton)mouseButtonEvent.Button;
            states[button] = MouseButtonState.Up;
            buttonEventArgs.Timestamp = mouseButtonEvent.Timestamp;
            buttonEventArgs.Handled = false;
            buttonEventArgs.MouseId = mouseButtonEvent.Which;
            buttonEventArgs.Button = button;
            buttonEventArgs.State = MouseButtonState.Up;
            buttonEventArgs.Clicks = mouseButtonEvent.Clicks;
            ButtonUp?.Invoke(null, buttonEventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnMotion(MouseMotionEvent mouseMotionEvent)
        {
            if (mouseMotionEvent.Xrel == 0 && mouseMotionEvent.Yrel == 0)
            {
                return;
            }

            delta += new Vector2(mouseMotionEvent.Xrel, mouseMotionEvent.Yrel);
            motionEventArgs.Timestamp = mouseMotionEvent.Timestamp;
            motionEventArgs.Handled = false;
            motionEventArgs.MouseId = mouseMotionEvent.Which;
            motionEventArgs.RelX = delta.X;
            motionEventArgs.RelY = delta.Y;
            motionEventArgs.X = pos.X;
            motionEventArgs.Y = pos.Y;
            Moved?.Invoke(null, motionEventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnWheel(MouseWheelEvent mouseWheelEvent)
        {
            deltaWheel += new Vector2(mouseWheelEvent.X, mouseWheelEvent.Y);
            wheelEventArgs.Timestamp = mouseWheelEvent.Timestamp;
            wheelEventArgs.Handled = false;
            wheelEventArgs.MouseId = mouseWheelEvent.Which;
            wheelEventArgs.Wheel = new Vector2(mouseWheelEvent.X, mouseWheelEvent.Y);
            wheelEventArgs.Direction = (MouseWheelDirection)mouseWheelEvent.Direction;
            Wheel?.Invoke(null, wheelEventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Flush()
        {
            sdl.GetMouseState(ref pos.X, ref pos.Y);
            delta = Vector2.Zero;
            deltaWheel = Vector2.Zero;
        }

        /// <summary>
        /// Converts the screen coordinates to world coordinates.
        /// </summary>
        /// <param name="proj">The projection matrix.</param>
        /// <param name="viewInv">The inverse view matrix.</param>
        /// <param name="viewport">The viewport settings.</param>
        /// <returns>The world coordinates in a 3D vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ScreenToWorld(Matrix4x4 proj, Matrix4x4 viewInv, Viewport viewport)
        {
            var vx = (2.0f * (pos.X - viewport.X) / viewport.Width - 1.0f) / proj.M11;
            var vy = (-2.0f * (pos.Y - viewport.Y) / viewport.Height + 1.0f) / proj.M22;
            Vector3 rayDirViewSpace = new(vx, vy, 1);
            Vector3 rayDir = Vector3.TransformNormal(rayDirViewSpace, viewInv);
            return Vector3.Normalize(rayDir);
        }

        /// <summary>
        /// Converts screen coordinates to UV coordinates.
        /// </summary>
        /// <param name="viewport">The viewport settings.</param>
        /// <returns>The UV coordinates in a 2D vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ScreenToUV(Viewport viewport)
        {
            var u = (pos.X - viewport.X) / viewport.Width;
            var v = (pos.Y - viewport.Y) / viewport.Height;
            return new Vector2(u, v);
        }
    }
}