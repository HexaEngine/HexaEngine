namespace HexaEngine.Core.IO.Binary.Materials
{
    /// <summary>
    /// Enumeration representing different types of material values.
    /// </summary>
    public enum MaterialValueType
    {
        /// <summary>
        /// Unknown material value type.
        /// </summary>
        Unknown,

        /// <summary>
        /// Single-precision floating-point material value type.
        /// </summary>
        Float,

        /// <summary>
        /// Two-component single-precision floating-point material value type.
        /// </summary>
        Float2,

        /// <summary>
        /// Three-component single-precision floating-point material value type.
        /// </summary>
        Float3,

        /// <summary>
        /// Four-component single-precision floating-point material value type.
        /// </summary>
        Float4,

        /// <summary>
        /// Boolean material value type.
        /// </summary>
        Bool,

        /// <summary>
        /// 8-bit unsigned integer material value type.
        /// </summary>
        UInt8,

        /// <summary>
        /// 16-bit unsigned integer material value type.
        /// </summary>
        UInt16,

        /// <summary>
        /// 32-bit unsigned integer material value type.
        /// </summary>
        UInt32,

        /// <summary>
        /// 64-bit unsigned integer material value type.
        /// </summary>
        UInt64,

        /// <summary>
        /// 8-bit signed integer material value type.
        /// </summary>
        Int8,

        /// <summary>
        /// 16-bit signed integer material value type.
        /// </summary>
        Int16,

        /// <summary>
        /// 32-bit signed integer material value type.
        /// </summary>
        Int32,

        /// <summary>
        /// 64-bit signed integer material value type.
        /// </summary>
        Int64,

        /// <summary>
        /// String material value type.
        /// </summary>
        String,
    }
}