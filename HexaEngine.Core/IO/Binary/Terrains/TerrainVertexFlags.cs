namespace HexaEngine.Core.IO.Binary.Terrains
{
    using System;

    /// <summary>
    /// Specifies flags that represent various vertex attributes in a terrain mesh.
    /// </summary>
    [Flags]
    public enum TerrainVertexFlags
    {
        /// <summary>
        /// No specific vertex attributes.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates the presence of position information in the vertex data.
        /// </summary>
        Positions = 2,

        /// <summary>
        /// Indicates the presence of UV coordinates in the vertex data.
        /// </summary>
        UVs = 4,

        /// <summary>
        /// Indicates the presence of normal vectors in the vertex data.
        /// </summary>
        Normals = 8,

        /// <summary>
        /// Indicates the presence of tangent vectors in the vertex data.
        /// </summary>
        Tangents = 16,

        /// <summary>
        /// Indicates the presence of bitangent vectors in the vertex data.
        /// </summary>
        Bitangents = 32,
    }

}