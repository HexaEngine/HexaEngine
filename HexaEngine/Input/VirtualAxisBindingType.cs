using HexaEngine.Core.Utilities;

namespace HexaEngine.Input
{
    [Flags]
    public enum VirtualAxisBindingType
    {
        [EnumName("None")]
        None = 0,

        [EnumName("Keyboard Key")]
        KeyboardKey = 1 << 0,

        [EnumName("Mouse Button")]
        MouseButton = 1 << 1,

        [EnumName("Joystick Button")]
        JoystickButton = 1 << 2,

        [EnumName("Gamepad Button")]
        GamepadButton = 1 << 3,

        [EnumName("Gamepad Touch")]
        GamepadTouch = 1 << 4,

        [EnumName("Gamepad Touch Pressure")]
        GamepadTouchPressure = 1 << 5,

        [EnumName("Touch")]
        Touch = 1 << 6,

        [EnumName("Touch Pressure")]
        TouchPressure = 1 << 7,

        [EnumName("Mouse Wheel")]
        MouseWheel = 1 << 8,

        [EnumName("Mouse Movement")]
        MouseMovement = 1 << 9,

        [EnumName("Joystick Ball")]
        JoystickBall = 1 << 10,

        [EnumName("Joystick Axis")]
        JoystickAxis = 1 << 11,

        [EnumName("Joystick Hat")]
        JoystickHat = 1 << 12,

        [EnumName("Gamepad Axis")]
        GamepadAxis = 1 << 13,

        [EnumName("Gamepad Touch Movement")]
        GamepadTouchMovement = 1 << 14,

        [EnumName("Gamepad Sensor")]
        GamepadSensor = 1 << 15,

        [EnumName("Touch Movement")]
        TouchMovement = 1 << 16
    }
}