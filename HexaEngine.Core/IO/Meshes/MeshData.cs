namespace HexaEngine.Core.IO.Meshes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO.Meshes.Processing;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Mathematics;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Numerics;
    using System.Text;

    public unsafe class MeshData
    {
        public string Name;
        public string MaterialName;
        public uint VerticesCount;
        public uint IndicesCount;
        public uint BoneCount;
        public BoundingBox Box;
        public BoundingSphere Sphere;
        public VertexFlags Flags;
        public uint[] Indices;
        public Vector4[] Colors;
        public Vector3[] Positions;
        public Vector3[] UVs;
        public Vector3[] Normals;
        public Vector3[] Tangents;
        public Vector3[] Bitangents;
        public BoneData[] Bones;

        private MeshData()
        {
        }

        public MeshData(string name, string materialName, BoundingBox box, BoundingSphere sphere, uint vertexCount, uint indexCount, uint weightCount, uint[] indices, Vector4[]? colors, Vector3[]? positions, Vector3[]? uvs, Vector3[]? normals, Vector3[]? tangents, Vector3[]? bitangents, BoneData[]? bones)
        {
            Name = name;
            MaterialName = materialName;
            Box = box;
            Sphere = sphere;

            VerticesCount = vertexCount;
            IndicesCount = indexCount;
            BoneCount = weightCount;

            Indices = indices;

            Colors = colors;
            if (colors != null)
            {
                Flags |= VertexFlags.Colors;
            }

            Positions = positions;
            if (positions != null)
            {
                Flags |= VertexFlags.Positions;
            }

            UVs = uvs;
            if (uvs != null)
            {
                Flags |= VertexFlags.UVs;
            }

            Normals = normals;
            if (normals != null)
            {
                Flags |= VertexFlags.Normals;
            }

            Tangents = tangents;
            if (tangents != null)
            {
                Flags |= VertexFlags.Tangents;
            }

            Bitangents = bitangents;
            if (bitangents != null)
            {
                Flags |= VertexFlags.Bitangents;
            }

            Bones = bones;
            if (bones != null)
            {
                Flags |= VertexFlags.Skinned;
            }
        }

        public static MeshData Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            MeshData data = new();
            data.Name = stream.ReadString(encoding, endianness);
            data.MaterialName = stream.ReadString(encoding, endianness);
            data.VerticesCount = stream.ReadUInt32(endianness);
            data.IndicesCount = stream.ReadUInt32(endianness);
            data.BoneCount = stream.ReadUInt32(endianness);
            data.Box = BoundingBox.Read(stream, endianness);
            data.Sphere = BoundingSphere.Read(stream, endianness);
            data.Flags = (VertexFlags)stream.ReadInt32(endianness);

            data.Indices = new uint[data.IndicesCount];
            for (ulong i = 0; i < data.IndicesCount; i++)
            {
                data.Indices[i] = stream.ReadUInt32(endianness);
            }

            if ((data.Flags & VertexFlags.Colors) != 0)
            {
                data.Colors = new Vector4[data.VerticesCount];
                for (ulong i = 0; i < data.VerticesCount; i++)
                {
                    data.Colors[i] = stream.ReadVector4(endianness);
                }
            }
            if ((data.Flags & VertexFlags.Positions) != 0)
            {
                data.Positions = new Vector3[data.VerticesCount];
                for (ulong i = 0; i < data.VerticesCount; i++)
                {
                    data.Positions[i] = stream.ReadVector3(endianness);
                }
            }
            if ((data.Flags & VertexFlags.UVs) != 0)
            {
                data.UVs = new Vector3[data.VerticesCount];
                for (ulong i = 0; i < data.VerticesCount; i++)
                {
                    data.UVs[i] = stream.ReadVector3(endianness);
                }
            }
            if ((data.Flags & VertexFlags.Normals) != 0)
            {
                data.Normals = new Vector3[data.VerticesCount];
                for (ulong i = 0; i < data.VerticesCount; i++)
                {
                    data.Normals[i] = stream.ReadVector3(endianness);
                }
            }
            if ((data.Flags & VertexFlags.Tangents) != 0)
            {
                data.Tangents = new Vector3[data.VerticesCount];
                for (ulong i = 0; i < data.VerticesCount; i++)
                {
                    data.Tangents[i] = stream.ReadVector3(endianness);
                }
            }
            if ((data.Flags & VertexFlags.Bitangents) != 0)
            {
                data.Bitangents = new Vector3[data.VerticesCount];
                for (ulong i = 0; i < data.VerticesCount; i++)
                {
                    data.Bitangents[i] = stream.ReadVector3(endianness);
                }
            }
            if ((data.Flags & VertexFlags.Skinned) != 0)
            {
                data.Bones = new BoneData[data.BoneCount];
                for (ulong i = 0; i < data.BoneCount; i++)
                {
                    data.Bones[i].Read(stream, encoding, endianness);
                }
            }

            return data;
        }

        public void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteString(Name, encoding, endianness);
            stream.WriteString(MaterialName, encoding, endianness);
            stream.WriteUInt32(VerticesCount, endianness);
            stream.WriteUInt32(IndicesCount, endianness);
            stream.WriteUInt32(BoneCount, endianness);
            Box.Write(stream, endianness);
            Sphere.Write(stream, endianness);
            stream.WriteInt32((int)Flags, endianness);

            for (ulong i = 0; i < IndicesCount; i++)
            {
                stream.WriteUInt32(Indices[i], endianness);
            }

            if ((Flags & VertexFlags.Colors) != 0)
            {
                for (ulong i = 0; i < VerticesCount; i++)
                {
                    stream.WriteVector4(Colors[i], endianness);
                }
            }
            if ((Flags & VertexFlags.Positions) != 0)
            {
                for (ulong i = 0; i < VerticesCount; i++)
                {
                    stream.WriteVector3(Positions[i], endianness);
                }
            }
            if ((Flags & VertexFlags.UVs) != 0)
            {
                for (ulong i = 0; i < VerticesCount; i++)
                {
                    stream.WriteVector3(UVs[i], endianness);
                }
            }
            if ((Flags & VertexFlags.Normals) != 0)
            {
                for (ulong i = 0; i < VerticesCount; i++)
                {
                    stream.WriteVector3(Normals[i], endianness);
                }
            }
            if ((Flags & VertexFlags.Tangents) != 0)
            {
                for (ulong i = 0; i < VerticesCount; i++)
                {
                    stream.WriteVector3(Tangents[i], endianness);
                }
            }
            if ((Flags & VertexFlags.Bitangents) != 0)
            {
                for (ulong i = 0; i < VerticesCount; i++)
                {
                    stream.WriteVector3(Bitangents[i], endianness);
                }
            }
            if ((Flags & VertexFlags.Skinned) != 0)
            {
                for (ulong i = 0; i < BoneCount; i++)
                {
                    Bones[i].Write(stream, encoding, endianness);
                }
            }
        }

        public IndexBuffer<uint> CreateIndexBuffer(IGraphicsDevice device, CpuAccessFlags accessFlags = CpuAccessFlags.None)
        {
            return new(device, Indices, accessFlags);
        }

        public bool WriteIndexBuffer(IGraphicsContext context, IndexBuffer<uint> ib)
        {
            for (int i = 0; i < IndicesCount; i++)
            {
                ib[i] = Indices[i];
            }

            return ib.Update(context);
        }

        public VertexBuffer<MeshVertex> CreateVertexBuffer(IGraphicsDevice device, CpuAccessFlags accessFlags = CpuAccessFlags.None)
        {
            var stride = sizeof(MeshVertex);
            var size = stride * (int)VerticesCount;
            var vertices = (MeshVertex*)Alloc(size);
            ZeroMemory(vertices, size);

            for (int i = 0; i < VerticesCount; i++)
            {
                MeshVertex vertex = default;

                if ((Flags & VertexFlags.Positions) != 0)
                {
                    vertex.Position = Positions[i];
                }

                if ((Flags & VertexFlags.UVs) != 0)
                {
                    vertex.UV = UVs[i];
                }

                if ((Flags & VertexFlags.Normals) != 0)
                {
                    vertex.Normal = Normals[i];
                }

                if ((Flags & VertexFlags.Tangents) != 0)
                {
                    vertex.Tangent = Tangents[i];
                }

                if ((Flags & VertexFlags.Bitangents) != 0)
                {
                    vertex.Bitangent = Bitangents[i];
                }

                vertices[i] = vertex;
            }

            VertexBuffer<MeshVertex> vertexBuffer = new(device, vertices, VerticesCount, accessFlags);
            Free(vertices);
            return vertexBuffer;
        }

        public VertexBuffer<SkinnedMeshVertex> CreateSkinnedVertexBuffer(IGraphicsDevice device, CpuAccessFlags accessFlags = CpuAccessFlags.None)
        {
            var stride = sizeof(SkinnedMeshVertex);
            var size = stride * (int)VerticesCount;
            var vertices = (SkinnedMeshVertex*)Alloc(size);
            ZeroMemory(vertices, size);
            int m = 0;

            for (int i = 0; i < VerticesCount; i++)
            {
                SkinnedMeshVertex vertex = default;

                if ((Flags & VertexFlags.Positions) != 0)
                {
                    vertex.Position = Positions[i];
                }

                if ((Flags & VertexFlags.UVs) != 0)
                {
                    vertex.UV = UVs[i];
                }

                if ((Flags & VertexFlags.Normals) != 0)
                {
                    vertex.Normal = Normals[i];
                }

                if ((Flags & VertexFlags.Tangents) != 0)
                {
                    vertex.Tangent = Tangents[i];
                }

                if ((Flags & VertexFlags.Bitangents) != 0)
                {
                    vertex.Bitangent = Bitangents[i];
                }

                if ((Flags & VertexFlags.Skinned) != 0)
                {
                    (vertex.BoneIds, vertex.Weights) = GatherBoneData(i);
                }

                vertices[i] = vertex;
            }

            VertexBuffer<SkinnedMeshVertex> vertexBuffer = new(device, vertices, VerticesCount, accessFlags);
            Free(vertices);
            return vertexBuffer;
        }

        public bool WriteVertexBuffer(IGraphicsContext context, VertexBuffer<MeshVertex> vb)
        {
            for (int i = 0; i < VerticesCount; i++)
            {
                MeshVertex vertex = default;

                if ((Flags & VertexFlags.Positions) != 0)
                {
                    vertex.Position = Positions[i];
                }

                if ((Flags & VertexFlags.UVs) != 0)
                {
                    vertex.UV = UVs[i];
                }

                if ((Flags & VertexFlags.Normals) != 0)
                {
                    vertex.Normal = Normals[i];
                }

                if ((Flags & VertexFlags.Tangents) != 0)
                {
                    vertex.Tangent = Tangents[i];
                }

                if ((Flags & VertexFlags.Bitangents) != 0)
                {
                    vertex.Bitangent = Bitangents[i];
                }

                vb[i] = vertex;
            }

            return vb.Update(context);
        }

        public bool WriteSkinnedVertexBuffer(IGraphicsContext context, VertexBuffer<SkinnedMeshVertex> vb)
        {
            for (int i = 0; i < VerticesCount; i++)
            {
                SkinnedMeshVertex vertex = default;

                if ((Flags & VertexFlags.Positions) != 0)
                {
                    vertex.Position = Positions[i];
                }

                if ((Flags & VertexFlags.UVs) != 0)
                {
                    vertex.UV = UVs[i];
                }

                if ((Flags & VertexFlags.Normals) != 0)
                {
                    vertex.Normal = Normals[i];
                }

                if ((Flags & VertexFlags.Tangents) != 0)
                {
                    vertex.Tangent = Tangents[i];
                }

                if ((Flags & VertexFlags.Bitangents) != 0)
                {
                    vertex.Bitangent = Bitangents[i];
                }

                if ((Flags & VertexFlags.Skinned) != 0)
                {
                    (vertex.BoneIds, vertex.Weights) = GatherBoneData(i);
                }

                vb[i] = vertex;
            }

            return vb.Update(context);
        }

        public Face[] GetFacesForVertex(int vertex)
        {
            List<Face> faces = new();
            for (int i = 0; i < IndicesCount;)
            {
                var idx1 = Indices[i++];
                var idx2 = Indices[i++];
                var idx3 = Indices[i++];
                if (idx1 == vertex || idx2 == vertex || idx3 == vertex)
                {
                    faces.Add(new(idx1, idx2, idx3));
                }
            }
            return faces.ToArray();
        }

        public void GetFacesForVertex(int vertex, List<Face> faces)
        {
            for (int i = 0; i < IndicesCount;)
            {
                var idx1 = Indices[i++];
                var idx2 = Indices[i++];
                var idx3 = Indices[i++];
                if (idx1 == vertex || idx2 == vertex || idx3 == vertex)
                {
                    faces.Add(new(idx1, idx2, idx3));
                }
            }
        }

        public void GetFacesForVertex(uint vertex, List<Face> faces)
        {
            faces.Clear();
            for (int i = 0; i < IndicesCount;)
            {
                var idx1 = Indices[i++];
                var idx2 = Indices[i++];
                var idx3 = Indices[i++];
                if (idx1 == vertex || idx2 == vertex || idx3 == vertex)
                {
                    faces.Add(new(idx1, idx2, idx3));
                }
            }
        }

        public Face GetFaceForIndex(uint index)
        {
            for (uint i = 0; i < IndicesCount;)
            {
                var idx1 = Indices[i++];
                var idx2 = Indices[i++];
                var idx3 = Indices[i++];
                if (idx1 == index || idx2 == index || idx3 == index)
                {
                    return new(idx1, idx2, idx3);
                }
            }
            return default;
        }

        public Face GetFaceAtIndex(uint index)
        {
            var i = index * 3;
            var idx1 = Indices[i];
            var idx2 = Indices[i + 1];
            var idx3 = Indices[i + 2];

            return new(idx1, idx2, idx3);
        }

        public Face[] GetNeighborFaces(Face face)
        {
            List<Face> faces = new();
            for (uint i = 0; i < IndicesCount;)
            {
                var idx1 = Indices[i++];
                var idx2 = Indices[i++];
                var idx3 = Indices[i++];
                if (idx1 == face.Index1 || idx2 == face.Index1 || idx3 == face.Index1)
                {
                    faces.Add(new(idx1, idx2, idx3));
                }
                if (idx1 == face.Index2 || idx2 == face.Index2 || idx3 == face.Index2)
                {
                    faces.Add(new(idx1, idx2, idx3));
                }
                if (idx1 == face.Index3 || idx2 == face.Index3 || idx3 == face.Index3)
                {
                    faces.Add(new(idx1, idx2, idx3));
                }
            }
            return faces.ToArray();
        }

        public (Point4 boneIds, Vector4 weigths) GatherBoneData(int vertexId)
        {
            Point4 boneIds = default;
            Vector4 weigths = default;

            int m = 0;
            for (int i = 0; i < Bones.Length; i++)
            {
                var bone = Bones[i];
                for (uint j = 0; j < bone.Weights.Length; j++)
                {
                    var weight = bone.Weights[j];
                    if (weight.VertexId == vertexId)
                    {
                        boneIds[m] = i;
                        weigths[m] = weight.Weight;
                        m++;
                        if (m == 4)
                        {
                            break;
                        }
                    }
                    else
                    {
                        boneIds[m] = -1;
                    }
                }
                if (m == 4)
                {
                    break;
                }
            }

            return (boneIds, weigths);
        }

        public static readonly InputElementDescription[] InputElements =
        {
            new InputElementDescription("POSITION", 0, Format.R32G32B32Float, 0, 0),
            new InputElementDescription("TEXCOORD", 0, Format.R32G32B32Float, 12, 0),
            new InputElementDescription("NORMAL", 0, Format.R32G32B32Float, 24, 0),
            new InputElementDescription("TANGENT", 0, Format.R32G32B32Float, 36, 0),
            new InputElementDescription("BINORMAL", 0, Format.R32G32B32Float, 48, 0),
        };

        public static readonly InputElementDescription[] SkinnedInputElements =
        {
            new InputElementDescription("POSITION", 0, Format.R32G32B32Float, 0, 0),
            new InputElementDescription("TEXCOORD", 0, Format.R32G32B32Float, 12, 0),
            new InputElementDescription("NORMAL", 0, Format.R32G32B32Float, 24, 0),
            new InputElementDescription("TANGENT", 0, Format.R32G32B32Float, 36, 0),
            new InputElementDescription("BINORMAL", 0, Format.R32G32B32Float, 48, 0),
            new InputElementDescription("BLENDINDICES", 0, Format.R32G32B32A32SInt, 60, 0),
            new InputElementDescription("BLENDWEIGHT", 0, Format.R32G32B32A32Float, 76, 0),
        };

        public long IntersectRay(Ray ray)
        {
            if (!Box.Intersects(ray).HasValue)
            {
                return -1;
            }

            long id = -1;
            Vector3 minPos = new(float.MaxValue);
            for (uint i = 0; i < IndicesCount / 3; i++)
            {
                var pos0 = Positions[Indices[i * 3]];
                var pos1 = Positions[Indices[i * 3 + 1]];
                var pos2 = Positions[Indices[i * 3 + 2]];

                if (!ray.Intersects(pos0, pos1, pos2, out var pointInTriangle))
                {
                    continue;
                }

                if (minPos.X < pointInTriangle.X && minPos.Y < pointInTriangle.Y && minPos.Z < pointInTriangle.Z)
                {
                    continue;
                }

                minPos = pointInTriangle;

                var d0 = Vector3.Distance(pos0, pointInTriangle);
                var d1 = Vector3.Distance(pos1, pointInTriangle);
                var d2 = Vector3.Distance(pos2, pointInTriangle);
                var min = Math.Min(d0, Math.Min(d1, d2));
                if (min == d0)
                {
                    return Indices[i * 3];
                }

                if (min == d1)
                {
                    return Indices[i * 3 + 1];
                }

                if (min == d2)
                {
                    return Indices[i * 3 + 2];
                }
            }

            return id;
        }

        public void GenerateNTB()
        {
            GenVertexNormalsProcess.GenMeshVertexNormals2(this);
            CalcTangentsProcess.ProcessMesh2(this);
        }

        public void GenerateBounds()
        {
            Box = BoundingBoxHelper.Compute(Positions);
            Sphere = BoundingSphere.CreateFromBoundingBox(Box);
        }

        public void Debone()
        {
            BoneCount = 0;
            Bones = null;
            Flags ^= VertexFlags.Skinned;
        }

        public Face[] GetFaces()
        {
            Face[] faces = new Face[IndicesCount / 3];
            for (int i = 0; i < faces.Length; i++)
            {
                faces[i] = new(Indices[i * 3], Indices[i * 3 + 1], Indices[i * 3 + 2]);
            }
            return faces;
        }
    }
}