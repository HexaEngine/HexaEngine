namespace HexaEngine.Input.Events
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    public struct InputEvent
    {
        public InputEvent(MouseMoveEventArgs eventArgs)
        {
            Timestamp = eventArgs.Timestamp;
            Type = InputEventType.MouseMoved;
            DeviceId = eventArgs.MouseId;
            MouseMovedEvent = new(eventArgs);
        }

        public InputEvent(MouseButtonEventArgs eventArgs)
        {
            Timestamp = eventArgs.Timestamp;
            Type = eventArgs.State == MouseButtonState.Up ? InputEventType.MouseUp : InputEventType.MouseDown;
            DeviceId = eventArgs.MouseId;
            MouseEvent = new(eventArgs);
        }

        public InputEvent(MouseWheelEventArgs eventArgs)
        {
            Timestamp = eventArgs.Timestamp;
            Type = InputEventType.MouseWheel;
            DeviceId = eventArgs.MouseId;
            MouseWheelEvent = new(eventArgs);
        }

        public InputEvent(KeyboardEventArgs eventArgs)
        {
            Timestamp = eventArgs.Timestamp;
            Type = eventArgs.State == KeyState.Up ? InputEventType.KeyboardKeyUp : InputEventType.KeyboardKeyDown;
            DeviceId = 0;
            KeyboardEvent = new(eventArgs);
        }

        public InputEvent(JoystickAxisMotionEventArgs eventArgs)
        {
            Timestamp = eventArgs.Timestamp;
            Type = InputEventType.JoystickAxisMotion;
            DeviceId = eventArgs.JoystickId;
            JoystickAxisMotionEvent = new(eventArgs);
        }

        public InputEvent(JoystickBallMotionEventArgs eventArgs)
        {
            Timestamp = eventArgs.Timestamp;
            Type = InputEventType.JoystickBallMotion;
            DeviceId = eventArgs.JoystickId;
            JoystickBallMotionEvent = new(eventArgs);
        }

        public InputEvent(JoystickButtonEventArgs eventArgs)
        {
            Timestamp = eventArgs.Timestamp;
            Type = eventArgs.State == JoystickButtonState.Up ? InputEventType.JoystickButtonUp : InputEventType.JoystickButtonDown;
            DeviceId = eventArgs.JoystickId;
            JoystickButtonEvent = new(eventArgs);
        }

        public InputEvent(JoystickHatMotionEventArgs eventArgs)
        {
            Timestamp = eventArgs.Timestamp;
            Type = InputEventType.JoystickHatMotion;
            DeviceId = eventArgs.JoystickId;
            JoystickHatMotionEvent = new(eventArgs);
        }

        public InputEvent(GamepadAxisMotionEventArgs eventArgs)
        {
            Timestamp = eventArgs.Timestamp;
            Type = InputEventType.GamepadAxisMotion;
            DeviceId = eventArgs.GamepadId;
            GamepadAxisMotionEvent = new(eventArgs);
        }

        public InputEvent(GamepadButtonEventArgs eventArgs)
        {
            Timestamp = eventArgs.Timestamp;
            Type = eventArgs.State == GamepadButtonState.Up ? InputEventType.GamepadButtonUp : InputEventType.GamepadButtonDown;
            DeviceId = eventArgs.GamepadId;
            GamepadButtonEvent = new(eventArgs);
        }

        public InputEvent(GamepadSensorUpdateEventArgs eventArgs)
        {
            Timestamp = eventArgs.Timestamp;
            Type = InputEventType.GamepadSensorUpdate;
            DeviceId = eventArgs.GamepadId;
            GamepadSensorUpdateEvent = new(eventArgs);
        }

        public InputEvent(GamepadTouchpadEventArgs eventArgs)
        {
            Timestamp = eventArgs.Timestamp;
            Type = eventArgs.State == FingerState.Up ? InputEventType.GamepadTouchPadUp : InputEventType.GamepadTouchPadDown;
            DeviceId = eventArgs.GamepadId;
            GamepadTouchPadEvent = new(eventArgs);
        }

        public InputEvent(GamepadTouchpadMotionEventArgs eventArgs)
        {
            Timestamp = eventArgs.Timestamp;
            Type = InputEventType.GamepadTouchPadMotion;
            DeviceId = eventArgs.GamepadId;
            GamepadTouchPadMotionEvent = new(eventArgs);
        }

        public InputEvent(TouchEventArgs eventArgs)
        {
            Timestamp = eventArgs.Timestamp;
            Type = eventArgs.State == FingerState.Up ? InputEventType.TouchDeviceTouchUp : InputEventType.TouchDeviceTouchDown;
            DeviceId = eventArgs.TouchDeviceId;
            TouchDeviceTouchEvent = new(eventArgs);
        }

        public InputEvent(TouchMoveEventArgs eventArgs)
        {
            Timestamp = eventArgs.Timestamp;
            Type = InputEventType.TouchDeviceTouchMotion;
            DeviceId = eventArgs.TouchDeviceId;
            TouchDeviceTouchMotionEvent = new(eventArgs);
        }

        /// <summary>
        /// The timestamp of the event in milliseconds since app start.
        /// </summary>
        [FieldOffset(0)]
        public uint Timestamp;

        /// <summary>
        /// The event type.
        /// </summary>
        [FieldOffset(4)]
        public InputEventType Type;

        /// <summary>
        /// The device id that issued the event.
        /// </summary>
        [FieldOffset(8)]
        public long DeviceId;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.MouseMoved"/>.
        /// </summary>
        [FieldOffset(16)]
        public MouseMovedEvent MouseMovedEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.MouseDown"/> <see cref="InputEventType.MouseUp"/>.
        /// </summary>
        [FieldOffset(16)]
        public MouseEvent MouseEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.MouseWheel"/>.
        /// </summary>
        [FieldOffset(16)]
        public MouseWheelEvent MouseWheelEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.KeyboardKeyDown"/> <see cref="InputEventType.KeyboardKeyUp"/>.
        /// </summary>
        [FieldOffset(16)]
        public KeyboardEvent KeyboardEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.JoystickAxisMotion"/>.
        /// </summary>
        [FieldOffset(16)]
        public JoystickAxisMotionEvent JoystickAxisMotionEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.JoystickBallMotion"/>.
        /// </summary>
        [FieldOffset(16)]
        public JoystickBallMotionEvent JoystickBallMotionEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.JoystickButtonDown"/> <see cref="InputEventType.JoystickButtonUp"/>.
        /// </summary>
        [FieldOffset(16)]
        public JoystickButtonEvent JoystickButtonEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.JoystickHatMotion"/>.
        /// </summary>
        [FieldOffset(16)]
        public JoystickHatMotionEvent JoystickHatMotionEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.GamepadAxisMotion"/>.
        /// </summary>
        [FieldOffset(16)]
        public GamepadAxisMotionEvent GamepadAxisMotionEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.GamepadButtonDown"/> <see cref="InputEventType.GamepadButtonUp"/>.
        /// </summary>
        [FieldOffset(16)]
        public GamepadButtonEvent GamepadButtonEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.GamepadSensorUpdate"/>.
        /// </summary>
        [FieldOffset(16)]
        public GamepadSensorUpdateEvent GamepadSensorUpdateEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.GamepadTouchPadDown"/> <see cref="InputEventType.GamepadTouchPadUp"/>.
        /// </summary>
        [FieldOffset(16)]
        public GamepadTouchPadEvent GamepadTouchPadEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.GamepadTouchPadMotion"/>.
        /// </summary>
        [FieldOffset(16)]
        public GamepadTouchPadMotionEvent GamepadTouchPadMotionEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.TouchDeviceTouchDown"/> <see cref="InputEventType.TouchDeviceTouchUp"/>.
        /// </summary>
        [FieldOffset(16)]
        public TouchDeviceTouchEvent TouchDeviceTouchEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.TouchDeviceTouchMotion"/>.
        /// </summary>
        [FieldOffset(16)]
        public TouchDeviceTouchMotionEvent TouchDeviceTouchMotionEvent;
    }
}