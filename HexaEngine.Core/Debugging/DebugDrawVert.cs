#nullable disable

namespace HexaEngine.Core.Debugging
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Represents a vertex used in debug drawing, containing position, texture coordinates, and color information.
    /// </summary>
    public struct DebugDrawVert : IEquatable<DebugDrawVert>
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

        public override readonly bool Equals(object obj)
        {
            return obj is DebugDrawVert vert && Equals(vert);
        }

        public readonly bool Equals(DebugDrawVert other)
        {
            return Position.Equals(other.Position) &&
                   UV.Equals(other.UV) &&
                   Color == other.Color;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Position, UV, Color);
        }

        public static bool operator ==(DebugDrawVert left, DebugDrawVert right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DebugDrawVert left, DebugDrawVert right)
        {
            return !(left == right);
        }
    }
}