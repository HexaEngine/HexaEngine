namespace HexaEngine.Core.Meshes
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Represents a vertex in a mesh.
    /// </summary>
    public struct MeshVertex : IEquatable<MeshVertex>
    {
        /// <summary>
        /// The position of the vertex.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The texture coordinates of the vertex.
        /// </summary>
        public Vector3 UV;

        /// <summary>
        /// The normal vector of the vertex.
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// The tangent vector of the vertex.
        /// </summary>
        public Vector3 Tangent;

        /// <summary>
        /// The bitangent vector of the vertex.
        /// </summary>
        public Vector3 Bitangent;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeshVertex"/> struct.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="texture">The texture coordinates of the vertex.</param>
        /// <param name="normal">The normal vector of the vertex.</param>
        /// <param name="tangent">The tangent vector of the vertex.</param>
        /// <param name="bitangent">The bitangent vector of the vertex.</param>
        public MeshVertex(Vector3 position, Vector2 texture, Vector3 normal, Vector3 tangent, Vector3 bitangent)
        {
            Position = position;
            UV = new Vector3(texture, 0);
            Normal = normal;
            Tangent = tangent;
            Bitangent = bitangent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeshVertex"/> struct.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="texture">The texture coordinates of the vertex.</param>
        /// <param name="normal">The normal vector of the vertex.</param>
        /// <param name="tangent">The tangent vector of the vertex.</param>
        /// <param name="bitangent">The bitangent vector of the vertex.</param>
        public MeshVertex(Vector3 position, Vector3 texture, Vector3 normal, Vector3 tangent, Vector3 bitangent)
        {
            Position = position;
            UV = texture;
            Normal = normal;
            Tangent = tangent;
            Bitangent = bitangent;
        }

        /// <summary>
        /// Creates a new <see cref="MeshVertex"/> with inverted texture coordinates.
        /// </summary>
        /// <returns>The new <see cref="MeshVertex"/> with inverted texture coordinates.</returns>
        public readonly MeshVertex InvertTex()
        {
            return new MeshVertex(Position, new Vector3(Math.Abs(UV.X - 1), Math.Abs(UV.Y - 1), UV.Z), Normal, Tangent, Bitangent);
        }

        /// <inheritdoc/>
        public static bool operator ==(MeshVertex a, MeshVertex b)
        {
            return a.Position == b.Position && a.UV == b.UV && a.Normal == b.Normal && a.Tangent == b.Tangent && a.Bitangent == b.Bitangent;
        }

        /// <inheritdoc/>
        public static bool operator !=(MeshVertex a, MeshVertex b)
        {
            return !(a == b);
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            if (obj is MeshVertex vertex)
            {
                return this == vertex;
            }

            return false;
        }

        /// <inheritdoc/>
        public readonly bool Equals(MeshVertex other)
        {
            return Position.Equals(other.Position) &&
                   UV.Equals(other.UV) &&
                   Normal.Equals(other.Normal) &&
                   Tangent.Equals(other.Tangent) &&
                   Bitangent.Equals(other.Bitangent);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Position, UV, Normal, Tangent, Bitangent);
        }

        /// <inheritdoc/>
        public override readonly string? ToString()
        {
            return $"<Pos: {Position},UV: {UV},N: {Normal},T: {Tangent},B: {Bitangent}>";
        }
    }
}