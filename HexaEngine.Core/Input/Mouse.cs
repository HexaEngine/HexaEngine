namespace HexaEngine.Core.Input
{
    using HexaEngine.Core.Events;
    using HexaEngine.Core.Input.Events;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Numerics;

    public static class Mouse
    {
        private static readonly Dictionary<MouseButton, KeyState> buttons = new();

        static Mouse()
        {
            foreach (MouseButton button in Enum.GetValues(typeof(MouseButton)))
            {
                buttons.Add(button, KeyState.Released);
            }
        }

        public static bool IsDown(MouseButton button)
        {
            return buttons[button] == KeyState.Pressed;
        }

        public static IReadOnlyDictionary<MouseButton, KeyState> Buttons => buttons;

        public static Vector2 PositionVector { get; private set; }

        public static Point Position { get; private set; }

        public static bool Hover { get; private set; }

        private static Vector2 Delta;

        public static Vector2 WheelDelta;

        public static void Update(MouseMotionEventArgs args)
        {
            Position = new Point(args.X, args.Y);
            PositionVector = new Vector2(args.X, args.Y);
            Delta += new Vector2(args.RelX, args.RelY);
        }

        public static void Update(MouseButtonEventArgs mouseButtonDown)
        {
            buttons[mouseButtonDown.MouseButton] = mouseButtonDown.KeyState;
        }

        public static void Update(EnterEventArgs args)
        {
            Hover = !args.Handled || Hover;
        }

        public static void Update(LeaveEventArgs args)
        {
            Hover = args.Handled && Hover;
        }

        public static void Update(MouseWheelEventArgs args)
        {
            WheelDelta = new Vector2(args.X, args.Y);
        }

        public static Vector2 GetDelta()
        {
            return Delta;
        }

        public static void Clear()
        {
            Delta = Vector2.Zero;
            WheelDelta = Vector2.Zero;
        }
    }
}