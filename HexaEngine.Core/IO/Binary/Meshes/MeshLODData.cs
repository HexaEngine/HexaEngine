namespace HexaEngine.Core.IO.Binary.Meshes
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Meshes.Processing;
    using HexaEngine.Core.MeshDecimator.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Numerics;

    public class MeshLODData : ILODData
    {
        private VertexFlags flags;
        private uint lodLevel;
        private uint vertexCount;
        private uint indexCount;
        private BoundingBox box;
        private BoundingSphere sphere;
        private uint[] indices;
        private Vector4[] colors;
        private Vector3[] positions;
        private UVChannel[] uvChannels;
        private Vector3[] normals;
        private Vector3[] tangents;
        private BoneWeight[]? boneWeights;

        /// <summary>
        /// The LOD-Level of the mesh.
        /// </summary>
        public uint LODLevel { get => lodLevel; set => lodLevel = value; }

        /// <summary>
        /// The number of vertices in the mesh.
        /// </summary>
        public uint VertexCount { get => vertexCount; set => vertexCount = value; }

        /// <summary>
        /// The number of indices in the mesh.
        /// </summary>
        public uint IndexCount { get => indexCount; set => indexCount = value; }

        /// <summary>
        /// The bounding box of the mesh.
        /// </summary>
        public BoundingBox Box { get => box; set => box = value; }

        /// <summary>
        /// The bounding sphere of the mesh.
        /// </summary>
        public BoundingSphere Sphere { get => sphere; set => sphere = value; }

        /// <summary>
        /// The array of indices defining the mesh topology.
        /// </summary>
        public uint[] Indices { get => indices; set => indices = value; }

        /// <summary>
        /// The array of colors associated with each vertex.
        /// </summary>
        public Vector4[] Colors { get => colors; set => colors = value; }

        /// <summary>
        /// The array of 3D positions of each vertex.
        /// </summary>
        public Vector3[] Positions { get => positions; set => positions = value; }

        /// <summary>
        /// UV Channels.
        /// </summary>
        public UVChannel[] UVChannels { get => uvChannels; set => uvChannels = value; }

        /// <summary>
        /// The array of 3D normals associated with each vertex.
        /// </summary>
        public Vector3[] Normals { get => normals; set => normals = value; }

        /// <summary>
        /// The array of 3D tangents associated with each vertex.
        /// </summary>
        public Vector3[] Tangents { get => tangents; set => tangents = value; }

        /// <summary>
        /// The array of bone weights associated with each vertex.
        /// </summary>
        public BoneWeight[]? BoneWeights { get => boneWeights; set => boneWeights = value; }

#nullable disable

        /// <summary>
        /// Private default constructor for internal use.
        /// </summary>
        private MeshLODData()
        {
        }

#nullable restore

        /// <summary>
        /// Creates a new instance of <see cref="MeshLODData"/>.
        /// </summary>
        /// <param name="lodLevel">The LOD Level.</param>
        /// <param name="verticesCount">The vertex count.</param>
        /// <param name="indicesCount">The index count.</param>
        /// <param name="box">The pre-computed bounding box.</param>
        /// <param name="sphere">The pre-computed bounding sphere.</param>
        /// <param name="indices">The indices.</param>
        /// <param name="colors">The vertex colors.</param>
        /// <param name="positions">The vertex positions.</param>
        /// <param name="uvChannels">The vertex UV coords.</param>
        /// <param name="normals">The vertex normals.</param>
        /// <param name="tangents">The vertex tangents.</param>
        /// <param name="boneWeights">The vertex bone weights.</param>
        public MeshLODData(uint lodLevel, uint verticesCount, uint indicesCount, BoundingBox box, BoundingSphere sphere, uint[] indices, Vector4[] colors, Vector3[] positions, UVChannel[] uvChannels, Vector3[] normals, Vector3[] tangents, BoneWeight[]? boneWeights)
        {
            LODLevel = lodLevel;
            VertexCount = verticesCount;
            IndexCount = indicesCount;
            Box = box;
            Sphere = sphere;
            this.indices = indices;
            this.colors = colors;
            this.positions = positions;
            this.uvChannels = uvChannels;
            this.normals = normals;
            this.tangents = tangents;
            this.boneWeights = boneWeights;
        }

        public unsafe uint GetSize(VertexFlags flags)
        {
            uint staticSize = 4 + 4 + 4 + (uint)sizeof(BoundingBox) + (uint)sizeof(BoundingSphere);
            uint indexStride = GetIndexStride();
            uint vertexStride = GetVertexStride(flags);

            return staticSize + indexStride * IndexCount + vertexStride * VertexCount;
        }

        public static unsafe uint GetIndexStride()
        {
            return sizeof(uint);
        }

        public static unsafe uint GetVertexStride(VertexFlags flags)
        {
            uint vertexStride = 0;
            if ((flags & VertexFlags.Colors) != 0)
            {
                vertexStride += (uint)sizeof(Vector4);
            }
            if ((flags & VertexFlags.Positions) != 0)
            {
                vertexStride += (uint)sizeof(Vector3);
            }
            if ((flags & VertexFlags.UVs) != 0)
            {
                vertexStride += (uint)sizeof(Vector3);
            }
            if ((flags & VertexFlags.Normals) != 0)
            {
                vertexStride += (uint)sizeof(Vector3);
            }
            if ((flags & VertexFlags.Tangents) != 0)
            {
                vertexStride += (uint)sizeof(Vector3);
            }
            if ((flags & VertexFlags.Skinned) != 0)
            {
                vertexStride += (uint)sizeof(BoneWeight);
            }

            return vertexStride;
        }

        public static MeshLODData Read(Stream stream, VertexFlags flags, Endianness endianness, Version version, UVChannelInfo info)
        {
            MeshLODData data = new();
            data.ReadFrom(stream, flags, endianness, version, info);
            return data;
        }

        public unsafe void ReadFrom(Stream stream, VertexFlags flags, Endianness endianness, Version version, UVChannelInfo info)
        {
            this.flags = flags;
            LODLevel = stream.ReadUInt32(endianness);
            VertexCount = stream.ReadUInt32(endianness);
            IndexCount = stream.ReadUInt32(endianness);
            Box = BoundingBox.Read(stream, endianness);
            Sphere = BoundingSphere.Read(stream, endianness);

            indices = new uint[IndexCount];
            stream.ReadArrayUInt32(indices, endianness);

            if ((flags & VertexFlags.Colors) != 0)
            {
                colors = new Vector4[VertexCount];
                stream.ReadArrayVector4(colors, endianness);
            }
            if ((flags & VertexFlags.Positions) != 0)
            {
                positions = new Vector3[VertexCount];
                stream.ReadArrayVector3(positions, endianness);
            }

            if ((flags & VertexFlags.UVs) != 0)
            {
                if (version <= new Version(1, 0, 0, 0))
                {
                    var uvs = new Vector3[VertexCount];
                    stream.ReadArrayVector3(uvs, endianness);

                    // The importer just expanded the 2D uvs to 3D uvs (This was a design choise mistake), so we compact them again back to uv 2D.
                    var uv2Ds = uvs.Select(x => new Vector2(x.X, x.Y));
                    UVChannel channel = default;
                    channel.SetUVs(uv2Ds.ToArray());
                    uvChannels = new UVChannel[UVChannelInfo.MaxChannels];
                    uvChannels[0] = channel;
                }
                else
                {
                    UVType* pType = (UVType*)&info;
                    uvChannels = new UVChannel[UVChannelInfo.MaxChannels];
                    for (int i = 0; i < UVChannelInfo.MaxChannels; i++)
                    {
                        var type = pType[i];
                        UVChannel uvChannel = new(type, vertexCount);
                        switch (type)
                        {
                            case UVType.Empty:
                                continue;

                            case UVType.UV2D:
                                stream.ReadArrayVector2(uvChannel.GetUV2D(), endianness);
                                break;

                            case UVType.UV3D:
                                stream.ReadArrayVector3(uvChannel.GetUV3D(), endianness);
                                break;

                            case UVType.UV4D:
                                stream.ReadArrayVector4(uvChannel.GetUV4D(), endianness);
                                break;

                            default:
                                throw new NotSupportedException($"Unsupported UV type: {type}");
                        }
                        uvChannels[i] = uvChannel;
                    }
                }
            }

            if ((flags & VertexFlags.Normals) != 0)
            {
                normals = new Vector3[VertexCount];
                stream.ReadArrayVector3(normals, endianness);
            }
            if ((flags & VertexFlags.Tangents) != 0)
            {
                tangents = new Vector3[VertexCount];
                stream.ReadArrayVector3(tangents, endianness);
            }
            if ((flags & VertexFlags.Skinned) != 0)
            {
                BoneWeights = new BoneWeight[VertexCount];
                for (ulong i = 0; i < VertexCount; i++)
                {
                    BoneWeights[i] = BoneWeight.Read(stream, endianness);
                }
            }
        }

        public void Write(Stream stream, VertexFlags flags, Endianness endianness)
        {
            // Write basic information
            stream.WriteUInt32(LODLevel, endianness);
            stream.WriteUInt32(VertexCount, endianness);
            stream.WriteUInt32(IndexCount, endianness);
            Box.Write(stream, endianness);
            Sphere.Write(stream, endianness);

            // Write indices
            stream.WriteArrayUInt32(indices, endianness);

            // Write optional vertex components if present
            if ((flags & VertexFlags.Colors) != 0)
            {
                stream.WriteArrayVector4(colors, endianness);
            }
            if ((flags & VertexFlags.Positions) != 0)
            {
                stream.WriteArrayVector3(positions, endianness);
            }

            if ((flags & VertexFlags.UVs) != 0)
            {
                for (int i = 0; i < UVChannelInfo.MaxChannels; i++)
                {
                    UVChannel uvChannel = uvChannels[i];
                    switch (uvChannel.Type)
                    {
                        case UVType.Empty:
                            continue;

                        case UVType.UV2D:
                            stream.WriteArrayVector2(uvChannel.GetUV2D(), endianness);
                            break;

                        case UVType.UV3D:
                            stream.WriteArrayVector3(uvChannel.GetUV3D(), endianness);
                            break;

                        case UVType.UV4D:
                            stream.WriteArrayVector4(uvChannel.GetUV4D(), endianness);
                            break;

                        default:
                            throw new NotSupportedException($"Unsupported UV type: {uvChannel.Type}");
                    }
                }
            }
            if ((flags & VertexFlags.Normals) != 0)
            {
                stream.WriteArrayVector3(normals, endianness);
            }
            if ((flags & VertexFlags.Tangents) != 0)
            {
                stream.WriteArrayVector3(tangents, endianness);
            }
#nullable disable
            if ((flags & VertexFlags.Skinned) != 0)
            {
                for (ulong i = 0; i < VertexCount; i++)
                {
                    BoneWeights[i].Write(stream, endianness);
                }
            }
#nullable restore
        }

        /// <summary>
        /// Gets an array of faces that include the specified vertex.
        /// </summary>
        /// <param name="vertex">The index of the vertex.</param>
        /// <returns>An array of faces that include the specified vertex.</returns>
        public Face[] GetFacesForVertex(int vertex)
        {
            List<Face> faces = new();
            for (int i = 0; i < IndexCount;)
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
            for (int i = 0; i < IndexCount;)
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
            for (int i = 0; i < IndexCount;)
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
            for (uint i = 0; i < IndexCount;)
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
            for (uint i = 0; i < IndexCount;)
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
            for (uint i = 0; i < IndexCount / 3; i++)
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
            for (uint i = 0; i < IndexCount / 3; i++)
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
        /// Gets an array of faces representing the triangles in the mesh.
        /// </summary>
        /// <returns>An array of faces.</returns>
        public Face[] GetFaces()
        {
            Face[] faces = new Face[IndexCount / 3];
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
            uint facesCount = IndexCount / 3;
            faces.Clear();
            faces.Capacity = (int)facesCount;
            for (int i = 0; i < facesCount; i++)
            {
                faces.Add(new(Indices[i * 3], Indices[i * 3 + 1], Indices[i * 3 + 2]));
            }
        }

        public IIndexBuffer CreateIndexBuffer(CpuAccessFlags accessFlags)
        {
            if (IndexCount > ushort.MaxValue)
            {
                return new IndexBuffer<uint>(Indices, accessFlags);
            }
            else
            {
                ushort[] indices = new ushort[IndexCount];
                for (int i = 0; i < IndexCount; i++)
                {
                    indices[i] = (ushort)Indices[i];
                }

                return new IndexBuffer<ushort>(indices, accessFlags);
            }
        }

        private unsafe int ComputeStride()
        {
            int stride = 0;
            if ((flags & VertexFlags.Colors) != 0)
            {
                stride += sizeof(Vector4);
            }
            if ((flags & VertexFlags.Positions) != 0)
            {
                stride += sizeof(Vector3);
            }
            if ((flags & VertexFlags.UVs) != 0)
            {
                for (int i = 0; i < uvChannels.Length; i++)
                {
                    switch (uvChannels[i].Type)
                    {
                        case UVType.UV2D:
                            stride += sizeof(Vector2);
                            break;

                        case UVType.UV3D:
                            stride += sizeof(Vector3);
                            break;

                        case UVType.UV4D:
                            stride += sizeof(Vector4);
                            break;
                    }
                }
            }
            if ((flags & VertexFlags.Normals) != 0)
            {
                stride += sizeof(Vector3);
            }
            if ((flags & VertexFlags.Tangents) != 0)
            {
                stride += sizeof(Vector3);
            }
            if ((flags & VertexFlags.Skinned) != 0)
            {
                stride += sizeof(Point4) + sizeof(Vector4);
            }

            return stride;
        }

        private static unsafe void Append<T>(void** data, T item) where T : unmanaged
        {
            T* p = (T*)*data;
            *p = item;
            p++;
            *data = p;
        }

        public unsafe IVertexBuffer CreateVertexBuffer(CpuAccessFlags accessFlags = CpuAccessFlags.None)
        {
            var stride = ComputeStride();
            var size = stride * (int)VertexCount;
            var vertices = (byte*)Alloc(size);
            ZeroMemory(vertices, size);

            WriteVertexData(stride, vertices);

            VertexBuffer vertexBuffer = new(vertices, stride, VertexCount, transferOwnership: true, accessFlags);
            return vertexBuffer;
        }

        private unsafe void WriteVertexData(int stride, byte* vertices)
        {
            for (int i = 0; i < VertexCount; i++)
            {
                void* vertexPointer = vertices + i * stride; // Pointer to the start of each vertex

                if ((flags & VertexFlags.Colors) != 0)
                {
                    Append(&vertexPointer, colors[i]);
                }

                if ((flags & VertexFlags.Positions) != 0)
                {
                    Append(&vertexPointer, positions[i]);
                }

                if ((flags & VertexFlags.UVs) != 0)
                {
                    for (int j = 0; j < uvChannels.Length; j++)
                    {
                        var uvChannel = uvChannels[j];
                        switch (uvChannel.Type)
                        {
                            case UVType.UV2D:
                                Append(&vertexPointer, uvChannel.GetUV2D()[i]);
                                break;

                            case UVType.UV3D:
                                Append(&vertexPointer, uvChannel.GetUV3D()[i]);
                                break;

                            case UVType.UV4D:
                                Append(&vertexPointer, uvChannel.GetUV4D()[i]);
                                break;
                        }
                    }
                }

                if ((flags & VertexFlags.Normals) != 0)
                {
                    Append(&vertexPointer, normals[i]);
                }

                if ((flags & VertexFlags.Tangents) != 0)
                {
                    Append(&vertexPointer, tangents[i]);
                }

                if ((flags & VertexFlags.Skinned) != 0)
                {
                    Append(&vertexPointer, boneWeights![i]);
                }
            }
        }

        public bool WriteIndexBuffer(IGraphicsContext context, IIndexBuffer ib)
        {
            if (IndexCount > ushort.MaxValue && ib.Format == IndexFormat.UInt16)
            {
                return false;
            }
            if (ib is IIndexBuffer<ushort> indexBufferU16)
            {
                for (uint i = 0; i < IndexCount; i++)
                {
                    indexBufferU16[i] = (ushort)Indices[i];
                }
                return indexBufferU16.Update(context);
            }
            else if (ib is IIndexBuffer<uint> indexBufferU32)
            {
                for (uint i = 0; i < IndexCount; i++)
                {
                    indexBufferU32[i] = Indices[i];
                }
                return indexBufferU32.Update(context);
            }
            return false;
        }

        public unsafe bool WriteVertexBuffer(IGraphicsContext context, IVertexBuffer vb)
        {
            var stride = ComputeStride();

            if (vb.Stride != stride)
            {
                throw new InvalidOperationException($"Buffer must match vertex stride. Is {vb.Stride} but should be {stride}");
            }

            if (vb.Count < VertexCount)
            {
                throw new InvalidOperationException("Buffer too small");
            }

            WriteVertexData(stride, (byte*)vb.Items);

            return vb.Update(context);
        }

        public IEnumerable<UVChannel> GetUVChannels()
        {
            for (int i = 0; i < UVChannelInfo.MaxChannels; i++)
            {
                var channel = uvChannels[i];
                if (channel.Type == UVType.Empty)
                {
                    continue;
                }
                yield return channel;
            }
        }
    }
}