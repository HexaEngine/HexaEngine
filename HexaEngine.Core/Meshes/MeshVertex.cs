namespace HexaEngine.Core.Meshes
{
    using System;
    using System.Numerics;

    public struct MeshVertex
    {
        public Vector3 Position;
        public Vector3 UV;
        public Vector3 Normal;
        public Vector3 Tangent;

        public MeshVertex(Vector3 position, Vector2 texture, Vector3 normal, Vector3 tangent)
        {
            Position = position;
            UV = new(texture, 0);
            Normal = normal;
            Tangent = tangent;
        }

        public MeshVertex(Vector3 position, Vector3 texture, Vector3 normal, Vector3 tangent)
        {
            Position = position;
            UV = texture;
            Normal = normal;
            Tangent = tangent;
        }

        public MeshVertex InvertTex()
        {
            return new MeshVertex(Position, new Vector3(Math.Abs(UV.X - 1), Math.Abs(UV.Y - 1), UV.Z), Normal, Tangent);
        }

        public static bool operator ==(MeshVertex a, MeshVertex b)
        {
            return a.Position == b.Position && a.UV == b.UV && a.Normal == b.Normal && a.Tangent == b.Tangent;
        }

        public static bool operator !=(MeshVertex a, MeshVertex b)
        {
            return !(a == b);
        }

        public override bool Equals(object? obj)
        {
            if (obj is MeshVertex vertex)
            {
                return this == vertex;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode() ^ UV.GetHashCode() ^ Normal.GetHashCode() ^ Tangent.GetHashCode();
        }
    }
}