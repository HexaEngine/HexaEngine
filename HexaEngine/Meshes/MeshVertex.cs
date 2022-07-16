namespace HexaEngine.Meshes
{
    using System;
    using System.Numerics;

    public struct MeshVertex
    {
        public Vector3 Position;
        public Vector2 Texture;
        public Vector3 Normal;
        public Vector3 Tangent;

        public MeshVertex(Vector3 position, Vector2 texture, Vector3 normal, Vector3 tangent)
        {
            Position = position;
            Texture = texture;
            Normal = normal;
            Tangent = tangent;
        }

        public MeshVertex(Vector3 position, Vector2 texture, Vector3 normal)
        {
            Position = position;
            Texture = texture;
            Normal = normal;
            Tangent = Vector3.Zero;
        }

        public MeshVertex InvertTex()
        {
            return new MeshVertex(Position, new(Math.Abs(Texture.X - 1), Math.Abs(Texture.Y - 1)), Normal, Tangent);
        }

        public static bool operator ==(MeshVertex a, MeshVertex b)
        {
            return a.Position == b.Position && a.Texture == b.Texture && a.Normal == b.Normal && a.Tangent == b.Tangent;
        }

        public static bool operator !=(MeshVertex a, MeshVertex b)
        {
            return !(a == b);
        }

        public override bool Equals(object? obj)
        {
            if (obj is MeshVertex vertex)
                return this == vertex;
            return false;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode() ^ Texture.GetHashCode() ^ Normal.GetHashCode() ^ Tangent.GetHashCode();
        }
    }
}