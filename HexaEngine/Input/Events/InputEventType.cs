namespace HexaEngine.Input.Events
{
    [Flags]
    public enum InputEventType
    {
        None = 0,

        /// <summary>
        /// Use <see cref="InputEvent.MouseMovedEvent"/> to access.
        /// </summary>
        MouseMoved = 1 << 1,

        /// <summary>
        /// Use <see cref="InputEvent.MouseEvent"/> to access.
        /// </summary>
        MouseUp = 1 << 2,

        /// <summary>
        /// Use <see cref="InputEvent.MouseEvent"/> to access.
        /// </summary>
        MouseDown = 1 << 3,

        /// <summary>
        /// Use <see cref="InputEvent.MouseWheelEvent"/> to access.
        /// </summary>
        MouseWheel = 1 << 4,

        /// <summary>
        /// Use <see cref="InputEvent.KeyboardEvent"/> to access.
        /// </summary>
        KeyboardKeyDown = 1 << 5,

        /// <summary>
        /// Use <see cref="InputEvent.KeyboardEvent"/> to access.
        /// </summary>
        KeyboardKeyUp = 1 << 6,

        /// <summary>
        /// Use <see cref="InputEvent.JoystickAxisMotionEvent"/> to access.
        /// </summary>
        JoystickAxisMotion = 1 << 7,

        /// <summary>
        /// Use <see cref="InputEvent.JoystickBallMotionEvent"/> to access.
        /// </summary>
        JoystickBallMotion = 1 << 8,

        /// <summary>
        /// Use <see cref="InputEvent.JoystickButtonEvent"/> to access.
        /// </summary>
        JoystickButtonDown = 1 << 9,

        /// <summary>
        /// Use <see cref="InputEvent.JoystickButtonEvent"/> to access.
        /// </summary>
        JoystickButtonUp = 1 << 10,

        /// <summary>
        /// Use <see cref="InputEvent.JoystickHatMotionEvent"/> to access.
        /// </summary>
        JoystickHatMotion = 1 << 11,

        /// <summary>
        /// Use <see cref="InputEvent.GamepadAxisMotionEvent"/> to access.
        /// </summary>
        GamepadAxisMotion = 1 << 12,

        /// <summary>
        /// Use <see cref="InputEvent.GamepadButtonEvent"/> to access.
        /// </summary>
        GamepadButtonDown = 1 << 13,

        /// <summary>
        /// Use <see cref="InputEvent.GamepadButtonEvent"/> to access.
        /// </summary>
        GamepadButtonUp = 1 << 14,

        /// <summary>
        /// Use <see cref="InputEvent.GamepadSensorUpdateEvent"/> to access.
        /// </summary>
        GamepadSensorUpdate = 1 << 15,

        /// <summary>
        /// Use <see cref="InputEvent.GamepadTouchPadEvent"/> to access.
        /// </summary>
        GamepadTouchPadDown = 1 << 16,

        /// <summary>
        /// Use <see cref="InputEvent.GamepadTouchPadEvent"/> to access.
        /// </summary>
        GamepadTouchPadUp = 1 << 17,

        /// <summary>
        /// Use <see cref="InputEvent.GamepadTouchPadMotionEvent"/> to access.
        /// </summary>
        GamepadTouchPadMotion = 1 << 18,

        /// <summary>
        /// Use <see cref="InputEvent.TouchDeviceTouchEvent"/> to access.
        /// </summary>
        TouchDeviceTouchDown = 1 << 19,

        /// <summary>
        /// Use <see cref="InputEvent.TouchDeviceTouchEvent"/> to access.
        /// </summary>
        TouchDeviceTouchUp = 1 << 20,

        /// <summary>
        /// Use <see cref="InputEvent.TouchDeviceTouchMotionEvent"/> to access.
        /// </summary>
        TouchDeviceTouchMotion = 1 << 21
    }
}