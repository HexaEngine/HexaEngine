namespace HexaEngine.Input
{
    using HexaEngine.Input.Events;

    public static class Helper
    {
        public static AxisType ConvertToAxisType(InputEventType type)
        {
            return type switch
            {
                InputEventType.None => throw new NotSupportedException(),
                InputEventType.MouseMoved => AxisType.MouseMovement,
                InputEventType.MouseUp => AxisType.MouseButton,
                InputEventType.MouseDown => AxisType.MouseButton,
                InputEventType.MouseWheel => AxisType.MouseWheel,
                InputEventType.KeyboardKeyDown => AxisType.KeyboardKey,
                InputEventType.KeyboardKeyUp => AxisType.KeyboardKey,
                InputEventType.JoystickAxisMotion => AxisType.JoystickAxis,
                InputEventType.JoystickBallMotion => AxisType.JoystickBall,
                InputEventType.JoystickButtonDown => AxisType.JoystickButton,
                InputEventType.JoystickButtonUp => AxisType.JoystickButton,
                InputEventType.JoystickHatMotion => AxisType.JoystickHat,
                InputEventType.GamepadAxisMotion => AxisType.GamepadAxis,
                InputEventType.GamepadButtonDown => AxisType.GamepadButton,
                InputEventType.GamepadButtonUp => AxisType.GamepadButton,
                InputEventType.GamepadSensorUpdate => AxisType.GamepadSensor,
                InputEventType.GamepadTouchPadDown => AxisType.GamepadTouch,
                InputEventType.GamepadTouchPadUp => AxisType.GamepadTouch,
                InputEventType.GamepadTouchPadMotion => AxisType.GamepadTouchMovement,
                InputEventType.TouchDeviceTouchDown => AxisType.Touch,
                InputEventType.TouchDeviceTouchUp => AxisType.Touch,
                InputEventType.TouchDeviceTouchMotion => AxisType.TouchMovement,
                _ => throw new NotSupportedException(),
            };
        }

        public static VirtualAxisBindingType ConvertToAxisBindingType(InputEventType type)
        {
            return type switch
            {
                InputEventType.None => throw new NotSupportedException(),
                InputEventType.MouseMoved => VirtualAxisBindingType.MouseMovement,
                InputEventType.MouseUp => VirtualAxisBindingType.MouseButton,
                InputEventType.MouseDown => VirtualAxisBindingType.MouseButton,
                InputEventType.MouseWheel => VirtualAxisBindingType.MouseWheel,
                InputEventType.KeyboardKeyDown => VirtualAxisBindingType.KeyboardKey,
                InputEventType.KeyboardKeyUp => VirtualAxisBindingType.KeyboardKey,
                InputEventType.JoystickAxisMotion => VirtualAxisBindingType.JoystickAxis,
                InputEventType.JoystickBallMotion => VirtualAxisBindingType.JoystickBall,
                InputEventType.JoystickButtonDown => VirtualAxisBindingType.JoystickButton,
                InputEventType.JoystickButtonUp => VirtualAxisBindingType.JoystickButton,
                InputEventType.JoystickHatMotion => VirtualAxisBindingType.JoystickHat,
                InputEventType.GamepadAxisMotion => VirtualAxisBindingType.GamepadAxis,
                InputEventType.GamepadButtonDown => VirtualAxisBindingType.GamepadButton,
                InputEventType.GamepadButtonUp => VirtualAxisBindingType.GamepadButton,
                InputEventType.GamepadSensorUpdate => VirtualAxisBindingType.GamepadSensor,
                InputEventType.GamepadTouchPadDown => VirtualAxisBindingType.GamepadTouch,
                InputEventType.GamepadTouchPadUp => VirtualAxisBindingType.GamepadTouch,
                InputEventType.GamepadTouchPadMotion => VirtualAxisBindingType.GamepadTouchMovement,
                InputEventType.TouchDeviceTouchDown => VirtualAxisBindingType.Touch,
                InputEventType.TouchDeviceTouchUp => VirtualAxisBindingType.Touch,
                InputEventType.TouchDeviceTouchMotion => VirtualAxisBindingType.TouchMovement,
                _ => throw new NotSupportedException(),
            };
        }
    }
}