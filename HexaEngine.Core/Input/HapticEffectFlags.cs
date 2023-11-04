namespace HexaEngine.Core.Input
{
    /// <summary>
    /// Represents flags that specify the types of haptic effects supported by a haptic feedback device.
    /// </summary>
    [Flags]
    public enum HapticEffectFlags : uint
    {
        /// <summary>
        /// A constant effect (e.g., constant force).
        /// </summary>
        Constant = 1u << 0,

        /// <summary>
        /// A sine wave effect (e.g., periodic vibration).
        /// </summary>
        Sine = 1u << 1,

        /// <summary>
        /// A left-right (waveform) effect (e.g., alternating left and right vibration).
        /// </summary>
        LeftRight = 1u << 2,

        /// <summary>
        /// A triangle wave effect.
        /// </summary>
        Triangle = 1u << 3,

        /// <summary>
        /// A sawtooth-up waveform effect.
        /// </summary>
        SawToothUp = 1u << 4,

        /// <summary>
        /// A sawtooth-down waveform effect.
        /// </summary>
        SawToothDown = 1u << 5,

        /// <summary>
        /// A ramp effect.
        /// </summary>
        Ramp = 1u << 6,

        /// <summary>
        /// A spring effect.
        /// </summary>
        Spring = 1u << 7,

        /// <summary>
        /// A damper effect.
        /// </summary>
        Damper = 1u << 8,

        /// <summary>
        /// An inertia effect.
        /// </summary>
        Inertia = 1u << 9,

        /// <summary>
        /// A friction effect.
        /// </summary>
        Friction = 1u << 10,

        /// <summary>
        /// A custom effect (user-defined).
        /// </summary>
        Custom = 1u << 11,

        /// <summary>
        /// A gain (strength) effect.
        /// </summary>
        Gain = 1u << 12,

        /// <summary>
        /// An auto-centering effect (spring effect to center the joystick).
        /// </summary>
        AutoCenter = 1u << 13,

        /// <summary>
        /// A status effect.
        /// </summary>
        Status = 1u << 14,

        /// <summary>
        /// A pause effect.
        /// </summary>
        Pause = 1u << 15,

        /// <summary>
        /// Represents an infinite value, used for some effect attributes.
        /// </summary>
        Infinity = 4294967295U,
    }
}