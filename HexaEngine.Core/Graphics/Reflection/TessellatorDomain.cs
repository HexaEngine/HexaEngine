namespace HexaEngine.Core.Graphics.Reflection
{
    using System;

    /// <summary>
    /// Specifies the domain of a tessellator.
    /// </summary>
    [Flags]
    public enum TessellatorDomain
    {
        /// <summary>
        /// Undefined domain.
        /// </summary>
        Undefined = 0x0,

        /// <summary>
        /// Isoline tessellation domain.
        /// </summary>
        Isoline = 0x1,

        /// <summary>
        /// Triangle tessellation domain.
        /// </summary>
        Tri = 0x2,

        /// <summary>
        /// Quad tessellation domain.
        /// </summary>
        Quad = 0x3,
    }
}