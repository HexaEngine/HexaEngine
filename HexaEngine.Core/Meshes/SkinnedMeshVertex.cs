namespace HexaEngine.Core.Meshes
{
    using System;
    using System.Numerics;

    public unsafe struct SkinnedMeshVertex : IEquatable<SkinnedMeshVertex>
    {
        public Vector3 Position;
        public Vector3 Texture;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 Bitangent;

        public fixed int BoneIds[4];
        public fixed float Weights[4];

        public SkinnedMeshVertex(Vector3 position, Vector3 texture, Vector3 normal, Vector3 tangent)
        {
            Position = position;
            Texture = texture;
            Normal = normal;
            Tangent = tangent;
        }

        public SkinnedMeshVertex(MeshVertex vertex)
        {
            Position = vertex.Position;
            Texture = vertex.UV;
            Normal = vertex.Normal;
            Tangent = vertex.Tangent;
        }

        public readonly SkinnedMeshVertex InvertTex()
        {
            return new SkinnedMeshVertex(Position, new(Math.Abs(Texture.X - 1), Math.Abs(Texture.Y - 1), 0), Normal, Tangent);
        }

        public override bool Equals(object? obj)
        {
            return obj is SkinnedMeshVertex vertex && Equals(vertex);
        }

        public bool Equals(SkinnedMeshVertex other)
        {
            return Position.Equals(other.Position) &&
                   Texture.Equals(other.Texture) &&
                   Normal.Equals(other.Normal) &&
                   Tangent.Equals(other.Tangent) &&
                   Bitangent.Equals(other.Bitangent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string? ToString()
        {
            return base.ToString();
        }

        public static bool operator ==(SkinnedMeshVertex left, SkinnedMeshVertex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SkinnedMeshVertex left, SkinnedMeshVertex right)
        {
            return !(left == right);
        }
    }
}