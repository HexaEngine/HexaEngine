namespace HexaEngine.Core.Graphics.Primitives
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Represents a vertex in a mesh.
    /// </summary>
    public struct PrimVertex : IEquatable<PrimVertex>
    {
        /// <summary>
        /// The position of the vertex.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The texture coordinates of the vertex.
        /// </summary>
        public Vector2 UV;

        /// <summary>
        /// The normal vector of the vertex.
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// The tangent vector of the vertex.
        /// </summary>
        public Vector3 Tangent;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimVertex"/> struct.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="uv">The texture coordinates of the vertex.</param>
        /// <param name="normal">The normal vector of the vertex.</param>
        /// <param name="tangent">The tangent vector of the vertex.</param>
        public PrimVertex(Vector3 position, Vector2 uv, Vector3 normal, Vector3 tangent)
        {
            Position = position;
            UV = uv;
            Normal = normal;
            Tangent = tangent;
        }

        /// <summary>
        /// Creates a new <see cref="PrimVertex"/> with inverted texture coordinates.
        /// </summary>
        /// <returns>The new <see cref="PrimVertex"/> with inverted texture coordinates.</returns>
        public readonly PrimVertex InvertTex()
        {
            return new PrimVertex(Position, new Vector2(Math.Abs(UV.X - 1), Math.Abs(UV.Y - 1)), Normal, Tangent);
        }

        /// <inheritdoc/>
        public static bool operator ==(PrimVertex a, PrimVertex b)
        {
            return a.Position == b.Position && a.UV == b.UV && a.Normal == b.Normal && a.Tangent == b.Tangent;
        }

        /// <inheritdoc/>
        public static bool operator !=(PrimVertex a, PrimVertex b)
        {
            return !(a == b);
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            if (obj is PrimVertex vertex)
            {
                return this == vertex;
            }

            return false;
        }

        /// <inheritdoc/>
        public readonly bool Equals(PrimVertex other)
        {
            return Position.Equals(other.Position) &&
                   UV.Equals(other.UV) &&
                   Normal.Equals(other.Normal) &&
                   Tangent.Equals(other.Tangent);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Position, UV, Normal, Tangent);
        }

        /// <inheritdoc/>
        public override readonly string? ToString()
        {
            return $"<Pos: {Position},UV: {UV},N: {Normal},T: {Tangent}>";
        }
    }
}