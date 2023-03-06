namespace HexaEngine.Core.Meshes
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;

    public struct MeshVertex
    {
        public Vector3 Position;
        public Vector2 UV;
        public Vector3 Normal;
        public Vector3 Tangent;

        public MeshVertex(Vector3 position, Vector2 texture, Vector3 normal, Vector3 tangent)
        {
            Position = position;
            UV = texture;
            Normal = normal;
            Tangent = tangent;
        }

        public MeshVertex(Vector3 position, Vector3 texture, Vector3 normal, Vector3 tangent)
        {
            Position = position;
            UV = new(texture.X, texture.Y);
            Normal = normal;
            Tangent = tangent;
        }

        public MeshVertex(Vector3 position, Vector2 texture, Vector3 normal)
        {
            Position = position;
            UV = texture;
            Normal = normal;
            Tangent = Vector3.Zero;
        }

        public MeshVertex InvertTex()
        {
            return new MeshVertex(Position, new Vector2(Math.Abs(UV.X - 1), Math.Abs(UV.Y - 1)), Normal, Tangent);
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
                return this == vertex;
            return false;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode() ^ UV.GetHashCode() ^ Normal.GetHashCode() ^ Tangent.GetHashCode();
        }

        public void Read(Stream stream, Endianness endianness)
        {
            Position = stream.ReadVector3(endianness);
            UV = stream.ReadVector2(endianness);
            Normal = stream.ReadVector3(endianness);
            Tangent = stream.ReadVector3(endianness);
        }

        public void Write(Stream stream, Endianness endianness)
        {
            stream.WriteVector3(Position, endianness);
            stream.WriteVector2(UV, endianness);
            stream.WriteVector3(Normal, endianness);
            stream.WriteVector3(Tangent, endianness);
        }
    }
}