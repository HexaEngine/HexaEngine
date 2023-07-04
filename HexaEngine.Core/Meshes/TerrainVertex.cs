namespace HexaEngine.Core.Meshes
{
    using System;
    using System.Numerics;

    public struct TerrainVertex : IEquatable<TerrainVertex>
    {
        public Vector3 Position;
        public Vector3 UV;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 Bitangent;

        public TerrainVertex(Vector3 position, Vector2 texture, Vector3 normal, Vector3 tangent, Vector3 bitangent)
        {
            Position = position;
            UV = new(texture, 0);
            Normal = normal;
            Tangent = tangent;
            Bitangent = bitangent;
        }

        public TerrainVertex(Vector3 position, Vector3 texture, Vector3 normal, Vector3 tangent, Vector3 bitangent)
        {
            Position = position;
            UV = texture;
            Normal = normal;
            Tangent = tangent;
            Bitangent = bitangent;
        }

        public readonly TerrainVertex InvertTex()
        {
            return new TerrainVertex(Position, new Vector3(Math.Abs(UV.X - 1), Math.Abs(UV.Y - 1), UV.Z), Normal, Tangent, Bitangent);
        }

        public static bool operator ==(TerrainVertex a, TerrainVertex b)
        {
            return a.Position == b.Position && a.UV == b.UV && a.Normal == b.Normal && a.Tangent == b.Tangent && a.Bitangent == b.Bitangent;
        }

        public static bool operator !=(TerrainVertex a, TerrainVertex b)
        {
            return !(a == b);
        }

        public override readonly bool Equals(object? obj)
        {
            if (obj is TerrainVertex vertex)
            {
                return this == vertex;
            }

            return false;
        }

        public readonly bool Equals(TerrainVertex other)
        {
            return Position.Equals(other.Position) &&
                   UV.Equals(other.UV) &&
                   Normal.Equals(other.Normal) &&
                   Tangent.Equals(other.Tangent) &&
                   Bitangent.Equals(other.Bitangent);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Position, UV, Normal, Tangent, Bitangent);
        }
    }
}