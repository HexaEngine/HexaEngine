namespace HexaEngine.Core.Input
{
    /// <summary>
    /// Defines the types of sensors available on a gamepad.
    /// </summary>
    public enum GamepadSensorType
    {
        /// <summary>
        /// The sensor type is unknown.
        /// </summary>
        Unknown = 0x0,

        /// <summary>
        /// The sensor type is an accelerometer.
        /// </summary>
        Accel = 0x1,

        /// <summary>
        /// The sensor type is a gyroscope.
        /// </summary>
        Gyro = 0x2,

        /// <summary>
        /// The sensor type is a left accelerometer.
        /// </summary>
        AccelLeft = 0x3,

        /// <summary>
        /// The sensor type is a left gyroscope.
        /// </summary>
        GyroLeft = 0x4,

        /// <summary>
        /// The sensor type is a right accelerometer.
        /// </summary>
        AccelRight = 0x5,

        /// <summary>
        /// The sensor type is a right gyroscope.
        /// </summary>
        GyroRight = 0x6,
    }
}