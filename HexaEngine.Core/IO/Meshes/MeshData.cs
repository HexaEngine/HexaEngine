namespace HexaEngine.Core.IO.Meshes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;
    using System;
    using System.IO;
    using System.Numerics;
    using System.Text;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    public unsafe struct MeshData
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
            MeshData data = default;
            data.Name = stream.ReadString(encoding, endianness);
            data.MaterialName = stream.ReadString(encoding, endianness);
            data.VerticesCount = stream.ReadUInt(endianness);
            data.IndicesCount = stream.ReadUInt(endianness);
            data.BoneCount = stream.ReadUInt(endianness);
            data.Box = BoundingBox.Read(stream, endianness);
            data.Sphere = BoundingSphere.Read(stream, endianness);
            data.Flags = (VertexFlags)stream.ReadInt(endianness);

            data.Indices = new uint[data.IndicesCount];
            for (ulong i = 0; i < data.IndicesCount; i++)
            {
                data.Indices[i] = stream.ReadUInt(endianness);
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
            stream.WriteUInt(VerticesCount, endianness);
            stream.WriteUInt(IndicesCount, endianness);
            stream.WriteUInt(BoneCount, endianness);
            Box.Write(stream, endianness);
            Sphere.Write(stream, endianness);
            stream.WriteInt((int)Flags, endianness);

            for (ulong i = 0; i < IndicesCount; i++)
            {
                stream.WriteUInt(Indices[i], endianness);
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

        public IBuffer CreateIndexBuffer(IGraphicsDevice device, Usage usage = Usage.Immutable, CpuAccessFlags accessFlags = CpuAccessFlags.None)
        {
            fixed (uint* ptr = Indices)
            {
                return device.CreateBuffer(ptr, (uint)Indices.Length, BindFlags.IndexBuffer, usage, accessFlags);
            }
        }

        public void WriteIndexBuffer(IGraphicsContext context, IBuffer ib)
        {
            var size = sizeof(uint) * (int)IndicesCount;
            var buffer = (uint*)Alloc(size);
            for (int i = 0; i < IndicesCount; i++)
            {
                buffer[i] = Indices[i];
            }

            context.Write(ib, buffer, size);

            Free(buffer);
        }

        public IBuffer CreateVertexBuffer(IGraphicsDevice device, Usage usage = Usage.Immutable, CpuAccessFlags accessFlags = CpuAccessFlags.None)
        {
            var stride = (int)GetStride();
            var size = stride * (int)VerticesCount;
            var buffer = (byte*)Alloc(size);
            Zero(buffer, size);
            int m = 0;

            for (int i = 0; i < VerticesCount; i++)
            {
                if ((Flags & VertexFlags.Colors) != 0)
                {
                    Colors[i].CopyTo(&m, buffer);
                }

                if ((Flags & VertexFlags.Positions) != 0)
                {
                    Positions[i].CopyTo(&m, buffer);
                }

                if ((Flags & VertexFlags.UVs) != 0)
                {
                    UVs[i].CopyTo(&m, buffer);
                }

                if ((Flags & VertexFlags.Normals) != 0)
                {
                    Normals[i].CopyTo(&m, buffer);
                }

                if ((Flags & VertexFlags.Tangents) != 0)
                {
                    Tangents[i].CopyTo(&m, buffer);
                }

                if ((Flags & VertexFlags.Bitangents) != 0)
                {
                    Bitangents[i].CopyTo(&m, buffer);
                }

                if ((Flags & VertexFlags.Skinned) != 0)
                {
                    var (boneIds, weigths) = GatherBoneData(i);
                    fixed (uint* p = boneIds)
                    {
                        MemoryCopy(p, &buffer[m], sizeof(uint) * 4);
                        m += sizeof(uint) * 4;
                    }
                    fixed (float* p = weigths)
                    {
                        MemoryCopy(p, &buffer[m], sizeof(float) * 4);
                        m += sizeof(float) * 4;
                    }
                }

                if (m % stride != 0)
                {
                    throw new InvalidOperationException();
                }

                if (m > size)
                {
                    throw new InternalBufferOverflowException();
                }
            }

            var result = device.CreateBuffer((void*)buffer, (uint)size, new(size, BindFlags.VertexBuffer, usage, accessFlags));
            Free(buffer);
            return result;
        }

        public void WriteVertexBuffer(IGraphicsContext context, IBuffer vb)
        {
            var stride = (int)GetStride();
            var size = stride * (int)VerticesCount;
            var buffer = (byte*)Alloc(size);
            Zero(buffer, size);
            int m = 0;

            for (int i = 0; i < VerticesCount; i++)
            {
                if ((Flags & VertexFlags.Colors) != 0)
                {
                    Colors[i].CopyTo(&m, buffer);
                }

                if ((Flags & VertexFlags.Positions) != 0)
                {
                    Positions[i].CopyTo(&m, buffer);
                }

                if ((Flags & VertexFlags.UVs) != 0)
                {
                    UVs[i].CopyTo(&m, buffer);
                }

                if ((Flags & VertexFlags.Normals) != 0)
                {
                    Normals[i].CopyTo(&m, buffer);
                }

                if ((Flags & VertexFlags.Tangents) != 0)
                {
                    Tangents[i].CopyTo(&m, buffer);
                }

                if ((Flags & VertexFlags.Bitangents) != 0)
                {
                    Bitangents[i].CopyTo(&m, buffer);
                }

                if ((Flags & VertexFlags.Skinned) != 0)
                {
                    var (boneIds, weigths) = GatherBoneData(i);
                    fixed (uint* p = boneIds)
                    {
                        MemoryCopy(p, &buffer[m], sizeof(uint) * 4);
                        m += sizeof(uint) * 4;
                    }
                    fixed (float* p = weigths)
                    {
                        MemoryCopy(p, &buffer[m], sizeof(float) * 4);
                        m += sizeof(float) * 4;
                    }
                }

                if (m % stride != 0)
                {
                    throw new InvalidOperationException();
                }

                if (m > size)
                {
                    throw new InternalBufferOverflowException();
                }
            }

            context.Write(vb, buffer, size);

            Free(buffer);
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

        public unsafe uint GetStride()
        {
            int result = 0;
            if ((Flags & VertexFlags.Colors) != 0)
            {
                result += sizeof(Vector4);
            }

            if ((Flags & VertexFlags.Positions) != 0)
            {
                result += sizeof(Vector3);
            }

            if ((Flags & VertexFlags.UVs) != 0)
            {
                result += sizeof(Vector3);
            }

            if ((Flags & VertexFlags.Normals) != 0)
            {
                result += sizeof(Vector3);
            }

            if ((Flags & VertexFlags.Tangents) != 0)
            {
                result += sizeof(Vector3);
            }

            if ((Flags & VertexFlags.Bitangents) != 0)
            {
                result += sizeof(Vector3);
            }

            if ((Flags & VertexFlags.Skinned) != 0)
            {
                result += sizeof(int) * 4 + sizeof(float) * 4;
            }

            return (uint)result;
        }

        public (uint[] boneIds, float[] weigths) GatherBoneData(int vertexId)
        {
            uint[] boneIds = new uint[4];
            float[] weigths = new float[4];
            uint m = 0;
            for (uint i = 0; i < Bones.Length; i++)
            {
                var bone = Bones[i];
                for (uint j = 0; j < bone.Weights.Length; j++)
                {
                    var weight = bone.Weights[j];
                    if (weight.VertexId == vertexId)
                    {
                        boneIds[m] = i + 1;
                        weigths[m] = weight.Weight;
                        m++;
                        if (m == 4)
                        {
                            break;
                        }
                    }
                }
                if (m == 4)
                {
                    break;
                }
            }

            return (boneIds, weigths);
        }

        public InputElementDescription[] GetInputElements()
        {
            var count = ((uint)Flags).Bitcount();
            if ((Flags & VertexFlags.Skinned) != 0)
            {
                count++;
            }
            var elements = new InputElementDescription[count];
            int i = 0;
            if ((Flags & VertexFlags.Colors) != 0)
            {
                elements[i++] = new InputElementDescription("COLOR", 0, Format.R32G32B32A32Float, 0);
            }

            if ((Flags & VertexFlags.Positions) != 0)
            {
                elements[i++] = new InputElementDescription("POSITION", 0, Format.R32G32B32Float, 0);
            }

            if ((Flags & VertexFlags.UVs) != 0)
            {
                elements[i++] = new InputElementDescription("TEXCOORD", 0, Format.R32G32B32Float, 0);
            }

            if ((Flags & VertexFlags.Normals) != 0)
            {
                elements[i++] = new InputElementDescription("NORMAL", 0, Format.R32G32B32Float, 0);
            }

            if ((Flags & VertexFlags.Tangents) != 0)
            {
                elements[i++] = new InputElementDescription("TANGENT", 0, Format.R32G32B32Float, 0);
            }

            if ((Flags & VertexFlags.Bitangents) != 0)
            {
                elements[i++] = new InputElementDescription("BINORMAL", 0, Format.R32G32B32Float, 0);
            }

            if ((Flags & VertexFlags.Skinned) != 0)
            {
                elements[i++] = new InputElementDescription("BLENDINDICES", 0, Format.R32G32B32A32UInt, 0);
                elements[i++] = new InputElementDescription("BLENDWEIGHT", 0, Format.R32G32B32A32Float, 0);
            }

            return elements;
        }

        public ShaderMacro[] GetShaderMacros()
        {
            var count = ((uint)Flags).Bitcount();
            var macros = new ShaderMacro[count];
            int i = 0;
            if ((Flags & VertexFlags.Colors) != 0)
            {
                macros[i++] = new ShaderMacro("VtxColor", "1");
            }

            if ((Flags & VertexFlags.Positions) != 0)
            {
                macros[i++] = new ShaderMacro("VtxPosition", "1");
            }

            if ((Flags & VertexFlags.UVs) != 0)
            {
                macros[i++] = new ShaderMacro("VtxUV", "1");
            }

            if ((Flags & VertexFlags.Normals) != 0)
            {
                macros[i++] = new ShaderMacro("VtxNormal", "1");
            }

            if ((Flags & VertexFlags.Tangents) != 0)
            {
                macros[i++] = new ShaderMacro("VtxTangent", "1");
            }

            if ((Flags & VertexFlags.Bitangents) != 0)
            {
                macros[i++] = new ShaderMacro("VtxBitangent", "1");
            }

            if ((Flags & VertexFlags.Skinned) != 0)
            {
                macros[i++] = new ShaderMacro("VtxSkinned", "1");
            }

            return macros;
        }

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
            var faces = IndicesCount / 3;
            UnsafeList<Vector3> tempNormal = new();
            UnsafeList<Vector3> tempTangent = new();

            for (uint i = 0; i < faces; i++)
            {
                var face = new Face(Indices[i * 3], Indices[i * 3 + 1], Indices[i * 3 + 2]);
                var vtxP1 = Positions[face.Index1];
                var vtxP2 = Positions[face.Index2];
                var vtxP3 = Positions[face.Index3];

                Vector3 v = vtxP2 - vtxP1;
                Vector3 w = vtxP3 - vtxP1;

                Vector3 normal = Vector3.Normalize(Vector3.Cross(v, w));

                tempNormal.Add(normal);

                float sx = UVs[face.Index2].X - UVs[face.Index1].X, sy = UVs[face.Index2].Y - UVs[face.Index1].Y;
                float tx = UVs[face.Index3].X - UVs[face.Index1].X, ty = UVs[face.Index3].Y - UVs[face.Index1].Y;

                float dirCorrection = (tx * sy - ty * sx) < 0.0f ? -1.0f : 1.0f;

                if (sx * ty == sy * tx)
                {
                    sx = 0.0f;
                    sy = 1.0f;
                    tx = 1.0f;
                    ty = 0.0f;
                }

                Vector3 tangent, bitangent;
                tangent.X = (w.X * sy - v.X * ty) * dirCorrection;
                tangent.Y = (w.Y * sy - v.Y * ty) * dirCorrection;
                tangent.Z = (w.Z * sy - v.Z * ty) * dirCorrection;
                bitangent.X = (-w.X * sx + v.X * tx) * dirCorrection;
                bitangent.Y = (-w.Y * sx + v.Y * tx) * dirCorrection;
                bitangent.Z = (-w.Z * sx + v.Z * tx) * dirCorrection;
                tempTangent.Add(tangent);
            }

            Vector3 normalSum = default;
            Vector3 tangentSum = default;

            for (int i = 0; i < VerticesCount; ++i)
            {
                // Check which triangles use this vertex
                for (int j = 0; j < faces; ++j)
                {
                    if (Indices[j * 3] == i || Indices[j * 3 + 1] == i || Indices[j * 3 + 2] == i)
                    {
                        normalSum += tempNormal[j];
                        tangentSum += tempTangent[j];
                    }
                }

                normalSum = Vector3.Normalize(normalSum);
                tangentSum = Vector3.Normalize(tangentSum);

                Normals[i] = normalSum;
                Tangents[i] = tangentSum;
                Bitangents[i] = Vector3.Cross(normalSum, tangentSum);

                normalSum = default;
                tangentSum = default;
            }

            tempNormal.Free();
            tempTangent.Free();
        }

        public void RecomputeNormals(Face lface)
        {
            UnsafeList<Vector3> tempNormal = new();
            Vector3 unnormalizedNormal;
            Vector3 edge1, edge2;

            UnsafeList<Vector3> tempTangent = new();
            Vector3 tangent;
            float tcU1, tcV1, tcU2, tcV2;

            Face[] faces = GetNeighborFaces(lface);

            var facesCount = faces.Length;

            for (uint i = 0; i < facesCount;)
            {
                var face = faces[i];
                var vtxP0 = Positions[face.Index1];
                var vtxP1 = Positions[face.Index2];
                var vtxP2 = Positions[face.Index3];

                edge1 = vtxP0 - vtxP2;
                edge2 = vtxP2 - vtxP1;

                unnormalizedNormal = Vector3.Cross(edge1, edge2);

                tempNormal.Add(unnormalizedNormal);

                var vtxUV0 = UVs[face.Index1];
                var vtxUV1 = UVs[face.Index2];
                var vtxUV2 = UVs[face.Index3];

                //Find first texture coordinate edge 2d vector
                tcU1 = vtxUV0.X - vtxUV2.X;
                tcV1 = vtxUV0.Y - vtxUV2.Y;

                //Find second texture coordinate edge 2d vector
                tcU2 = vtxUV2.X - vtxUV1.X;
                tcV2 = vtxUV2.Y - vtxUV1.Y;

                //Find tangent using both tex coord edges and position edges
                tangent = new Vector3((tcV1 * edge1.X - tcV2 * edge2.X) * (1.0f / (tcU1 * tcV2 - tcU2 * tcV1)),
                                      (tcV1 * edge1.Y - tcV2 * edge2.Y) * (1.0f / (tcU1 * tcV2 - tcU2 * tcV1)),
                                      (tcV1 * edge1.Z - tcV2 * edge2.Z) * (1.0f / (tcU1 * tcV2 - tcU2 * tcV1)));

                tempTangent.Add(tangent);
            }

            Vector3 normalSum = default;
            Vector3 tangentSum = default;
            int facesUsing = 0;

            for (int i = 0; i < VerticesCount; ++i)
            {
                //Check which triangles use this vertex
                for (int j = 0; j < facesCount; ++j)
                {
                    var face = faces[j];
                    if (face.Index1 == i || face.Index2 == i || face.Index3 == i)
                    {
                        normalSum += tempNormal[j];
                        tangentSum += tempTangent[j];

                        facesUsing++;
                    }
                }

                normalSum /= facesUsing;
                tangentSum /= facesUsing;

                normalSum = Vector3.Normalize(normalSum);
                tangentSum = Vector3.Normalize(tangentSum);

                Normals[i] = normalSum;
                Tangents[i] = tangentSum;
                Bitangents[i] = Vector3.Cross(normalSum, tangentSum);

                normalSum = default;
                tangentSum = default;
                facesUsing = 0;
            }
        }
    }
}