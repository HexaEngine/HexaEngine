namespace HexaEngine.Core.Graphics.Reflection
{
    using System;

    /// <summary>
    /// Specifies the tessellator partitioning scheme.
    /// </summary>
    [Flags]
    public enum TessellatorPartitioning
    {
        /// <summary>
        /// Undefined tessellator partitioning scheme.
        /// </summary>
        Undefined = 0x0,

        /// <summary>
        /// Integer tessellator partitioning scheme.
        /// </summary>
        Integer = 0x1,

        /// <summary>
        /// Power-of-two tessellator partitioning scheme.
        /// </summary>
        Pow2 = 0x2,

        /// <summary>
        /// Fractional odd tessellator partitioning scheme.
        /// </summary>
        FractionalOdd = 0x3,

        /// <summary>
        /// Fractional even tessellator partitioning scheme.
        /// </summary>
        FractionalEven = 0x4,
    }
}