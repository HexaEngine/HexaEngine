namespace HexaEngine.Core.Input
{
    /// <summary>
    /// A utility class that provides conversion methods for translating between different input and SDL types.
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Converts a GamepadAxis enum value to its corresponding SDL GameControllerAxis.
        /// </summary>
        /// <param name="gamepadAxis">The GamepadAxis enum value to convert.</param>
        /// <returns>The corresponding SDL GameControllerAxis.</returns>
        public static Hexa.NET.SDL3.SDLGamepadAxis ConvertBack(GamepadAxis gamepadAxis)
        {
            return (Hexa.NET.SDL3.SDLGamepadAxis)gamepadAxis;
        }

        /// <summary>
        /// Converts an SDL GameControllerAxis to its corresponding GamepadAxis enum value.
        /// </summary>
        /// <param name="axis">The SDL GameControllerAxis to convert.</param>
        /// <returns>The corresponding GamepadAxis enum value.</returns>
        public static GamepadAxis Convert(Hexa.NET.SDL3.SDLGamepadAxis axis)
        {
            return (GamepadAxis)axis;
        }

        /// <summary>
        /// Converts a GamepadButton enum value to its corresponding SDL GameControllerButton.
        /// </summary>
        /// <param name="gamepadButton">The GamepadButton enum value to convert.</param>
        /// <returns>The corresponding SDL GameControllerButton.</returns>
        public static Hexa.NET.SDL3.SDLGamepadButton ConvertBack(GamepadButton gamepadButton)
        {
            return (Hexa.NET.SDL3.SDLGamepadButton)gamepadButton;
        }

        /// <summary>
        /// Converts an SDL GameControllerButton to its corresponding GamepadButton enum value.
        /// </summary>
        /// <param name="button">The SDL GameControllerButton to convert.</param>
        /// <returns>The corresponding GamepadButton enum value.</returns>
        public static GamepadButton Convert(Hexa.NET.SDL3.SDLGamepadButton button)
        {
            return (GamepadButton)button;
        }

        /// <summary>
        /// Converts an SDL GameControllerType to its corresponding GamepadType enum value.
        /// </summary>
        /// <param name="gameControllerType">The SDL GameControllerType to convert.</param>
        /// <returns>The corresponding GamepadType enum value.</returns>
        public static GamepadType Convert(Hexa.NET.SDL3.SDLGamepadType gameControllerType)
        {
            return (GamepadType)gameControllerType;
        }

        /// <summary>
        /// Converts a GamepadSensorType enum value to its corresponding SDL SensorType.
        /// </summary>
        /// <param name="gamepadSensorType">The GamepadSensorType enum value to convert.</param>
        /// <returns>The corresponding SDL SensorType.</returns>
        public static Hexa.NET.SDL3.SDLSensorType ConvertBack(GamepadSensorType gamepadSensorType)
        {
            return (Hexa.NET.SDL3.SDLSensorType)gamepadSensorType;
        }

        /// <summary>
        /// Converts an SDL SensorType to its corresponding GamepadSensorType enum value.
        /// </summary>
        /// <param name="sensorType">The SDL SensorType to convert.</param>
        /// <returns>The corresponding GamepadSensorType enum value.</returns>
        public static GamepadSensorType Convert(Hexa.NET.SDL3.SDLSensorType sensorType)
        {
            return (GamepadSensorType)sensorType;
        }

        /// <summary>
        /// Converts an SDL JoystickType to its corresponding JoystickType enum value.
        /// </summary>
        /// <param name="joystickType">The SDL JoystickType to convert.</param>
        /// <returns>The corresponding JoystickType enum value.</returns>
        public static JoystickType Convert(Hexa.NET.SDL3.SDLJoystickType joystickType)
        {
            return (JoystickType)joystickType;
        }

        /// <summary>
        /// Converts an SDL JoystickPowerLevel to its corresponding JoystickPowerLevel enum value.
        /// </summary>
        /// <param name="joystickPowerLevel">The SDL JoystickPowerLevel to convert.</param>
        /// <returns>The corresponding JoystickPowerLevel enum value.</returns>
        public static JoystickConnectionState Convert(Hexa.NET.SDL3.SDLJoystickConnectionState joystickPowerLevel)
        {
            return (JoystickConnectionState)joystickPowerLevel;
        }

        /// <summary>
        /// Converts an SDL PowerState to its corresponding PowerState enum value.
        /// </summary>
        /// <param name="powerState">The SDL PowerState to convert.</param>
        /// <returns>The corresponding PowerState enum value.</returns>
        public static PowerState Convert(Hexa.NET.SDL3.SDLPowerState powerState)
        {
            return (PowerState)powerState;
        }
    }
}