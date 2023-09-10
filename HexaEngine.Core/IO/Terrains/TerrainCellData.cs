﻿namespace HexaEngine.Core.IO.Terrains
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO.Meshes.Processing;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Numerics;
    using System.Text;

    /// <summary>
    /// Represents a terrain mesh generated from a height map.
    /// </summary>
    public unsafe class TerrainCellData
    {
        public string Name;
        public string[] Materials;
        public uint VerticesCount;
        public uint IndicesCount;
        public uint LODLevel;
        public BoundingBox Box;
        public BoundingSphere Sphere;
        public TerrainVertexFlags Flags;
        public HeightMap HeightMap;
        public uint[] Indices;
        public Vector3[] Positions;
        public Vector3[] UVs;
        public Vector3[] Normals;
        public Vector3[] Tangents;
        public Vector3[] Bitangents;

        private TerrainCellData()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TerrainCellData"/> class from a height map.
        /// </summary>
        /// <param name="map">The height map.</param>
        public TerrainCellData(HeightMap map, uint lodLevel = 0)
        {
            HeightMap = map;
            LODLevel = lodLevel;
            uint cols = map.Width;
            uint rows = map.Height;

            // Create the grid
            VerticesCount = rows * cols;
            IndicesCount = (rows - 1) * (cols - 1) * 2 * 3;

            Indices = new uint[IndicesCount];
            Positions = new Vector3[VerticesCount];
            UVs = new Vector3[VerticesCount];
            Normals = new Vector3[VerticesCount];
            Tangents = new Vector3[VerticesCount];
            Bitangents = new Vector3[VerticesCount];

            int k = 0;
            int texUIndex = 0;
            int texVIndex = 0;

            for (uint i = 0; i < rows; ++i)
            {
                for (uint j = 0; j < cols; ++j)
                {
                    Positions[i * cols + j] = map[i * cols + j];
                }
            }

            Flags |= TerrainVertexFlags.Positions;

            Box = BoundingBoxHelper.Compute(Positions);
            Sphere = BoundingSphere.CreateFromBoundingBox(Box);

            for (uint i = 0; i < rows - 1; i++)
            {
                for (uint j = 0; j < cols - 1; j++)
                {
                    // Bottom left of quad
                    Indices[k] = i * cols + j;
                    UVs[i * cols + j] = new Vector3(texUIndex + 0.0f, texVIndex + 1.0f, 0);

                    // Top left of quad
                    Indices[k + 1] = (i + 1) * cols + j;
                    UVs[(i + 1) * cols + j] = new Vector3(texUIndex + 0.0f, texVIndex + 0.0f, 0);

                    // Bottom right of quad
                    Indices[k + 2] = i * cols + j + 1;
                    UVs[i * cols + j + 1] = new Vector3(texUIndex + 1.0f, texVIndex + 1.0f, 0);

                    // Top left of quad
                    Indices[k + 3] = (i + 1) * cols + j;
                    UVs[(i + 1) * cols + j] = new Vector3(texUIndex + 0.0f, texVIndex + 0.0f, 0);

                    // Top right of quad
                    Indices[k + 4] = (i + 1) * cols + j + 1;
                    UVs[(i + 1) * cols + j + 1] = new Vector3(texUIndex + 1.0f, texVIndex + 0.0f, 0);

                    // Bottom right of quad
                    Indices[k + 5] = i * cols + j + 1;
                    UVs[i * cols + j + 1] = new Vector3(texUIndex + 1.0f, texVIndex + 1.0f, 0);

                    k += 6; // next quad

                    texUIndex++;
                }
                texUIndex = 0;
                texVIndex++;
            }

            Flags |= TerrainVertexFlags.UVs;

            GenVertexNormalsProcess.GenMeshVertexNormals2(this);

            Flags |= TerrainVertexFlags.Normals;
            CalcTangentsProcess.ProcessMesh2(this);

            Flags |= TerrainVertexFlags.Tangents | TerrainVertexFlags.Bitangents;

            var tesselationLevel = 4 - lodLevel;

            for (int i = 0; i < tesselationLevel; i++)
            {
                TessellatorProcess.Tessellate(this);
            }
        }

        public uint Width => HeightMap.Width;

        public uint Height => HeightMap.Height;

        public static TerrainCellData Read(Stream src, Encoding encoding, Endianness endianness)
        {
            TerrainCellData data = new();
            data.Name = src.ReadString(encoding, endianness);
            data.Materials = new string[src.ReadInt32(endianness)];
            for (int i = 0; i < data.Materials.Length; i++)
            {
                data.Materials[i] = src.ReadString(encoding, endianness);
            }
            data.VerticesCount = src.ReadUInt32(endianness);
            data.IndicesCount = src.ReadUInt32(endianness);
            data.LODLevel = src.ReadUInt32(endianness);
            data.Box = BoundingBox.Read(src, endianness);
            data.Sphere = BoundingSphere.Read(src, endianness);
            data.Flags = (TerrainVertexFlags)src.ReadInt32(endianness);

            data.HeightMap = HeightMap.Read(src, encoding, endianness);

            data.Indices = new uint[data.IndicesCount];
            for (ulong i = 0; i < data.IndicesCount; i++)
            {
                data.Indices[i] = src.ReadUInt32(endianness);
            }

            if ((data.Flags & TerrainVertexFlags.Positions) != 0)
            {
                data.Positions = new Vector3[data.VerticesCount];
                for (ulong i = 0; i < data.VerticesCount; i++)
                {
                    data.Positions[i] = src.ReadVector3(endianness);
                }
            }
            if ((data.Flags & TerrainVertexFlags.UVs) != 0)
            {
                data.UVs = new Vector3[data.VerticesCount];
                for (ulong i = 0; i < data.VerticesCount; i++)
                {
                    data.UVs[i] = src.ReadVector3(endianness);
                }
            }
            if ((data.Flags & TerrainVertexFlags.Normals) != 0)
            {
                data.Normals = new Vector3[data.VerticesCount];
                for (ulong i = 0; i < data.VerticesCount; i++)
                {
                    data.Normals[i] = src.ReadVector3(endianness);
                }
            }
            if ((data.Flags & TerrainVertexFlags.Tangents) != 0)
            {
                data.Tangents = new Vector3[data.VerticesCount];
                for (ulong i = 0; i < data.VerticesCount; i++)
                {
                    data.Tangents[i] = src.ReadVector3(endianness);
                }
            }
            if ((data.Flags & TerrainVertexFlags.Bitangents) != 0)
            {
                data.Bitangents = new Vector3[data.VerticesCount];
                for (ulong i = 0; i < data.VerticesCount; i++)
                {
                    data.Bitangents[i] = src.ReadVector3(endianness);
                }
            }

            return data;
        }

        public void Write(Stream dst, Encoding encoding, Endianness endianness)
        {
            dst.WriteString(Name, encoding, endianness);
            dst.WriteInt32(Materials.Length, endianness);
            for (int i = 0; i < Materials.Length; i++)
            {
                dst.WriteString(Materials[i], encoding, endianness);
            }
            dst.WriteUInt32(IndicesCount, endianness);
            dst.WriteUInt32(VerticesCount, endianness);
            dst.WriteUInt32(LODLevel, endianness);
            Box.Write(dst, endianness);
            Sphere.Write(dst, endianness);
            dst.WriteInt32((int)Flags, endianness);

            HeightMap.Write(dst, encoding, endianness);

            for (ulong i = 0; i < IndicesCount; i++)
            {
                dst.WriteUInt32(Indices[i], endianness);
            }

            if ((Flags & TerrainVertexFlags.Positions) != 0)
            {
                for (ulong i = 0; i < VerticesCount; i++)
                {
                    dst.WriteVector3(Positions[i], endianness);
                }
            }
            if ((Flags & TerrainVertexFlags.UVs) != 0)
            {
                for (ulong i = 0; i < VerticesCount; i++)
                {
                    dst.WriteVector3(UVs[i], endianness);
                }
            }
            if ((Flags & TerrainVertexFlags.Normals) != 0)
            {
                for (ulong i = 0; i < VerticesCount; i++)
                {
                    dst.WriteVector3(Normals[i], endianness);
                }
            }
            if ((Flags & TerrainVertexFlags.Tangents) != 0)
            {
                for (ulong i = 0; i < VerticesCount; i++)
                {
                    dst.WriteVector3(Tangents[i], endianness);
                }
            }
            if ((Flags & TerrainVertexFlags.Bitangents) != 0)
            {
                for (ulong i = 0; i < VerticesCount; i++)
                {
                    dst.WriteVector3(Bitangents[i], endianness);
                }
            }
        }

        /// <summary>
        /// Creates an index buffer for the terrain.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="accessFlags">The CPU access flags.</param>
        /// <returns>The index buffer.</returns>
        public IndexBuffer<uint> CreateIndexBuffer(IGraphicsDevice device, CpuAccessFlags accessFlags = CpuAccessFlags.None)
        {
            return new IndexBuffer<uint>(device, Indices, accessFlags);
        }

        /// <summary>
        /// Writes the terrain indices to an existing index buffer.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        /// <param name="ib">The index buffer to write to.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        public bool WriteIndexBuffer(IGraphicsContext context, IndexBuffer<uint> ib)
        {
            for (int i = 0; i < IndicesCount; i++)
            {
                ib[i] = Indices[i];
            }

            return ib.Update(context);
        }

        /// <summary>
        /// Creates a vertex buffer for the terrain.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="accessFlags">The CPU access flags.</param>
        /// <returns>The vertex buffer.</returns>
        public VertexBuffer<TerrainVertex> CreateVertexBuffer(IGraphicsDevice device, CpuAccessFlags accessFlags = CpuAccessFlags.None)
        {
            var stride = sizeof(TerrainVertex);
            var size = stride * (int)VerticesCount;
            var vertices = (TerrainVertex*)Alloc(size);
            ZeroMemory(vertices, size);

            for (int i = 0; i < VerticesCount; i++)
            {
                TerrainVertex vertex = default;

                if ((Flags & TerrainVertexFlags.Positions) != 0)
                {
                    vertex.Position = Positions[i];
                }

                if ((Flags & TerrainVertexFlags.UVs) != 0)
                {
                    vertex.UV = UVs[i];
                }

                if ((Flags & TerrainVertexFlags.Normals) != 0)
                {
                    vertex.Normal = Normals[i];
                }

                if ((Flags & TerrainVertexFlags.Tangents) != 0)
                {
                    vertex.Tangent = Tangents[i];
                }

                if ((Flags & TerrainVertexFlags.Bitangents) != 0)
                {
                    vertex.Bitangent = Bitangents[i];
                }

                vertices[i] = vertex;
            }

            VertexBuffer<TerrainVertex> vertexBuffer = new VertexBuffer<TerrainVertex>(device, vertices, VerticesCount, accessFlags);
            Free(vertices);
            return vertexBuffer;
        }

        /// <summary>
        /// Writes the terrain vertices to an existing vertex buffer.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        /// <param name="vb">The vertex buffer to write to.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        public bool WriteVertexBuffer(IGraphicsContext context, VertexBuffer<TerrainVertex> vb)
        {
            for (int i = 0; i < VerticesCount; i++)
            {
                TerrainVertex vertex = default;

                if ((Flags & TerrainVertexFlags.Positions) != 0)
                {
                    vertex.Position = Positions[i];
                }

                if ((Flags & TerrainVertexFlags.UVs) != 0)
                {
                    vertex.UV = UVs[i];
                }

                if ((Flags & TerrainVertexFlags.Normals) != 0)
                {
                    vertex.Normal = Normals[i];
                }

                if ((Flags & TerrainVertexFlags.Tangents) != 0)
                {
                    vertex.Tangent = Tangents[i];
                }

                if ((Flags & TerrainVertexFlags.Bitangents) != 0)
                {
                    vertex.Bitangent = Bitangents[i];
                }

                vb[i] = vertex;
            }

            return vb.Update(context);
        }

        /// <summary>
        /// The input element descriptions for the terrain vertex format.
        /// </summary>
        public static readonly InputElementDescription[] InputElements =
        {
            new InputElementDescription("POSITION", 0, Format.R32G32B32Float, 0),
            new InputElementDescription("TEXCOORD", 0, Format.R32G32B32Float, 0),
            new InputElementDescription("NORMAL", 0, Format.R32G32B32Float, 0),
            new InputElementDescription("TANGENT", 0, Format.R32G32B32Float, 0),
            new InputElementDescription("BINORMAL", 0, Format.R32G32B32Float, 0),
        };

        /// <summary>
        /// Recalculates the terrain data, including bounding box, vertex normals, tangents, and bitangents.
        /// </summary>
        public void Recalculate()
        {
            Box = BoundingBoxHelper.Compute(Positions);
            GenVertexNormalsProcess.GenMeshVertexNormals2(this);
            CalcTangentsProcess.ProcessMesh2(this);
        }

        /// <summary>
        /// Performs a ray intersection test with the terrain.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="pointInTerrain">The point of intersection in terrain coordinates, if the ray intersects.</param>
        /// <returns>True if the ray intersects the terrain; otherwise, false.</returns>
        public bool IntersectRay(Ray ray, out Vector3 pointInTerrain)
        {
            pointInTerrain = default;
            if (!Box.Intersects(ray).HasValue)
            {
                return false;
            }

            for (uint i = 0; i < IndicesCount / 3; i++)
            {
                Vector3 pos0 = Positions[Indices[i * 3]];
                Vector3 pos1 = Positions[Indices[i * 3 + 1]];
                Vector3 pos2 = Positions[Indices[i * 3 + 2]];

                if (ray.Intersects2(pos0, pos1, pos2, out pointInTerrain))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Performs a ray intersection test with the transformed terrain.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="transform">The transformation matrix to apply to the terrain.</param>
        /// <param name="pointInTerrain">The point of intersection in terrain coordinates, if the ray intersects.</param>
        /// <returns>True if the ray intersects the transformed terrain; otherwise, false.</returns>
        public bool IntersectRay(Ray ray, Matrix4x4 transform, out Vector3 pointInTerrain)
        {
            pointInTerrain = default;

            if (!BoundingBox.Transform(Box, transform).Intersects(ray).HasValue)
            {
                return false;
            }

            for (uint i = 0; i < IndicesCount / 3; i++)
            {
                Vector3 pos0 = Positions[Indices[i * 3]];
                Vector3 pos1 = Positions[Indices[i * 3 + 1]];
                Vector3 pos2 = Positions[Indices[i * 3 + 2]];

                if (ray.Intersects2(Vector3.Transform(pos0, transform), Vector3.Transform(pos1, transform), Vector3.Transform(pos2, transform), out pointInTerrain))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the index for the specified coordinates in the terrain.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>The index.</returns>
        public uint GetIndexFor(uint x, uint y)
        {
            return y * Width + x;
        }

        /// <summary>
        /// Gets the index for the specified coordinates in the terrain.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>The index.</returns>
        public int GetIndexFor(int x, int y)
        {
            return (int)(x * Width + y);
        }

        /// <summary>
        /// Averages the edge of the terrain with the corresponding edge of another terrain.
        /// </summary>
        /// <param name="edge">The edge to average.</param>
        /// <param name="other">The other terrain.</param>
        public void AverageEdge(Edge edge, TerrainCellData other)
        {
            if (edge == Edge.ZPos)
            {
                for (uint i = 0; i < Width; i++)
                {
                    var indexA = GetIndexFor(i, Height - 1);
                    var indexB = GetIndexFor(i, 0);

                    Normals[indexA] = other.Normals[indexB] = Vector3.Normalize(Normals[indexA] + other.Normals[indexB]);
                    Tangents[indexA] = other.Tangents[indexB] = Vector3.Normalize(Tangents[indexA] + other.Tangents[indexB]);
                    Bitangents[indexA] = other.Bitangents[indexB] = Vector3.Normalize(Bitangents[indexA] + other.Bitangents[indexB]);
                }
            }
            if (edge == Edge.ZNeg)
            {
                for (uint i = 0; i < Width; i++)
                {
                    var indexA = GetIndexFor(i, 0);
                    var indexB = GetIndexFor(i, Height - 1);

                    Normals[indexA] = other.Normals[indexB] = Vector3.Normalize(Normals[indexA] + other.Normals[indexB]);
                    Tangents[indexA] = other.Tangents[indexB] = Vector3.Normalize(Tangents[indexA] + other.Tangents[indexB]);
                    Bitangents[indexA] = other.Bitangents[indexB] = Vector3.Normalize(Bitangents[indexA] + other.Bitangents[indexB]);
                }
            }
            if (edge == Edge.XPos)
            {
                for (uint i = 0; i < Height; i++)
                {
                    var indexA = GetIndexFor(Width - 1, i);
                    var indexB = GetIndexFor(0, i);

                    Normals[indexA] = other.Normals[indexB] = Vector3.Normalize(Normals[indexA] + other.Normals[indexB]);
                    Tangents[indexA] = other.Tangents[indexB] = Vector3.Normalize(Tangents[indexA] + other.Tangents[indexB]);
                    Bitangents[indexA] = other.Bitangents[indexB] = Vector3.Normalize(Bitangents[indexA] + other.Bitangents[indexB]);
                }
            }
            if (edge == Edge.XNeg)
            {
                for (uint i = 0; i < Height; i++)
                {
                    var indexA = GetIndexFor(0, i);
                    var indexB = GetIndexFor(Width - 1, i);

                    Normals[indexA] = other.Normals[indexB] = Vector3.Normalize(Normals[indexA] + other.Normals[indexB]);
                    Tangents[indexA] = other.Tangents[indexB] = Vector3.Normalize(Tangents[indexA] + other.Tangents[indexB]);
                    Bitangents[indexA] = other.Bitangents[indexB] = Vector3.Normalize(Bitangents[indexA] + other.Bitangents[indexB]);
                }
            }
        }
    }
}