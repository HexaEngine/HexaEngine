namespace HexaEngine.Core.IO.Metadata
{
    /// <summary>
    /// Represents the types of metadata entries.
    /// </summary>
    public enum MetadataType
    {
        /// <summary>
        /// Boolean metadata type.
        /// </summary>
        Bool,

        /// <summary>
        /// 16-bit signed integer metadata type.
        /// </summary>
        Int16,

        /// <summary>
        /// 16-bit unsigned integer metadata type.
        /// </summary>
        UInt16,

        /// <summary>
        /// 32-bit signed integer metadata type.
        /// </summary>
        Int32,

        /// <summary>
        /// 32-bit unsigned integer metadata type.
        /// </summary>
        UInt32,

        /// <summary>
        /// 64-bit signed integer metadata type.
        /// </summary>
        Int64,

        /// <summary>
        /// 64-bit unsigned integer metadata type.
        /// </summary>
        UInt64,

        /// <summary>
        /// Single-precision floating-point metadata type.
        /// </summary>
        Float,

        /// <summary>
        /// Double-precision floating-point metadata type.
        /// </summary>
        Double,

        /// <summary>
        /// String metadata type.
        /// </summary>
        String,

        /// <summary>
        /// Two-component floating-point vector metadata type.
        /// </summary>
        Float2,

        /// <summary>
        /// Three-component floating-point vector metadata type.
        /// </summary>
        Float3,

        /// <summary>
        /// Four-component floating-point vector metadata type.
        /// </summary>
        Float4,

        /// <summary>
        /// Four-by-four matrix metadata type.
        /// </summary>
        Float4x4,

        /// <summary>
        /// Metadata metadata type, representing nested metadata.
        /// </summary>
        Metadata,
    }
}