namespace HexaEngine.Core.Input
{
    using HexaEngine.Core.Input.Events;
    using Silk.NET.SDL;
    using System.Runtime.CompilerServices;

    public static class Joysticks
    {
        private static readonly List<Joystick> joysticks = new();
        private static readonly Dictionary<int, Joystick> idToJoystick = new();

        private static readonly JoystickEventArgs joystickEventArgs = new();

        public static IReadOnlyList<Joystick> Sticks => joysticks;

        public static event EventHandler<JoystickEventArgs>? JoystickAdded;

        public static event EventHandler<JoystickEventArgs>? JoystickRemoved;

        public static event EventHandler<JoystickAxisMotionEventArgs>? AxisMotion;

        public static event EventHandler<JoystickBallMotionEventArgs>? BallMotion;

        public static event EventHandler<JoystickButtonEventArgs>? ButtonDown;

        public static event EventHandler<JoystickButtonEventArgs>? ButtonUp;

        public static event EventHandler<JoystickHatMotionEventArgs>? HatMotion;

        public static Joystick GetById(int joystickId)
        {
            return idToJoystick[joystickId];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddJoystick(JoyDeviceEvent even)
        {
            Joystick joystick = new(even.Which);
            joysticks.Add(joystick);
            idToJoystick.Add(joystick.Id, joystick);
            joystickEventArgs.Timestamp = even.Timestamp;
            joystickEventArgs.JoystickId = even.Which;
            JoystickAdded?.Invoke(joystick, joystickEventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RemoveJoystick(JoyDeviceEvent even)
        {
            Joystick joystick = idToJoystick[even.Which];
            joystickEventArgs.Timestamp = even.Timestamp;
            joystickEventArgs.JoystickId = even.Which;
            JoystickRemoved?.Invoke(joystick, joystickEventArgs);

            joysticks.Remove(joystick);
            idToJoystick.Remove(even.Which);
            joystick.Dispose();
        }

        internal static void OnAxisMotion(JoyAxisEvent even)
        {
            var result = idToJoystick[even.Which].OnAxisMotion(even);
            if (result == null)
                return;
            var value = result.Value;
            AxisMotion?.Invoke(value.Joystick, value.AxisMotionEventArgs);
        }

        internal static void OnBallMotion(JoyBallEvent even)
        {
            var result = idToJoystick[even.Which].OnBallMotion(even);
            BallMotion?.Invoke(result.Joystick, result.BallMotionEventArgs);
        }

        internal static void OnButtonDown(JoyButtonEvent even)
        {
            var result = idToJoystick[even.Which].OnButtonDown(even);
            ButtonDown?.Invoke(result.Joystick, result.ButtonEventArgs);
        }

        internal static void OnButtonUp(JoyButtonEvent even)
        {
            var result = idToJoystick[even.Which].OnButtonUp(even);
            ButtonUp?.Invoke(result.Joystick, result.ButtonEventArgs);
        }

        internal static void OnHatMotion(JoyHatEvent even)
        {
            var result = idToJoystick[even.Which].OnHatMotion(even);
            HatMotion?.Invoke(result.Joystick, result.HatMotionEventArgs);
        }
    }
}