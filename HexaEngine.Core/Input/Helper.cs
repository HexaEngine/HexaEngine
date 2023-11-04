namespace HexaEngine.Core.Input
{
    /// <summary>
    /// A utility class that provides conversion methods for translating between different input and SDL types.
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Converts an SDL key code to its corresponding Key enum value.
        /// </summary>
        /// <param name="code">The SDL key code to convert.</param>
        /// <returns>The corresponding Key enum value.</returns>
        public static Key Convert(Silk.NET.SDL.KeyCode code)
        {
            return (Key)code;
        }

        /// <summary>
        /// Converts a Key enum value to its corresponding SDL key code.
        /// </summary>
        /// <param name="code">The Key enum value to convert.</param>
        /// <returns>The corresponding SDL key code.</returns>
        public static Silk.NET.SDL.KeyCode ConvertBack(Key code)
        {
            return (Silk.NET.SDL.KeyCode)code;
        }

        /// <summary>
        /// Converts a GamepadAxis enum value to its corresponding SDL GameControllerAxis.
        /// </summary>
        /// <param name="gamepadAxis">The GamepadAxis enum value to convert.</param>
        /// <returns>The corresponding SDL GameControllerAxis.</returns>
        public static Silk.NET.SDL.GameControllerAxis ConvertBack(GamepadAxis gamepadAxis)
        {
            return (Silk.NET.SDL.GameControllerAxis)gamepadAxis;
        }

        /// <summary>
        /// Converts an SDL GameControllerAxis to its corresponding GamepadAxis enum value.
        /// </summary>
        /// <param name="axis">The SDL GameControllerAxis to convert.</param>
        /// <returns>The corresponding GamepadAxis enum value.</returns>
        public static GamepadAxis Convert(Silk.NET.SDL.GameControllerAxis axis)
        {
            return (GamepadAxis)axis;
        }

        /// <summary>
        /// Converts a GamepadButton enum value to its corresponding SDL GameControllerButton.
        /// </summary>
        /// <param name="gamepadButton">The GamepadButton enum value to convert.</param>
        /// <returns>The corresponding SDL GameControllerButton.</returns>
        public static Silk.NET.SDL.GameControllerButton ConvertBack(GamepadButton gamepadButton)
        {
            return (Silk.NET.SDL.GameControllerButton)gamepadButton;
        }

        /// <summary>
        /// Converts an SDL GameControllerButton to its corresponding GamepadButton enum value.
        /// </summary>
        /// <param name="button">The SDL GameControllerButton to convert.</param>
        /// <returns>The corresponding GamepadButton enum value.</returns>
        public static GamepadButton Convert(Silk.NET.SDL.GameControllerButton button)
        {
            return (GamepadButton)button;
        }

        /// <summary>
        /// Converts an SDL GameControllerType to its corresponding GamepadType enum value.
        /// </summary>
        /// <param name="gameControllerType">The SDL GameControllerType to convert.</param>
        /// <returns>The corresponding GamepadType enum value.</returns>
        public static GamepadType Convert(Silk.NET.SDL.GameControllerType gameControllerType)
        {
            return (GamepadType)gameControllerType;
        }

        /// <summary>
        /// Converts a GamepadSensorType enum value to its corresponding SDL SensorType.
        /// </summary>
        /// <param name="gamepadSensorType">The GamepadSensorType enum value to convert.</param>
        /// <returns>The corresponding SDL SensorType.</returns>
        public static Silk.NET.SDL.SensorType ConvertBack(GamepadSensorType gamepadSensorType)
        {
            return (Silk.NET.SDL.SensorType)gamepadSensorType;
        }

        /// <summary>
        /// Converts an SDL SensorType to its corresponding GamepadSensorType enum value.
        /// </summary>
        /// <param name="sensorType">The SDL SensorType to convert.</param>
        /// <returns>The corresponding GamepadSensorType enum value.</returns>
        public static GamepadSensorType Convert(Silk.NET.SDL.SensorType sensorType)
        {
            return (GamepadSensorType)sensorType;
        }

        /// <summary>
        /// Converts an SDL JoystickType to its corresponding JoystickType enum value.
        /// </summary>
        /// <param name="joystickType">The SDL JoystickType to convert.</param>
        /// <returns>The corresponding JoystickType enum value.</returns>
        public static JoystickType Convert(Silk.NET.SDL.JoystickType joystickType)
        {
            return (JoystickType)joystickType;
        }

        /// <summary>
        /// Converts an SDL JoystickPowerLevel to its corresponding JoystickPowerLevel enum value.
        /// </summary>
        /// <param name="joystickPowerLevel">The SDL JoystickPowerLevel to convert.</param>
        /// <returns>The corresponding JoystickPowerLevel enum value.</returns>
        public static JoystickPowerLevel Convert(Silk.NET.SDL.JoystickPowerLevel joystickPowerLevel)
        {
            return (JoystickPowerLevel)joystickPowerLevel;
        }
    }
}