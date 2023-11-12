namespace HexaEngine.Core.Graphics.Reflection
{
    using System;

    /// <summary>
    /// Represents the component type of a register in HLSL.
    /// </summary>
    [Flags]
    public enum RegisterComponentType
    {
        /// <summary>
        /// Unknown component type.
        /// </summary>
        Unknown = 0x0,

        /// <summary>
        /// 32-bit unsigned integer component type.
        /// </summary>
        Uint32 = 0x1,

        /// <summary>
        /// 32-bit signed integer component type.
        /// </summary>
        Sint32 = 0x2,

        /// <summary>
        /// 32-bit floating-point component type.
        /// </summary>
        Float32 = 0x3,
    }
}