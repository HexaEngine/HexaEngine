namespace HexaEngine.Core.IO.Binary.Meshes
{
    using System;

    /// <summary>
    /// Represents a face (triangle) in a mesh defined by three vertex indices.
    /// </summary>
    public struct Face : IEquatable<Face>
    {
        /// <summary>
        /// The index of the first vertex in the face.
        /// </summary>
        public uint Index1;

        /// <summary>
        /// The index of the second vertex in the face.
        /// </summary>
        public uint Index2;

        /// <summary>
        /// The index of the third vertex in the face.
        /// </summary>
        public uint Index3;

        /// <summary>
        /// Initializes a new instance of the <see cref="Face"/> struct with the specified vertex indices.
        /// </summary>
        /// <param name="index1">The index of the first vertex.</param>
        /// <param name="index2">The index of the second vertex.</param>
        /// <param name="index3">The index of the third vertex.</param>
        public Face(uint index1, uint index2, uint index3)
        {
            Index1 = index1;
            Index2 = index2;
            Index3 = index3;
        }

        /// <summary>
        /// Gets or sets the vertex index at the specified position.
        /// </summary>
        /// <param name="index">The position of the vertex index (0, 1, or 2).</param>
        /// <returns>The vertex index at the specified position.</returns>
        public unsafe uint this[int index]
        {
            get
            {
                fixed (Face* p = &this)
                {
                    return ((uint*)p)[index];
                }
            }
            set
            {
                fixed (Face* p = &this)
                {
                    ((uint*)p)[index] = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the vertex index at the specified position.
        /// </summary>
        /// <param name="index">The position of the vertex index (0, 1, or 2).</param>
        /// <returns>The vertex index at the specified position.</returns>
        public unsafe uint this[uint index]
        {
            get
            {
                fixed (Face* p = &this)
                {
                    return ((uint*)p)[index];
                }
            }
            set
            {
                fixed (Face* p = &this)
                {
                    ((uint*)p)[index] = value;
                }
            }
        }

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj)
        {
            return obj is Face face && Equals(face);
        }

        /// <inheritdoc/>
        public readonly bool Equals(Face other)
        {
            return Index1 == other.Index1 &&
                   Index2 == other.Index2 &&
                   Index3 == other.Index3;
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Index1, Index2, Index3);
        }

        /// <summary>
        /// Determines whether this face shares any vertex indices with the specified face.
        /// </summary>
        /// <param name="other">The face to compare.</param>
        /// <returns><c>true</c> if the faces share any vertex indices; otherwise, <c>false</c>.</returns>
        public readonly bool Shares(Face other)
        {
            return Index1 == other.Index1 || Index2 == other.Index2 || Index3 == other.Index3;
        }

        /// <summary>
        /// Determines whether this face shares a vertex index with the specified index.
        /// </summary>
        /// <param name="index">The vertex index to compare.</param>
        /// <returns><c>true</c> if the face shares the specified vertex index; otherwise, <c>false</c>.</returns>
        public readonly bool Shares(uint index)
        {
            return Index1 == index || Index2 == index || Index3 == index;
        }

        /// <summary>
        /// Determines whether two faces are equal.
        /// </summary>
        public static bool operator ==(Face left, Face right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two faces are not equal.
        /// </summary>
        public static bool operator !=(Face left, Face right)
        {
            return !(left == right);
        }
    }
}