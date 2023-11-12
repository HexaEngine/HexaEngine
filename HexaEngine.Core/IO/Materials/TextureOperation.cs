namespace HexaEngine.Core.IO.Materials
{
    /// <summary>
    /// Enumeration representing different texture operations.
    /// </summary>
    public enum TextureOp
    {
        /// <summary>
        /// No texture operation.
        /// </summary>
        None = 0,

        /// <summary>
        /// Multiply texture operation.
        /// </summary>
        Multiply = 0x0,

        /// <summary>
        /// Add texture operation.
        /// </summary>
        Add = 0x1,

        /// <summary>
        /// Subtract texture operation.
        /// </summary>
        Subtract = 0x2,

        /// <summary>
        /// Divide texture operation.
        /// </summary>
        Divide = 0x3,

        /// <summary>
        /// Smooth add texture operation.
        /// </summary>
        SmoothAdd = 0x4,

        /// <summary>
        /// Signed add texture operation.
        /// </summary>
        SignedAdd = 0x5
    }
}