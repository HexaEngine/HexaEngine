namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Defines logical operations used in graphics rendering.
    /// </summary>
    public enum LogicOperation
    {
        /// <summary>
        /// Clear operation.
        /// </summary>
        Clear = 0,

        /// <summary>
        /// Set operation.
        /// </summary>
        Set = 1,

        /// <summary>
        /// Copy operation.
        /// </summary>
        Copy = 2,

        /// <summary>
        /// Copy Inverted operation.
        /// </summary>
        CopyInverted = 3,

        /// <summary>
        /// No-operation (Noop).
        /// </summary>
        Noop = 4,

        /// <summary>
        /// Invert operation.
        /// </summary>
        Invert = 5,

        /// <summary>
        /// Logical AND operation.
        /// </summary>
        And = 6,

        /// <summary>
        /// Logical NAND operation.
        /// </summary>
        Nand = 7,

        /// <summary>
        /// Logical OR operation.
        /// </summary>
        Or = 8,

        /// <summary>
        /// Logical NOR operation.
        /// </summary>
        Nor = 9,

        /// <summary>
        /// Logical XOR operation.
        /// </summary>
        Xor = 10,

        /// <summary>
        /// Logical Equivalence operation.
        /// </summary>
        Equiv = 11,

        /// <summary>
        /// Logical AND Reverse operation.
        /// </summary>
        AndReverse = 12,

        /// <summary>
        /// Logical AND Inverted operation.
        /// </summary>
        AndInverted = 13,

        /// <summary>
        /// Logical OR Reverse operation.
        /// </summary>
        OrReverse = 14,

        /// <summary>
        /// Logical OR Inverted operation.
        /// </summary>
        OrInverted = 15
    }
}