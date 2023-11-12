namespace HexaEngine.Core.Graphics.Reflection
{
    using System;

    /// <summary>
    /// Specifies the output primitive type of a tessellator.
    /// </summary>
    [Flags]
    public enum TessellatorOutputPrimitive
    {
        /// <summary>
        /// Undefined output primitive type.
        /// </summary>
        Undefined = 0x0,

        /// <summary>
        /// Output as points.
        /// </summary>
        Point = 0x1,

        /// <summary>
        /// Output as lines.
        /// </summary>
        Line = 0x2,

        /// <summary>
        /// Output as clockwise-oriented triangles.
        /// </summary>
        TriangleCW = 0x3,

        /// <summary>
        /// Output as counter-clockwise-oriented triangles.
        /// </summary>
        TriangleCcw = 0x4,
    }
}