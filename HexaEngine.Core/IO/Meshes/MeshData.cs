namespace HexaEngine.Core.IO.Meshes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Meshes.Processing;
    using HexaEngine.Mathematics;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Numerics;
    using System.Text;

    /// <summary>
    /// Represents mesh data including vertex and index information, bounding volumes, and vertex flags.
    /// </summary>
    public unsafe class MeshData
    {
        /// <summary>
        /// The name of the mesh.
        /// </summary>
        public string Name;

        /// <summary>
        /// The name of the material associated with the mesh.
        /// </summary>
        public string MaterialName;

        /// <summary>
        /// The number of vertices in the mesh.
        /// </summary>
        public uint VerticesCount;

        /// <summary>
        /// The number of indices in the mesh.
        /// </summary>
        public uint IndicesCount;

        /// <summary>
        /// The number of bones (weights) in the mesh.
        /// </summary>
        public uint BoneCount;

        /// <summary>
        /// The bounding box of the mesh.
        /// </summary>
        public BoundingBox Box;

        /// <summary>
        /// The bounding sphere of the mesh.
        /// </summary>
        public BoundingSphere Sphere;

        /// <summary>
        /// Flags indicating which vertex components are present in the mesh.
        /// </summary>
        public VertexFlags Flags;

        /// <summary>
        /// The array of indices defining the mesh topology.
        /// </summary>
        public uint[] Indices;

        /// <summary>
        /// The array of colors associated with each vertex.
        /// </summary>
        public Vector4[] Colors;

        /// <summary>
        /// The array of 3D positions of each vertex.
        /// </summary>
        public Vector3[] Positions;

        /// <summary>
        /// The array of 3D texture coordinates (UVs) of each vertex.
        /// </summary>
        public Vector3[] UVs;

        /// <summary>
        /// The array of 3D normals associated with each vertex.
        /// </summary>
        public Vector3[] Normals;

        /// <summary>
        /// The array of 3D tangents associated with each vertex.
        /// </summary>
        public Vector3[] Tangents;

        /// <summary>
        /// The array of 3D bitangents associated with each vertex.
        /// </summary>
        public Vector3[] Bitangents;

        /// <summary>
        /// The array of bone data representing the skeleton structure of the mesh.
        /// </summary>
        public BoneData[]? Bones;

#nullable disable

        /// <summary>
        /// Private default constructor for internal use.
        /// </summary>
        private MeshData()
        {
        }

#nullable restore

        /// <summary>
        /// Initializes a new instance of the <see cref="MeshData"/> class with specified parameters.
        /// </summary>
        /// <param name="name">The name of the mesh.</param>
        /// <param name="materialName">The name of the material associated with the mesh.</param>
        /// <param name="box">The bounding box of the mesh.</param>
        /// <param name="sphere">The bounding sphere of the mesh.</param>
        /// <param name="vertexCount">The number of vertices in the mesh.</param>
        /// <param name="indexCount">The number of indices in the mesh.</param>
        /// <param name="weightCount">The number of bones (weights) in the mesh.</param>
        /// <param name="indices">The array of indices defining the mesh topology.</param>
        /// <param name="colors">The array of colors associated with each vertex.</param>
        /// <param name="positions">The array of 3D positions of each vertex.</param>
        /// <param name="uvs">The array of 3D texture coordinates (UVs) of each vertex.</param>
        /// <param name="normals">The array of 3D normals associated with each vertex.</param>
        /// <param name="tangents">The array of 3D tangents associated with each vertex.</param>
        /// <param name="bitangents">The array of 3D bitangents associated with each vertex.</param>
        /// <param name="bones">The array of bone data representing the skeleton structure of the mesh.</param>
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

            Colors = colors ?? [];
            if (colors != null)
            {
                Flags |= VertexFlags.Colors;
            }

            Positions = positions ?? [];
            if (positions != null)
            {
                Flags |= VertexFlags.Positions;
            }

            UVs = uvs ?? [];
            if (uvs != null)
            {
                Flags |= VertexFlags.UVs;
            }

            Normals = normals ?? [];
            if (normals != null)
            {
                Flags |= VertexFlags.Normals;
            }

            Tangents = tangents ?? [];
            if (tangents != null)
            {
                Flags |= VertexFlags.Tangents;
            }

            Bitangents = bitangents ?? [];
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

        /// <summary>
        /// Reads mesh data from a stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding used for reading strings.</param>
        /// <param name="endianness">The endianness used for reading numerical data.</param>
        public static MeshData Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            MeshData data = new();
            data.Name = stream.ReadString(encoding, endianness) ?? string.Empty;
            data.MaterialName = stream.ReadString(encoding, endianness) ?? string.Empty;
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

        /// <summary>
        /// Writes the mesh data to a stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to write the data to.</param>
        /// <param name="encoding">The encoding used for writing strings.</param>
        /// <param name="endianness">The endianness used for writing numerical data.</param>
        public void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            // Write basic information
            stream.WriteString(Name, encoding, endianness);
            stream.WriteString(MaterialName, encoding, endianness);
            stream.WriteUInt32(VerticesCount, endianness);
            stream.WriteUInt32(IndicesCount, endianness);
            stream.WriteUInt32(BoneCount, endianness);
            Box.Write(stream, endianness);
            Sphere.Write(stream, endianness);
            stream.WriteInt32((int)Flags, endianness);

            // Write indices
            for (ulong i = 0; i < IndicesCount; i++)
            {
                stream.WriteUInt32(Indices[i], endianness);
            }

            // Write optional vertex components if present
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
#nullable disable
            if ((Flags & VertexFlags.Skinned) != 0)
            {
                for (ulong i = 0; i < BoneCount; i++)
                {
                    Bones[i].Write(stream, encoding, endianness);
                }
            }
#nullable restore
        }

        /// <summary>
        /// Creates an index buffer for the mesh data on the specified graphics device.
        /// </summary>
        /// <param name="device">The graphics device to create the index buffer on.</param>
        /// <param name="accessFlags">Optional CPU access flags for the index buffer.</param>
        /// <returns>The created index buffer.</returns>
        public IndexBuffer<uint> CreateIndexBuffer(IGraphicsDevice device, CpuAccessFlags accessFlags = CpuAccessFlags.None)
        {
            return new(device, Indices, accessFlags);
        }

        /// <summary>
        /// Writes the mesh data indices to the provided index buffer and updates it on the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context to perform the update on.</param>
        /// <param name="ib">The index buffer to write to and update.</param>
        /// <returns>True if the buffer has been updated; otherwise, false.</returns>
        public bool WriteIndexBuffer(IGraphicsContext context, IndexBuffer<uint> ib)
        {
            for (int i = 0; i < IndicesCount; i++)
            {
                ib[i] = Indices[i];
            }

            return ib.Update(context);
        }

        /// <summary>
        /// Creates a vertex buffer for the mesh data on the specified graphics device.
        /// </summary>
        /// <param name="device">The graphics device to create the vertex buffer on.</param>
        /// <param name="accessFlags">Optional CPU access flags for the vertex buffer.</param>
        /// <returns>The created vertex buffer.</returns>
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

        /// <summary>
        /// Creates a skinned vertex buffer for the mesh data on the specified graphics device.
        /// </summary>
        /// <param name="device">The graphics device to create the vertex buffer on.</param>
        /// <param name="accessFlags">Optional CPU access flags for the vertex buffer.</param>
        /// <returns>The created skinned vertex buffer.</returns>
        public VertexBuffer<SkinnedMeshVertex> CreateSkinnedVertexBuffer(IGraphicsDevice device, CpuAccessFlags accessFlags = CpuAccessFlags.None)
        {
            var stride = sizeof(SkinnedMeshVertex);
            var size = stride * (int)VerticesCount;
            var vertices = (SkinnedMeshVertex*)Alloc(size);
            ZeroMemory(vertices, size);

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

        /// <summary>
        /// Writes the mesh data vertices to the provided vertex buffer and updates it on the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context to perform the update on.</param>
        /// <param name="vb">The vertex buffer to write to and update.</param>
        /// <returns>True if the buffer has been updated; otherwise, false.</returns>
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

        /// <summary>
        /// Writes the skinned mesh data vertices to the provided skinned vertex buffer and updates it on the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context to perform the update on.</param>
        /// <param name="vb">The skinned vertex buffer to write to and update.</param>
        /// <returns>True if the buffer has been updated; otherwise, false.</returns>
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

        /// <summary>
        /// Gets an array of faces that include the specified vertex.
        /// </summary>
        /// <param name="vertex">The index of the vertex.</param>
        /// <returns>An array of faces that include the specified vertex.</returns>
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

        /// <summary>
        /// Gets the faces that include the specified vertex and adds them to the provided list.
        /// </summary>
        /// <param name="vertex">The index of the vertex.</param>
        /// <param name="faces">The list to which the faces will be added.</param>
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

        /// <summary>
        /// Clears the provided list and adds the faces that include the specified vertex to it.
        /// </summary>
        /// <param name="vertex">The index of the vertex.</param>
        /// <param name="faces">The list to which the faces will be added.</param>
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

        /// <summary>
        /// Gets the face that includes the specified index.
        /// </summary>
        /// <param name="index">The index of the face.</param>
        /// <returns>The face that includes the specified index.</returns>
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

        /// <summary>
        /// Gets the face at the specified index.
        /// </summary>
        /// <param name="index">The index of the face.</param>
        /// <returns>The face at the specified index.</returns>
        public Face GetFaceAtIndex(uint index)
        {
            var i = index * 3;
            var idx1 = Indices[i];
            var idx2 = Indices[i + 1];
            var idx3 = Indices[i + 2];

            return new(idx1, idx2, idx3);
        }

        /// <summary>
        /// Gets an array of faces that are neighbors to the specified face.
        /// </summary>
        /// <param name="face">The reference face.</param>
        /// <returns>An array of faces that are neighbors to the specified face.</returns>
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

        /// <summary>
        /// Gathers bone data for the specified vertex ID.
        /// </summary>
        /// <param name="vertexId">The ID of the vertex.</param>
        /// <returns>A tuple containing bone IDs and weights for the specified vertex ID.</returns>
        public (Point4 boneIds, Vector4 weigths) GatherBoneData(int vertexId)
        {
            Point4 boneIds = default;
            Vector4 weigths = default;

#nullable disable
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
#nullable restore

            return (boneIds, weigths);
        }

        /// <summary>
        /// Describes the input elements for a non-skinned mesh.
        /// </summary>
        public static readonly InputElementDescription[] InputElements =
        {
            new InputElementDescription("POSITION", 0, Format.R32G32B32Float, 0, 0),
            new InputElementDescription("TEXCOORD", 0, Format.R32G32B32Float, 12, 0),
            new InputElementDescription("NORMAL", 0, Format.R32G32B32Float, 24, 0),
            new InputElementDescription("TANGENT", 0, Format.R32G32B32Float, 36, 0),
            new InputElementDescription("BINORMAL", 0, Format.R32G32B32Float, 48, 0),
        };

        /// <summary>
        /// Describes the input elements for a skinned mesh.
        /// </summary>
        public static readonly InputElementDescription[] SkinnedInputElements =
        {
            new InputElementDescription("POSITION", 0, Format.R32G32B32Float, 0, 0),
            new InputElementDescription("TEXCOORD", 0, Format.R32G32B32Float, 12, 0),
            new InputElementDescription("NORMAL", 0, Format.R32G32B32Float, 24, 0),
            new InputElementDescription("TANGENT", 0, Format.R32G32B32Float, 36, 0),
            new InputElementDescription("BINORMAL", 0, Format.R32G32B32Float, 48, 0),
            new InputElementDescription("BLENDINDICES", 0, Format.R32G32B32A32UInt, 60, 0),
            new InputElementDescription("BLENDWEIGHT", 0, Format.R32G32B32A32Float, 76, 0),
        };

        /// <summary>
        /// Intersects the mesh with a ray and returns the index of the closest intersected vertex.
        /// </summary>
        /// <param name="ray">The ray used for intersection.</param>
        /// <returns>The index of the closest intersected vertex or -1 if no intersection occurs.</returns>
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

        /// <summary>
        /// Calculates the intersection between the ray and the triangle mesh, returning the distance to the intersection point.
        /// </summary>
        /// <param name="ray">The ray to perform the intersection test with.</param>
        /// <returns>
        /// The distance along the ray to the intersection point. Returns <c>null</c> if there is no intersection with the mesh.
        /// </returns>
        public float? Intersect(Ray ray)
        {
            // Check if the ray intersects the bounding box of the triangle mesh.
            if (!Box.Intersects(ray).HasValue)
            {
                // No intersection with the bounding box, so no intersection with the mesh.
                return null;
            }

            // Initialize a vector representing the minimum intersection point.
            Vector3 minPos = new(float.MaxValue);

            // Iterate through each triangle in the mesh.
            for (uint i = 0; i < IndicesCount / 3; i++)
            {
                // Get the vertices of the current triangle.
                var pos0 = Positions[Indices[i * 3]];
                var pos1 = Positions[Indices[i * 3 + 1]];
                var pos2 = Positions[Indices[i * 3 + 2]];

                // Check if the ray intersects the current triangle.
                if (!ray.Intersects(pos0, pos1, pos2, out var pointInTriangle))
                {
                    // No intersection with this triangle, continue to the next one.
                    continue;
                }

                // Check if the intersection point is closer than the current minimum.
                if (minPos.X < pointInTriangle.X && minPos.Y < pointInTriangle.Y && minPos.Z < pointInTriangle.Z)
                {
                    // The intersection point is not closer, continue to the next triangle.
                    continue;
                }

                minPos = pointInTriangle;

                // Return the distance from the ray origin to the intersection point.
                return (float)(ray.Position - minPos).Length();
            }

            return null;
        }

        /// <summary>
        /// Generates normal, tangent, and bitangent vectors for the mesh vertices.
        /// </summary>
        public void GenerateNTB()
        {
            GenVertexNormalsProcess.GenMeshVertexNormals2(this);
            CalcTangentsProcess.ProcessMesh2(this);
        }

        /// <summary>
        /// Generates bounding box and bounding sphere for the mesh.
        /// </summary>
        public void GenerateBounds()
        {
            Box = BoundingBoxHelper.Compute(Positions);
            Sphere = BoundingSphere.CreateFromBoundingBox(Box);
        }

        /// <summary>
        /// Removes bone-related information from the mesh.
        /// </summary>
        public void Debone()
        {
            BoneCount = 0;
            Bones = null;
            Flags ^= VertexFlags.Skinned;
        }

        /// <summary>
        /// Gets an array of faces representing the triangles in the mesh.
        /// </summary>
        /// <returns>An array of faces.</returns>
        public Face[] GetFaces()
        {
            Face[] faces = new Face[IndicesCount / 3];
            for (int i = 0; i < faces.Length; i++)
            {
                faces[i] = new(Indices[i * 3], Indices[i * 3 + 1], Indices[i * 3 + 2]);
            }
            return faces;
        }

        /// <summary>
        /// Gets a list of faces representing the triangles in the mesh.
        /// </summary>
        /// <param name="faces">The list to populate with faces.</param>
        public void GetFaces(List<Face> faces)
        {
            uint facesCount = IndicesCount / 3;
            faces.Clear();
            faces.Capacity = (int)facesCount;
            for (int i = 0; i < facesCount; i++)
            {
                faces.Add(new(Indices[i * 3], Indices[i * 3 + 1], Indices[i * 3 + 2]));
            }
        }
    }
}