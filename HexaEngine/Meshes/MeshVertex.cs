﻿namespace HexaEngine.Meshes
{
    using HexaEngine.Scenes;
    using System;
    using System.Numerics;

    public struct MeshBone
    {
        public string Name;
        public MeshWeight[] Weights;
        public Matrix4x4 Offset;
        public GameObject Node;

        public MeshBone(string name, MeshWeight[] weights, Matrix4x4 offset)
        {
            Name = name;
            Weights = weights;
            Offset = offset;
        }
    }

    public struct MeshWeight
    {
        public uint VertexId;
        public float Weight;
    }

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

    public struct SkinnedMeshVertex
    {
        public Vector3 Position;
        public Vector2 Texture;
        public Vector3 Normal;
        public Vector3 Tangent;

        public int BoneId1;
        public int BoneId2;
        public int BoneId3;
        public int BoneId4;

        public float Weight1;
        public float Weight2;
        public float Weight3;
        public float Weight4;

        public SkinnedMeshVertex(Vector3 position, Vector2 texture, Vector3 normal, Vector3 tangent, int boneId1, int boneId2, int boneId3, int boneId4, float weight1, float weight2, float weight3, float weight4)
        {
            Position = position;
            Texture = texture;
            Normal = normal;
            Tangent = tangent;
            BoneId1 = boneId1;
            BoneId2 = boneId2;
            BoneId3 = boneId3;
            BoneId4 = boneId4;
            Weight1 = weight1;
            Weight2 = weight2;
            Weight3 = weight3;
            Weight4 = weight4;
        }

        public SkinnedMeshVertex InvertTex()
        {
            return new SkinnedMeshVertex(Position, new(Math.Abs(Texture.X - 1), Math.Abs(Texture.Y - 1)), Normal, Tangent, BoneId1, BoneId2, BoneId3, BoneId4, Weight1, Weight2, Weight3, Weight4);
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

    public struct TerrainVertex
    {
        public Vector3 Position;
        public Vector2 Texture;
        public Vector2 CTexture;
    }

    public struct TerrainVertexStatic
    {
        public Vector3 Position;
        public Vector2 Texture;
        public Vector2 CTexture;
        public Vector3 Normal;
        public Vector3 Tangent;
    }
}