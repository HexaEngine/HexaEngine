namespace HexaEngine.Core.Input
{
    using HexaEngine.Core.Input.Events;
    using Silk.NET.SDL;
    using System.Runtime.CompilerServices;

    public static unsafe class Gamepads
    {
        private static readonly List<Gamepad> gamepads = new();
        private static readonly Dictionary<int, Gamepad> idToGamepads = new();

        private static readonly GamepadEventArgs gamepadEventArgs = new();

        public static IReadOnlyList<Gamepad> Controllers => gamepads;

        public static event EventHandler<GamepadEventArgs>? GamepadAdded;

        public static event EventHandler<GamepadEventArgs>? GamepadRemoved;

        public static event EventHandler<GamepadRemappedEventArgs>? Remapped;

        public static event EventHandler<GamepadAxisMotionEventArgs>? AxisMotion;

        public static event EventHandler<GamepadButtonEventArgs>? ButtonDown;

        public static event EventHandler<GamepadButtonEventArgs>? ButtonUp;

        public static event EventHandler<GamepadTouchpadEventArgs>? TouchPadDown;

        public static event EventHandler<GamepadTouchpadMotionEventArgs>? TouchPadMotion;

        public static event EventHandler<GamepadTouchpadEventArgs>? TouchPadUp;

        public static event EventHandler<GamepadSensorUpdateEventArgs>? SensorUpdate;

        public static Gamepad GetById(int gamepadId)
        {
            return idToGamepads[gamepadId];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Init()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddController(ControllerDeviceEvent even)
        {
            Gamepad gamepad = new(even.Which);
            gamepads.Add(gamepad);
            idToGamepads.Add(gamepad.Id, gamepad);
            gamepadEventArgs.Timestamp = even.Timestamp;
            gamepadEventArgs.GamepadId = even.Which;
            GamepadAdded?.Invoke(gamepad, gamepadEventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RemoveController(ControllerDeviceEvent even)
        {
            Gamepad gamepad = idToGamepads[even.Which];
            gamepadEventArgs.Timestamp = even.Timestamp;
            gamepadEventArgs.GamepadId = even.Which;
            GamepadRemoved?.Invoke(gamepad, gamepadEventArgs);

            gamepads.Remove(gamepad);
            idToGamepads.Remove(even.Which);
            gamepad.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnRemapped(ControllerDeviceEvent even)
        {
            var result = idToGamepads[even.Which].OnRemapped();
            Remapped?.Invoke(result.Gamepad, result.EventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnAxisMotion(ControllerAxisEvent even)
        {
            var result = idToGamepads[even.Which].OnAxisMotion(even);
            if (result == null)
                return;
            var value = result.Value;
            AxisMotion?.Invoke(value.Gamepad, value.EventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnButtonDown(ControllerButtonEvent even)
        {
            var result = idToGamepads[even.Which].OnButtonDown(even);
            ButtonDown?.Invoke(result.Gamepad, result.EventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnButtonUp(ControllerButtonEvent even)
        {
            var result = idToGamepads[even.Which].OnButtonUp(even);
            ButtonUp?.Invoke(result.Gamepad, result.EventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnTouchPadDown(ControllerTouchpadEvent even)
        {
            var result = idToGamepads[even.Which].OnTouchPadDown(even);
            TouchPadDown?.Invoke(result.Touchpad, result.EventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnTouchPadMotion(ControllerTouchpadEvent even)
        {
            var result = idToGamepads[even.Which].OnTouchPadMotion(even);
            TouchPadMotion?.Invoke(result.Touchpad, result.EventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnTouchPadUp(ControllerTouchpadEvent even)
        {
            var result = idToGamepads[even.Which].OnTouchPadUp(even);
            TouchPadUp?.Invoke(result.Touchpad, result.EventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnSensorUpdate(ControllerSensorEvent even)
        {
            var result = idToGamepads[even.Which].OnSensorUpdate(even);
            SensorUpdate?.Invoke(result.Sensor, result.EventArgs);
        }
    }
}