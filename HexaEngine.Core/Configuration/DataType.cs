namespace HexaEngine.Core.Configuration
{
    /// <summary>
    /// Represents a data type for <see cref="ConfigValue"/>
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// Represents a data type for string values.
        /// </summary>
        String,

        /// <summary>
        /// Represents a data type for boolean (true or false) values.
        /// </summary>
        Bool,

        /// <summary>
        /// Represents an unsigned 8-bit integer data type.
        /// </summary>
        UInt8,

        /// <summary>
        /// Represents a signed 8-bit integer data type.
        /// </summary>
        Int8,

        /// <summary>
        /// Represents an unsigned 16-bit integer data type.
        /// </summary>
        UInt16,

        /// <summary>
        /// Represents a signed 16-bit integer data type.
        /// </summary>
        Int16,

        /// <summary>
        /// Represents an unsigned 32-bit integer data type.
        /// </summary>
        UInt32,

        /// <summary>
        /// Represents a signed 32-bit integer data type.
        /// </summary>
        Int32,

        /// <summary>
        /// Represents an unsigned 64-bit integer data type.
        /// </summary>
        UInt64,

        /// <summary>
        /// Represents a signed 64-bit integer data type.
        /// </summary>
        Int64,

        /// <summary>
        /// Represents a single-precision floating-point data type.
        /// </summary>
        Float,

        /// <summary>
        /// Represents a double-precision floating-point data type.
        /// </summary>
        Double,

        /// <summary>
        /// Represents a 2D vector of single-precision floating-point values.
        /// </summary>
        Float2,

        /// <summary>
        /// Represents a 3D vector of single-precision floating-point values.
        /// </summary>
        Float3,

        /// <summary>
        /// Represents a 4D vector of single-precision floating-point values.
        /// </summary>
        Float4,

        /// <summary>
        /// Represents a color data type with red, green, blue, and alpha components.
        /// </summary>
        ColorRGBA,

        /// <summary>
        /// Represents a color data type with red, green, and blue components.
        /// </summary>
        ColorRGB,

        /// <summary>
        /// Represents a data type for button values (possibly for user interface purposes).
        /// </summary>
        Button,

        /// <summary>
        /// Represents a data type for key values (e.g., keyboard keys).
        /// </summary>
        Keys,

        /// <summary>
        /// Represents a data type for enumerated values with predefined options.
        /// </summary>
        Enum
    }
}