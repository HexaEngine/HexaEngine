namespace HexaEngine.Core.Input
{
    using HexaEngine.Core.Input.Events;
    using Silk.NET.SDL;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides management and events for joysticks connected to the system.
    /// </summary>
    public static class Joysticks
    {
        private static readonly List<Joystick> joysticks = new();
        private static readonly Dictionary<int, Joystick> idToJoystick = new();

        private static readonly JoystickEventArgs joystickEventArgs = new();

        /// <summary>
        /// Gets a read-only list of connected joysticks.
        /// </summary>
        public static IReadOnlyList<Joystick> Sticks => joysticks;

        /// <summary>
        /// Event that occurs when a joystick is added or connected to the system.
        /// </summary>
        public static event JoystickEventHandler<JoystickEventArgs>? JoystickAdded;

        /// <summary>
        /// Event that occurs when a joystick is removed or disconnected from the system.
        /// </summary>
        public static event JoystickEventHandler<JoystickEventArgs>? JoystickRemoved;

        /// <summary>
        /// Event that occurs when there is axis motion on a joystick.
        /// </summary>
        public static event JoystickEventHandler<JoystickAxisMotionEventArgs>? AxisMotion;

        /// <summary>
        /// Event that occurs when there is ball motion on a joystick.
        /// </summary>
        public static event JoystickEventHandler<JoystickBallMotionEventArgs>? BallMotion;

        /// <summary>
        /// Event that occurs when a button on a joystick is pressed down.
        /// </summary>
        public static event JoystickEventHandler<JoystickButtonEventArgs>? ButtonDown;

        /// <summary>
        /// Event that occurs when a button on a joystick is released.
        /// </summary>
        public static event JoystickEventHandler<JoystickButtonEventArgs>? ButtonUp;

        /// <summary>
        /// Event that occurs when there is motion of the hat control on a joystick.
        /// </summary>
        public static event JoystickEventHandler<JoystickHatMotionEventArgs>? HatMotion;

        /// <summary>
        /// Gets a joystick by its unique identifier.
        /// </summary>
        /// <param name="joystickId">The unique identifier of the joystick.</param>
        /// <returns>The joystick with the specified identifier.</returns>
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
            {
                return;
            }

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