﻿namespace HexaEngine.Core.IO
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Represents a vertex used in terrain meshes.
    /// </summary>
    public struct TerrainVertex : IEquatable<TerrainVertex>
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
        /// Initializes a new instance of the <see cref="TerrainVertex"/> struct with the specified position, texture coordinates, normal, tangent, and bitangent.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="texture">The texture coordinates of the vertex.</param>
        /// <param name="normal">The normal vector of the vertex.</param>
        /// <param name="tangent">The tangent vector of the vertex.</param>
        public TerrainVertex(Vector3 position, Vector2 texture, Vector3 normal, Vector3 tangent)
        {
            Position = position;
            UV = texture;
            Normal = normal;
            Tangent = tangent;
        }

        /// <summary>
        /// Creates a new <see cref="TerrainVertex"/> with inverted texture coordinates.
        /// </summary>
        /// <returns>A new <see cref="TerrainVertex"/> with inverted texture coordinates.</returns>
        public readonly TerrainVertex InvertTex()
        {
            return new TerrainVertex(Position, new Vector2(Math.Abs(UV.X - 1), Math.Abs(UV.Y - 1)), Normal, Tangent);
        }

        /// <summary>
        /// Determines whether the specified <see cref="TerrainVertex"/> is equal to the current <see cref="TerrainVertex"/>.
        /// </summary>
        /// <param name="other">The <see cref="TerrainVertex"/> to compare with the current <see cref="TerrainVertex"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="TerrainVertex"/> is equal to the current <see cref="TerrainVertex"/>; otherwise, <c>false</c>.</returns>
        public readonly bool Equals(TerrainVertex other)
        {
            return Position.Equals(other.Position) &&
                   UV.Equals(other.UV) &&
                   Normal.Equals(other.Normal) &&
                   Tangent.Equals(other.Tangent);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="TerrainVertex"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current <see cref="TerrainVertex"/>.</param>
        /// <returns><c>true</c> if the specified object is equal to the current <see cref="TerrainVertex"/>; otherwise, <c>false</c>.</returns>
        public override readonly bool Equals(object? obj)
        {
            if (obj is TerrainVertex vertex)
            {
                return this == vertex;
            }

            return false;
        }

        /// <summary>
        /// Returns the hash code for the current <see cref="TerrainVertex"/>.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Position, UV, Normal, Tangent);
        }

        /// <inheritdoc/>
        public override readonly string? ToString()
        {
            return $"<Pos: {Position},UV: {UV},N: {Normal},T: {Tangent}>";
        }

        /// <summary>
        /// Determines whether two <see cref="TerrainVertex"/> instances are equal.
        /// </summary>
        /// <param name="a">The first <see cref="TerrainVertex"/> to compare.</param>
        /// <param name="b">The second <see cref="TerrainVertex"/> to compare.</param>
        /// <returns><c>true</c> if the two instances are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(TerrainVertex a, TerrainVertex b)
        {
            return a.Position == b.Position && a.UV == b.UV && a.Normal == b.Normal && a.Tangent == b.Tangent;
        }

        /// <summary>
        /// Determines whether two <see cref="TerrainVertex"/> instances are not equal.
        /// </summary>
        /// <param name="a">The first <see cref="TerrainVertex"/> to compare.</param>
        /// <param name="b">The second <see cref="TerrainVertex"/> to compare.</param>
        /// <returns><c>true</c> if the two instances are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(TerrainVertex a, TerrainVertex b)
        {
            return !(a == b);
        }
    }
}