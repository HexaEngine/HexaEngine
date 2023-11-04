namespace HexaEngine.Core.Input.Events
{
    using System.Numerics;

    /// <summary>
    /// Provides data for a gamepad sensor update event.
    /// </summary>
    public class GamepadSensorUpdateEventArgs : GamepadEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GamepadSensorUpdateEventArgs"/> class.
        /// </summary>
        public GamepadSensorUpdateEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GamepadSensorUpdateEventArgs"/> class with sensor data.
        /// </summary>
        /// <param name="type">The type of sensor.</param>
        /// <param name="data">A pointer to the sensor data.</param>
        /// <param name="length">The length of the sensor data.</param>
        public unsafe GamepadSensorUpdateEventArgs(GamepadSensorType type, float* data, int length)
        {
            Type = type;
            Data = data;
            Length = length;
        }

        /// <summary>
        /// Gets the type of the sensor.
        /// </summary>
        public GamepadSensorType Type { get; internal set; }

        /// <summary>
        /// Gets the corresponding sensor object for this sensor update event.
        /// </summary>
        public GamepadSensor GamepadSensor => Gamepad.Sensors[Type];

        /// <summary>
        /// Gets a pointer to the sensor data.
        /// </summary>
        public unsafe float* Data { get; internal set; }

        /// <summary>
        /// Gets the length of the sensor data.
        /// </summary>
        public int Length { get; internal set; }

        /// <summary>
        /// Gets the sensor data as a <see cref="Vector3"/>.
        /// </summary>
        public unsafe Vector3 Vector => *(Vector3*)Data;
    }
}