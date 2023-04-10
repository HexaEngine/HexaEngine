namespace HexaEngine.Core.Meshes
{
    using System;
    using System.Numerics;

    public unsafe struct SkinnedMeshVertex
    {
        public Vector3 Position;
        public Vector3 Texture;
        public Vector3 Normal;
        public Vector3 Tangent;

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

        public SkinnedMeshVertex InvertTex()
        {
            return new SkinnedMeshVertex(Position, new(Math.Abs(Texture.X - 1), Math.Abs(Texture.Y - 1), 0), Normal, Tangent);
        }

        public static unsafe bool operator ==(SkinnedMeshVertex a, SkinnedMeshVertex b)
        {
            byte* ap = (byte*)&a;
            byte* bp = (byte*)&b;
            for (int i = 0; i < sizeof(SkinnedMeshVertex); i++)
            {
                if (ap[i] != bp[i])
                    return false;
            }
            return true;
        }

        public static bool operator !=(SkinnedMeshVertex a, SkinnedMeshVertex b)
        {
            return !(a == b);
        }

        public override bool Equals(object? obj)
        {
            if (obj is SkinnedMeshVertex vertex)
                return this == vertex;
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this);
        }
    }
}