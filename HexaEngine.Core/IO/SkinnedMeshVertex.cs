namespace HexaEngine.Core.IO
{
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;

    /// <summary>
    /// Represents a vertex in a skinned mesh.
    /// </summary>
    public unsafe struct SkinnedMeshVertex : IEquatable<SkinnedMeshVertex>
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
        /// The array of bone IDs influencing the vertex.
        /// </summary>
        public Point4 BoneIds;

        /// <summary>
        /// The array of bone weights influencing the vertex.
        /// </summary>
        public Vector4 Weights;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkinnedMeshVertex"/> struct.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="texture">The texture coordinates of the vertex.</param>
        /// <param name="normal">The normal vector of the vertex.</param>
        /// <param name="tangent">The tangent vector of the vertex.</param>
        public SkinnedMeshVertex(Vector3 position, Vector3 texture, Vector3 normal, Vector3 tangent)
        {
            Position = position;
            UV = texture;
            Normal = normal;
            Tangent = tangent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SkinnedMeshVertex"/> struct using a <see cref="MeshVertex"/>.
        /// </summary>
        /// <param name="vertex">The <see cref="MeshVertex"/> to convert.</param>
        public SkinnedMeshVertex(MeshVertex vertex)
        {
            Position = vertex.Position;
            UV = vertex.UV;
            Normal = vertex.Normal;
            Tangent = vertex.Tangent;
        }

        /// <summary>
        /// Creates a new <see cref="SkinnedMeshVertex"/> with inverted texture coordinates.
        /// </summary>
        /// <returns>The new <see cref="SkinnedMeshVertex"/> with inverted texture coordinates.</returns>
        public readonly SkinnedMeshVertex InvertTex()
        {
            return new SkinnedMeshVertex(Position, new Vector3(Math.Abs(UV.X - 1), Math.Abs(UV.Y - 1), 0), Normal, Tangent);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is SkinnedMeshVertex vertex && Equals(vertex);
        }

        /// <inheritdoc/>
        public readonly bool Equals(SkinnedMeshVertex other)
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
            return $"<Pos: {Position},UV: {UV},N: {Normal},T: {Tangent}, BoneIds: {BoneIds}, Weights: {Weights}>";
        }

        /// <summary>
        /// Compares two <see cref="SkinnedMeshVertex"/> objects for equality.
        /// </summary>
        /// <param name="left">The first <see cref="SkinnedMeshVertex"/> to compare.</param>
        /// <param name="right">The second <see cref="SkinnedMeshVertex"/> to compare.</param>
        /// <returns>True if the two vertices are equal, false otherwise.</returns>
        public static bool operator ==(SkinnedMeshVertex left, SkinnedMeshVertex right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two <see cref="SkinnedMeshVertex"/> objects for inequality.
        /// </summary>
        /// <param name="left">The first <see cref="SkinnedMeshVertex"/> to compare.</param>
        /// <param name="right">The second <see cref="SkinnedMeshVertex"/> to compare.</param>
        /// <returns>True if the two vertices are not equal, false otherwise.</returns>
        public static bool operator !=(SkinnedMeshVertex left, SkinnedMeshVertex right)
        {
            return !(left == right);
        }
    }
}