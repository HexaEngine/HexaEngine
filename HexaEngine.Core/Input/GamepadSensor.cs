namespace HexaEngine.Core.Input
{
    using Hexa.NET.SDL3;
    using HexaEngine.Core.Input.Events;
    using System.Numerics;
    using static Extensions.SdlErrorHandlingExtensions;

    /// <summary>
    /// Represents a generic delegate for handling gamepad sensor events.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of event-specific data or argument.</typeparam>
    /// <param name="sender">The object that raises the sensor event.</param>
    /// <param name="e">The event-specific data or argument.</param>
    public delegate void GamepadSensorEventHandler<TEventArgs>(GamepadSensor sender, TEventArgs e);

    /// <summary>
    /// Represents a sensor on a gamepad that provides data updates.
    /// </summary>
    public unsafe class GamepadSensor : IDisposable
    {
        private readonly SDLGamepad* controller;
        private readonly GamepadSensorType type;
        private readonly float* buffer;
        private readonly int length = 3;

        private readonly GamepadSensorUpdateEventArgs sensorUpdateEventArgs = new();

        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="GamepadSensor"/> class.
        /// </summary>
        /// <param name="controller">The game controller associated with the sensor.</param>
        /// <param name="sensorType">The type of sensor.</param>
        public GamepadSensor(SDLGamepad* controller, GamepadSensorType sensorType)
        {
            this.controller = controller;
            this.type = sensorType;
            buffer = AllocT<float>(3);
            SDL.GetGamepadSensorData(controller, Helper.ConvertBack(sensorType), buffer, length);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the sensor is enabled.
        /// </summary>
        public bool Enabled
        {
            get => SDL.GamepadSensorEnabled(controller, Helper.ConvertBack(type));
            set => SDL.SetGamepadSensorEnabled(controller, Helper.ConvertBack(type), value);
        }

        /// <summary>
        /// Gets the type of the sensor.
        /// </summary>
        public GamepadSensorType Type => type;

        /// <summary>
        /// Gets the data from the sensor as a span of float values.
        /// </summary>
        public Span<float> Data => new(buffer, length);

        /// <summary>
        /// Gets the sensor data as a Vector3.
        /// </summary>
        public Vector3 Vector => *(Vector3*)buffer;

        /// <summary>
        /// Occurs when the sensor data is updated.
        /// </summary>
        public event GamepadSensorEventHandler<GamepadSensorUpdateEventArgs>? SensorUpdate;

        internal (GamepadSensor Sensor, GamepadSensorUpdateEventArgs EventArgs) OnSensorUpdate(SDLGamepadSensorEvent even)
        {
            MemcpyT(&even.Data_0, buffer, length);
            sensorUpdateEventArgs.Timestamp = even.Timestamp;
            sensorUpdateEventArgs.Handled = false;
            sensorUpdateEventArgs.GamepadId = even.Which;
            sensorUpdateEventArgs.Data = buffer;
            sensorUpdateEventArgs.Length = length;
            sensorUpdateEventArgs.Type = type;
            SensorUpdate?.Invoke(this, sensorUpdateEventArgs);
            return (this, sensorUpdateEventArgs);
        }

        /// <summary>
        /// Flushes and updates the sensor data.
        /// </summary>
        public void Flush()
        {
            SDL.GetGamepadSensorData(controller, Helper.ConvertBack(type), buffer, length);
            sensorUpdateEventArgs.Data = buffer;
            sensorUpdateEventArgs.Length = length;
            sensorUpdateEventArgs.Type = type;
            SensorUpdate?.Invoke(this, sensorUpdateEventArgs);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Free(buffer);
                disposedValue = true;
            }
        }

        /// <summary>
        /// Releases the resources used by the <see cref="GamepadSensor"/>.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}