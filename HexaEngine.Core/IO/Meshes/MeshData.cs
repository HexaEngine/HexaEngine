namespace HexaEngine.Core.IO.Meshes
{
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Numerics;
    using System.Text;

    public unsafe struct MeshData
    {
        public string Name;
        public MaterialData Material;
        public uint VerticesCount;
        public uint IndicesCount;
        public uint WeightsCount;
        public BoundingBox Box;
        public BoundingSphere Sphere;
        public MeshFlags Flags;
        public uint[] Indices;
        public Vector4[] Colors;
        public Vector3[] Positions;
        public Vector3[] UVs;
        public Vector3[] Normals;
        public Vector3[] Tangents;
        public MeshWeight[] Weights;

        public MeshData(string name, MaterialData material, BoundingBox box, BoundingSphere sphere, uint vertexCount, uint indexCount, uint weightCount, uint[]? indices, Vector4[]? colors, Vector3[]? positions, Vector3[]? uvs, Vector3[]? normals, Vector3[]? tangents, MeshWeight[]? weights)
        {
            Name = name;
            Material = material;
            Box = box;
            Sphere = sphere;

            VerticesCount = vertexCount;
            IndicesCount = indexCount;
            WeightsCount = weightCount;

            if (indices != null)
                Flags |= MeshFlags.Indexed;
            Indices = indices;
            if (colors != null)
                Flags |= MeshFlags.Colors;
            Colors = colors;
            if (positions != null)
                Flags |= MeshFlags.Positions;
            Positions = positions;
            if (uvs != null)
                Flags |= MeshFlags.UVs;
            UVs = uvs;
            if (normals != null)
                Flags |= MeshFlags.Normals;
            Normals = normals;
            if (tangents != null)
                Flags |= MeshFlags.Tangents;
            Tangents = tangents;
            if (weights != null)
                Flags |= MeshFlags.Weights;
            Weights = weights;
        }

        public static MeshData Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            MeshData data = default;
            data.Name = stream.ReadString(encoding, endianness);
            data.Material = MaterialData.Read(stream, encoding, endianness);
            data.VerticesCount = stream.ReadUInt(endianness);
            data.IndicesCount = stream.ReadUInt(endianness);
            data.WeightsCount = stream.ReadUInt(endianness);
            data.Box = BoundingBox.Read(stream, endianness);
            data.Sphere = BoundingSphere.Read(stream, endianness);
            data.Flags = (MeshFlags)stream.ReadInt(endianness);

            if ((data.Flags & MeshFlags.Indexed) != 0)
            {
                data.Indices = new uint[data.IndicesCount];
                for (ulong i = 0; i < data.IndicesCount; i++)
                {
                    data.Indices[i] = stream.ReadUInt(endianness);
                }
            }

            if ((data.Flags & MeshFlags.Colors) != 0)
            {
                data.Colors = new Vector4[data.VerticesCount];
                for (ulong i = 0; i < data.VerticesCount; i++)
                {
                    data.Colors[i] = stream.ReadVector4(endianness);
                }
            }
            if ((data.Flags & MeshFlags.Positions) != 0)
            {
                data.Positions = new Vector3[data.VerticesCount];
                for (ulong i = 0; i < data.VerticesCount; i++)
                {
                    data.Positions[i] = stream.ReadVector3(endianness);
                }
            }
            if ((data.Flags & MeshFlags.UVs) != 0)
            {
                data.UVs = new Vector3[data.VerticesCount];
                for (ulong i = 0; i < data.VerticesCount; i++)
                {
                    data.UVs[i] = stream.ReadVector3(endianness);
                }
            }
            if ((data.Flags & MeshFlags.Normals) != 0)
            {
                data.Normals = new Vector3[data.VerticesCount];
                for (ulong i = 0; i < data.VerticesCount; i++)
                {
                    data.Normals[i] = stream.ReadVector3(endianness);
                }
            }
            if ((data.Flags & MeshFlags.Tangents) != 0)
            {
                data.Tangents = new Vector3[data.VerticesCount];
                for (ulong i = 0; i < data.VerticesCount; i++)
                {
                    data.Tangents[i] = stream.ReadVector3(endianness);
                }
            }
            if ((data.Flags & MeshFlags.Weights) != 0)
            {
                data.Weights = new MeshWeight[data.WeightsCount];
                for (ulong i = 0; i < data.WeightsCount; i++)
                {
                    data.Weights[i].Read(stream, endianness);
                }
            }

            return data;
        }

        public void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteString(Name, encoding, endianness);
            Material.Write(stream, encoding, endianness);
            stream.WriteUInt(VerticesCount, endianness);
            stream.WriteUInt(IndicesCount, endianness);
            stream.WriteUInt(WeightsCount, endianness);
            Box.Write(stream, endianness);
            Sphere.Write(stream, endianness);
            stream.WriteInt((int)Flags, endianness);
            if ((Flags & MeshFlags.Indexed) != 0)
            {
                for (ulong i = 0; i < IndicesCount; i++)
                {
                    stream.WriteUInt(Indices[i], endianness);
                }
            }
            if ((Flags & MeshFlags.Colors) != 0)
            {
                for (ulong i = 0; i < VerticesCount; i++)
                {
                    stream.WriteVector4(Colors[i], endianness);
                }
            }
            if ((Flags & MeshFlags.Positions) != 0)
            {
                for (ulong i = 0; i < VerticesCount; i++)
                {
                    stream.WriteVector3(Positions[i], endianness);
                }
            }
            if ((Flags & MeshFlags.UVs) != 0)
            {
                for (ulong i = 0; i < VerticesCount; i++)
                {
                    stream.WriteVector3(UVs[i], endianness);
                }
            }
            if ((Flags & MeshFlags.Normals) != 0)
            {
                for (ulong i = 0; i < VerticesCount; i++)
                {
                    stream.WriteVector3(Normals[i], endianness);
                }
            }
            if ((Flags & MeshFlags.Tangents) != 0)
            {
                for (ulong i = 0; i < VerticesCount; i++)
                {
                    stream.WriteVector3(Tangents[i], endianness);
                }
            }
            if ((Flags & MeshFlags.Weights) != 0)
            {
                for (ulong i = 0; i < WeightsCount; i++)
                {
                    Weights[i].Write(stream, endianness);
                }
            }
        }

        public MeshVertex[] GetVertices()
        {
            if ((Flags & MeshFlags.Positions) == 0)
                throw new NotSupportedException();
            if ((Flags & MeshFlags.UVs) == 0)
                throw new NotSupportedException();
            if ((Flags & MeshFlags.Normals) == 0)
                throw new NotSupportedException();
            if ((Flags & MeshFlags.Tangents) == 0)
                throw new NotSupportedException();
            MeshVertex[] vertices = new MeshVertex[VerticesCount];
            for (uint i = 0; i < VerticesCount; i++)
            {
                vertices[i] = new(Positions[i], UVs[i], Normals[i], Tangents[i]);
            }
            return vertices;
        }
    }
}