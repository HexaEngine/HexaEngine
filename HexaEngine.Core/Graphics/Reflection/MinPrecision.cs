namespace HexaEngine.Core.Graphics.Reflection
{
    /// <summary>
    /// Represents the minimum precision qualifier for numerical types in HLSL.
    /// </summary>
    public enum MinPrecision
    {
        /// <summary>
        /// Default precision.
        /// </summary>
        Default = 0,

        /// <summary>
        /// 16-bit floating-point precision.
        /// </summary>
        Float16 = 1,

        /// <summary>
        /// 28-bit floating-point precision.
        /// </summary>
        Float28 = 2,

        /// <summary>
        /// Reserved precision.
        /// </summary>
        Reserved = 3,

        /// <summary>
        /// 16-bit signed integer precision.
        /// </summary>
        Sint16 = 4,

        /// <summary>
        /// 16-bit unsigned integer precision.
        /// </summary>
        Uint16 = 5,

        /// <summary>
        /// Any 16-bit precision.
        /// </summary>
        Any16 = 240,

        /// <summary>
        /// Any 10-bit precision.
        /// </summary>
        Any10 = 241
    }
}