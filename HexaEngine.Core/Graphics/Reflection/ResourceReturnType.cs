namespace HexaEngine.Core.Graphics.Reflection
{
    using System;

    /// <summary>
    /// Represents the return type of a shader resource in HLSL.
    /// </summary>
    [Flags]
    public enum ResourceReturnType
    {
        /// <summary>
        /// No return type specified.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Unsigned normalized integer return type.
        /// </summary>
        UNorm = 0x1,

        /// <summary>
        /// Signed normalized integer return type.
        /// </summary>
        SNorm = 0x2,

        /// <summary>
        /// Signed integer return type.
        /// </summary>
        SInt = 0x3,

        /// <summary>
        /// Unsigned integer return type.
        /// </summary>
        UInt = 0x4,

        /// <summary>
        /// Floating-point return type.
        /// </summary>
        Float = 0x5,

        /// <summary>
        /// Mixed return type.
        /// </summary>
        Mixed = 0x6,

        /// <summary>
        /// Double-precision floating-point return type.
        /// </summary>
        Double = 0x7,

        /// <summary>
        /// Continued return type.
        /// </summary>
        Continued = 0x8,
    }
}