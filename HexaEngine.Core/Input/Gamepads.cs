namespace HexaEngine.Core.Input
{
    using Hexa.NET.SDL3;
    using HexaEngine.Core.Input.Events;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a generic delegate for handling gamepad events.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of event-specific data or argument.</typeparam>
    /// <param name="sender">The object that raises the event.</param>
    /// <param name="e">The event-specific data or argument.</param>
    public delegate void GamepadEventHandler<TEventArgs>(Gamepad sender, TEventArgs e);

    /// <summary>
    /// Provides a static class to manage gamepad devices and events.
    /// </summary>
    public static unsafe class Gamepads
    {
        private static readonly List<Gamepad> gamepads = new();
        private static readonly Dictionary<int, Gamepad> idToGamepads = new();

        private static readonly GamepadEventArgs gamepadEventArgs = new();

        /// <summary>
        /// Gets a list of all connected gamepad controllers.
        /// </summary>
        public static IReadOnlyList<Gamepad> Controllers => gamepads;

        /// <summary>
        /// Occurs when a gamepad controller is added or connected.
        /// </summary>
        public static event GamepadEventHandler<GamepadEventArgs>? GamepadAdded;

        /// <summary>
        /// Occurs when a gamepad controller is removed or disconnected.
        /// </summary>
        public static event GamepadEventHandler<GamepadEventArgs>? GamepadRemoved;

        /// <summary>
        /// Occurs when a gamepad controller is remapped.
        /// </summary>
        public static event GamepadEventHandler<GamepadRemappedEventArgs>? Remapped;

        /// <summary>
        /// Occurs when a gamepad controller's axis is moved.
        /// </summary>
        public static event GamepadEventHandler<GamepadAxisMotionEventArgs>? AxisMotion;

        /// <summary>
        /// Occurs when a button on a gamepad controller is pressed down.
        /// </summary>
        public static event GamepadEventHandler<GamepadButtonEventArgs>? ButtonDown;

        /// <summary>
        /// Occurs when a button on a gamepad controller is released.
        /// </summary>
        public static event GamepadEventHandler<GamepadButtonEventArgs>? ButtonUp;

        /// <summary>
        /// Occurs when a touchpad on a gamepad controller is touched.
        /// </summary>
        public static event GamepadTouchpadEventHandler<GamepadTouchpadEventArgs>? TouchPadDown;

        /// <summary>
        /// Occurs when a touchpad on a gamepad controller is moved.
        /// </summary>
        public static event GamepadTouchpadEventHandler<GamepadTouchpadMotionEventArgs>? TouchPadMotion;

        /// <summary>
        /// Occurs when a touchpad on a gamepad controller is released.
        /// </summary>
        public static event GamepadTouchpadEventHandler<GamepadTouchpadEventArgs>? TouchPadUp;

        /// <summary>
        /// Occurs when a sensor on a gamepad controller is updated.
        /// </summary>
        public static event GamepadSensorEventHandler<GamepadSensorUpdateEventArgs>? SensorUpdate;

        /// <summary>
        /// Gets a gamepad controller by its unique identifier.
        /// </summary>
        /// <param name="gamepadId">The unique identifier of the gamepad controller.</param>
        /// <returns>The gamepad controller associated with the provided identifier.</returns>
        public static Gamepad GetById(int gamepadId)
        {
            return idToGamepads[gamepadId];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Init()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddController(SDLGamepadDeviceEvent even)
        {
            Gamepad gamepad = new(even.Which);
            gamepads.Add(gamepad);
            idToGamepads.Add(gamepad.Id, gamepad);
            gamepadEventArgs.Timestamp = even.Timestamp;
            gamepadEventArgs.GamepadId = even.Which;
            GamepadAdded?.Invoke(gamepad, gamepadEventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RemoveController(SDLGamepadDeviceEvent even)
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
        internal static void OnRemapped(SDLGamepadDeviceEvent even)
        {
            var result = idToGamepads[even.Which].OnRemapped();
            Remapped?.Invoke(result.Gamepad, result.EventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnAxisMotion(SDLGamepadAxisEvent even)
        {
            var result = idToGamepads[even.Which].OnAxisMotion(even);
            if (result == null)
            {
                return;
            }

            var value = result.Value;
            AxisMotion?.Invoke(value.Gamepad, value.EventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnButtonDown(SDLGamepadButtonEvent even)
        {
            var result = idToGamepads[even.Which].OnButtonDown(even);
            ButtonDown?.Invoke(result.Gamepad, result.EventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnButtonUp(SDLGamepadButtonEvent even)
        {
            var result = idToGamepads[even.Which].OnButtonUp(even);
            ButtonUp?.Invoke(result.Gamepad, result.EventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnTouchPadDown(SDLGamepadTouchpadEvent even)
        {
            var result = idToGamepads[even.Which].OnTouchPadDown(even);
            TouchPadDown?.Invoke(result.Touchpad, result.EventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnTouchPadMotion(SDLGamepadTouchpadEvent even)
        {
            var result = idToGamepads[even.Which].OnTouchPadMotion(even);
            TouchPadMotion?.Invoke(result.Touchpad, result.EventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnTouchPadUp(SDLGamepadTouchpadEvent even)
        {
            var result = idToGamepads[even.Which].OnTouchPadUp(even);
            TouchPadUp?.Invoke(result.Touchpad, result.EventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnSensorUpdate(SDLGamepadSensorEvent even)
        {
            var result = idToGamepads[even.Which].OnSensorUpdate(even);
            SensorUpdate?.Invoke(result.Sensor, result.EventArgs);
        }
    }
}