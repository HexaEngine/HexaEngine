﻿namespace HexaEngine.Core.IO.Binary.Materials
{
    /// <summary>
    /// Represents flags that define various properties of a material.
    /// </summary>
    [Flags]
    public enum MaterialFlags
    {
        /// <summary>
        /// No special flags set.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that the material should do depth-testing.
        /// </summary>
        DepthTest = 1,

        /// <summary>
        /// Indicates that the material supports tessellation.
        /// </summary>
        Tessellation = 2,

        /// <summary>
        /// Indicates that the material has geometry properties.
        /// </summary>
        Geometry = 4,

        /// <summary>
        /// Indicates that the material has custom properties.
        /// </summary>
        Custom = 8,

        /// <summary>
        /// Indicates that the material is transparent.
        /// </summary>
        Transparent = 16,

        /// <summary>
        /// Indicates that the material is associated with a node.
        /// </summary>
        Node = 32,

        /// <summary>
        /// Indicates that the material supports alpha test.
        /// </summary>
        AlphaTest = 64,

        /// <summary>
        /// Indicates that the material should do depth-testing with comparison always.
        /// </summary>
        DepthAlways = 128
    }
}