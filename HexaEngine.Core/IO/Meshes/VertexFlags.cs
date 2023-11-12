namespace HexaEngine.Core.IO.Meshes
{
    /// <summary>
    /// Represents flags that define additional information about a vertex.
    /// </summary>
    [Flags]
    public enum VertexFlags
    {
        /// <summary>
        /// No additional information specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates the presence of vertex colors.
        /// </summary>
        Colors = 2,

        /// <summary>
        /// Indicates the presence of vertex positions.
        /// </summary>
        Positions = 4,

        /// <summary>
        /// Indicates the presence of UV coordinates.
        /// </summary>
        UVs = 8,

        /// <summary>
        /// Indicates the presence of vertex normals.
        /// </summary>
        Normals = 16,

        /// <summary>
        /// Indicates the presence of vertex tangents.
        /// </summary>
        Tangents = 32,

        /// <summary>
        /// Indicates the presence of vertex bitangents.
        /// </summary>
        Bitangents = 64,

        /// <summary>
        /// Indicates that the vertex is part of a skinned mesh.
        /// </summary>
        Skinned = 128,
    }
}