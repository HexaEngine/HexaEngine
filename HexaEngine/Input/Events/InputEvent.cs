namespace HexaEngine.Input
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    public struct InputEvent
    {
        public InputEvent(MouseMotionEventArgs eventArgs) : this(0, eventArgs)
        {
        }

        public InputEvent(MouseButtonEventArgs eventArgs) : this(0, eventArgs)
        {
        }

        public InputEvent(MouseWheelEventArgs eventArgs) : this(0, eventArgs)
        {
        }

        public InputEvent(KeyboardEventArgs eventArgs) : this(0, eventArgs)
        {
        }

        public InputEvent(JoystickAxisMotionEventArgs eventArgs) : this(0, eventArgs)
        {
        }

        public InputEvent(JoystickBallMotionEventArgs eventArgs) : this(0, eventArgs)
        {
        }

        public InputEvent(JoystickButtonEventArgs eventArgs) : this(0, eventArgs)
        {
        }

        public InputEvent(JoystickHatMotionEventArgs eventArgs) : this(0, eventArgs)
        {
        }

        public InputEvent(GamepadAxisMotionEventArgs eventArgs) : this(0, eventArgs)
        {
        }

        public InputEvent(GamepadButtonEventArgs eventArgs) : this(0, eventArgs)
        {
        }

        public InputEvent(GamepadSensorUpdateEventArgs eventArgs) : this(0, eventArgs)
        {
        }

        public InputEvent(GamepadTouchpadEventArgs eventArgs) : this(0, eventArgs)
        {
        }

        public InputEvent(GamepadTouchpadMotionEventArgs eventArgs) : this(0, eventArgs)
        {
        }

        public InputEvent(TouchEventArgs eventArgs) : this(0, eventArgs)
        {
        }

        public InputEvent(TouchMotionEventArgs eventArgs) : this(0, eventArgs)
        {
        }

        public InputEvent(long timestamp, MouseMotionEventArgs eventArgs)
        {
            Timestamp = timestamp;
            Type = InputEventType.MouseMoved;
            DeviceId = eventArgs.MouseId;
            MouseMovedEvent = new(eventArgs);
        }

        public InputEvent(long timestamp, MouseButtonEventArgs eventArgs)
        {
            Timestamp = timestamp;
            Type = eventArgs.State == MouseButtonState.Up ? InputEventType.MouseUp : InputEventType.MouseDown;
            DeviceId = eventArgs.MouseId;
            MouseEvent = new(eventArgs);
        }

        public InputEvent(long timestamp, MouseWheelEventArgs eventArgs)
        {
            Timestamp = timestamp;
            Type = InputEventType.MouseWheel;
            DeviceId = eventArgs.MouseId;
            MouseWheelEvent = new(eventArgs);
        }

        public InputEvent(long timestamp, KeyboardEventArgs eventArgs)
        {
            Timestamp = timestamp;
            Type = eventArgs.State == KeyState.Up ? InputEventType.KeyboardKeyUp : InputEventType.KeyboardKeyDown;
            DeviceId = 0;
            KeyboardEvent = new(eventArgs);
        }

        public InputEvent(long timestamp, JoystickAxisMotionEventArgs eventArgs)
        {
            Timestamp = timestamp;
            Type = InputEventType.JoystickAxisMotion;
            DeviceId = eventArgs.JoystickId;
            JoystickAxisMotionEvent = new(eventArgs);
        }

        public InputEvent(long timestamp, JoystickBallMotionEventArgs eventArgs)
        {
            Timestamp = timestamp;
            Type = InputEventType.JoystickBallMotion;
            DeviceId = eventArgs.JoystickId;
            JoystickBallMotionEvent = new(eventArgs);
        }

        public InputEvent(long timestamp, JoystickButtonEventArgs eventArgs)
        {
            Timestamp = timestamp;
            Type = eventArgs.State == JoystickButtonState.Up ? InputEventType.JoystickButtonUp : InputEventType.JoystickButtonDown;
            DeviceId = eventArgs.JoystickId;
            JoystickButtonEvent = new(eventArgs);
        }

        public InputEvent(long timestamp, JoystickHatMotionEventArgs eventArgs)
        {
            Timestamp = timestamp;
            Type = InputEventType.JoystickHatMotion;
            DeviceId = eventArgs.JoystickId;
            JoystickHatMotionEvent = new(eventArgs);
        }

        public InputEvent(long timestamp, GamepadAxisMotionEventArgs eventArgs)
        {
            Timestamp = timestamp;
            Type = InputEventType.GamepadAxisMotion;
            DeviceId = eventArgs.GamepadId;
            GamepadAxisMotionEvent = new(eventArgs);
        }

        public InputEvent(long timestamp, GamepadButtonEventArgs eventArgs)
        {
            Timestamp = timestamp;
            Type = eventArgs.State == GamepadButtonState.Up ? InputEventType.GamepadButtonUp : InputEventType.GamepadButtonDown;
            DeviceId = eventArgs.GamepadId;
            GamepadButtonEvent = new(eventArgs);
        }

        public InputEvent(long timestamp, GamepadSensorUpdateEventArgs eventArgs)
        {
            Timestamp = timestamp;
            Type = InputEventType.GamepadSensorUpdate;
            DeviceId = eventArgs.GamepadId;
            GamepadSensorUpdateEvent = new(eventArgs);
        }

        public InputEvent(long timestamp, GamepadTouchpadEventArgs eventArgs)
        {
            Timestamp = timestamp;
            Type = eventArgs.State == FingerState.Up ? InputEventType.GamepadTouchPadUp : InputEventType.GamepadTouchPadDown;
            DeviceId = eventArgs.GamepadId;
            GamepadTouchPadEvent = new(eventArgs);
        }

        public InputEvent(long timestamp, GamepadTouchpadMotionEventArgs eventArgs)
        {
            Timestamp = timestamp;
            Type = InputEventType.GamepadTouchPadMotion;
            DeviceId = eventArgs.GamepadId;
            GamepadTouchPadMotionEvent = new(eventArgs);
        }

        public InputEvent(long timestamp, TouchEventArgs eventArgs)
        {
            Timestamp = timestamp;
            Type = eventArgs.State == FingerState.Up ? InputEventType.TouchDeviceTouchUp : InputEventType.TouchDeviceTouchDown;
            DeviceId = eventArgs.TouchDeviceId;
            TouchDeviceTouchEvent = new(eventArgs);
        }

        public InputEvent(long timestamp, TouchMotionEventArgs eventArgs)
        {
            Timestamp = timestamp;
            Type = InputEventType.TouchDeviceTouchMotion;
            DeviceId = eventArgs.TouchDeviceId;
            TouchDeviceTouchMotionEvent = new(eventArgs);
        }

        /// <summary>
        /// The timestamp of the event in ticks.
        /// </summary>
        [FieldOffset(0)]
        public long Timestamp;

        /// <summary>
        /// The event type.
        /// </summary>
        [FieldOffset(8)]
        public InputEventType Type;

        /// <summary>
        /// The device id that issued the event.
        /// </summary>
        [FieldOffset(12)]
        public long DeviceId;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.MouseMoved"/>.
        /// </summary>
        [FieldOffset(20)]
        public MouseMovedEvent MouseMovedEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.MouseDown"/> <see cref="InputEventType.MouseUp"/>.
        /// </summary>
        [FieldOffset(20)]
        public MouseEvent MouseEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.MouseWheel"/>.
        /// </summary>
        [FieldOffset(20)]
        public MouseWheelEvent MouseWheelEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.KeyboardKeyDown"/> <see cref="InputEventType.KeyboardKeyUp"/>.
        /// </summary>
        [FieldOffset(20)]
        public KeyboardEvent KeyboardEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.JoystickAxisMotion"/>.
        /// </summary>
        [FieldOffset(20)]
        public JoystickAxisMotionEvent JoystickAxisMotionEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.JoystickBallMotion"/>.
        /// </summary>
        [FieldOffset(20)]
        public JoystickBallMotionEvent JoystickBallMotionEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.JoystickButtonDown"/> <see cref="InputEventType.JoystickButtonUp"/>.
        /// </summary>
        [FieldOffset(20)]
        public JoystickButtonEvent JoystickButtonEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.JoystickHatMotion"/>.
        /// </summary>
        [FieldOffset(20)]
        public JoystickHatMotionEvent JoystickHatMotionEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.GamepadAxisMotion"/>.
        /// </summary>
        [FieldOffset(20)]
        public GamepadAxisMotionEvent GamepadAxisMotionEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.GamepadButtonDown"/> <see cref="InputEventType.GamepadButtonUp"/>.
        /// </summary>
        [FieldOffset(20)]
        public GamepadButtonEvent GamepadButtonEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.GamepadSensorUpdate"/>.
        /// </summary>
        [FieldOffset(20)]
        public GamepadSensorUpdateEvent GamepadSensorUpdateEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.GamepadTouchPadDown"/> <see cref="InputEventType.GamepadTouchPadUp"/>.
        /// </summary>
        [FieldOffset(20)]
        public GamepadTouchPadEvent GamepadTouchPadEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.GamepadTouchPadMotion"/>.
        /// </summary>
        [FieldOffset(20)]
        public GamepadTouchPadMotionEvent GamepadTouchPadMotionEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.TouchDeviceTouchDown"/> <see cref="InputEventType.TouchDeviceTouchUp"/>.
        /// </summary>
        [FieldOffset(20)]
        public TouchDeviceTouchEvent TouchDeviceTouchEvent;

        /// <summary>
        /// Event data for event type <see cref="InputEventType.TouchDeviceTouchMotion"/>.
        /// </summary>
        [FieldOffset(20)]
        public TouchDeviceTouchMotionEvent TouchDeviceTouchMotionEvent;
    }
}