namespace HexaEngine.Core.IO.Meshes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Mathematics;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Numerics;
    using System.Text;

    public unsafe struct MeshData
    {
        public string Name;
        public MaterialData Material;
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

        public MeshData(string name, MaterialData material, BoundingBox box, BoundingSphere sphere, uint vertexCount, uint indexCount, uint weightCount, uint[] indices, Vector4[]? colors, Vector3[]? positions, Vector3[]? uvs, Vector3[]? normals, Vector3[]? tangents, Vector3[]? bitangents, BoneData[]? bones)
        {
            Name = name;
            Material = material;
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
            data.Material = MaterialData.Read(stream, encoding, endianness);
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
            Material.Write(stream, encoding, endianness);
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
                    throw new InvalidOperationException();
                if (m > size)
                    throw new InternalBufferOverflowException();
            }

            var result = device.CreateBuffer((void*)buffer, (uint)size, new(size, BindFlags.VertexBuffer, usage, accessFlags));
            Free(buffer);
            return result;
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
                            break;
                    }
                }
                if (m == 4)
                    break;
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
                elements[i++] = new InputElementDescription("COLOR", 0, Format.RGBA32Float, 0);
            }

            if ((Flags & VertexFlags.Positions) != 0)
            {
                elements[i++] = new InputElementDescription("POSITION", 0, Format.RGB32Float, 0);
            }

            if ((Flags & VertexFlags.UVs) != 0)
            {
                elements[i++] = new InputElementDescription("TEXCOORD", 0, Format.RGB32Float, 0);
            }

            if ((Flags & VertexFlags.Normals) != 0)
            {
                elements[i++] = new InputElementDescription("NORMAL", 0, Format.RGB32Float, 0);
            }

            if ((Flags & VertexFlags.Tangents) != 0)
            {
                elements[i++] = new InputElementDescription("TANGENT", 0, Format.RGB32Float, 0);
            }

            if ((Flags & VertexFlags.Bitangents) != 0)
            {
                elements[i++] = new InputElementDescription("BINORMAL", 0, Format.RGB32Float, 0);
            }

            if ((Flags & VertexFlags.Skinned) != 0)
            {
                elements[i++] = new InputElementDescription("BLENDINDICES", 0, Format.RGBA32UInt, 0);
                elements[i++] = new InputElementDescription("BLENDWEIGHT", 0, Format.RGBA32Float, 0);
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
    }
}