#nullable disable

namespace HexaEngine.Editor
{
    using System.Numerics;

    /// <summary>
    /// Represents a vertex used in debug drawing, containing position, texture coordinates, and color information.
    /// </summary>
    public struct DebugDrawVert
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DebugDrawVert"/> struct with the specified position, texture coordinates, and color.
        /// </summary>
        /// <param name="position">The position of the vertex in 3D space.</param>
        /// <param name="uv">The texture coordinates of the vertex.</param>
        /// <param name="color">The color information of the vertex.</param>
        public DebugDrawVert(Vector3 position, Vector2 uv, uint color)
        {
            Position = position;
            UV = uv;
            Color = color;
        }

        /// <summary>
        /// Gets or sets the position of the vertex in 3D space.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Gets or sets the texture coordinates of the vertex.
        /// </summary>
        public Vector2 UV;

        /// <summary>
        /// Gets or sets the color information of the vertex.
        /// </summary>
        public uint Color;
    }
}