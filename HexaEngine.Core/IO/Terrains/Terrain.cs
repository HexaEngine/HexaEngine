namespace HexaEngine.Core.IO.Terrains
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO.Meshes.Processing;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Mathematics;
    using System.Numerics;

    /// <summary>
    /// Represents a terrain mesh generated from a height map.
    /// </summary>
    public unsafe class Terrain
    {
        /// <summary>
        /// The number of vertices in the terrain.
        /// </summary>
        public uint VerticesCount;

        /// <summary>
        /// The number of indices in the terrain.
        /// </summary>
        public uint IndicesCount;

        /// <summary>
        /// The width of the terrain.
        /// </summary>
        public uint Width;

        /// <summary>
        /// The height of the terrain.
        /// </summary>
        public uint Height;

        /// <summary>
        /// The array of indices that define the terrain's triangles.
        /// </summary>
        public uint[] Indices;

        /// <summary>
        /// The array of vertex positions.
        /// </summary>
        public Vector3[] Positions;

        /// <summary>
        /// The array of vertex UV coordinates.
        /// </summary>
        public Vector3[] UVs;

        /// <summary>
        /// The array of vertex normals.
        /// </summary>
        public Vector3[] Normals;

        /// <summary>
        /// The array of vertex tangents.
        /// </summary>
        public Vector3[] Tangents;

        /// <summary>
        /// The array of vertex bitangents.
        /// </summary>
        public Vector3[] Bitangents;

        /// <summary>
        /// The bounding box of the terrain.
        /// </summary>
        public BoundingBox Box;

        /// <summary>
        /// The flags indicating which data is present in the terrain.
        /// </summary>
        public TerrainFlags Flags;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terrain"/> class from a height map.
        /// </summary>
        /// <param name="map">The height map.</param>
        public Terrain(HeightMap map)
        {
            Width = (uint)map.Width;
            Height = (uint)map.Height;
            int cols = map.Width;
            int rows = map.Height;

            // Create the grid
            VerticesCount = (uint)(rows * cols);
            IndicesCount = (uint)((rows - 1) * (cols - 1) * 2) * 3;

            Indices = new uint[IndicesCount];
            Positions = new Vector3[VerticesCount];
            UVs = new Vector3[VerticesCount];
            Normals = new Vector3[VerticesCount];
            Tangents = new Vector3[VerticesCount];
            Bitangents = new Vector3[VerticesCount];

            int k = 0;
            int texUIndex = 0;
            int texVIndex = 0;

            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    Positions[i * cols + j] = map[i * cols + j];
                }
            }

            Flags |= TerrainFlags.Positions;

            Box = BoundingBoxHelper.Compute(Positions);

            for (uint i = 0; i < rows - 1; i++)
            {
                for (uint j = 0; j < cols - 1; j++)
                {
                    // Bottom left of quad
                    Indices[k] = (uint)(i * cols + j);
                    UVs[i * cols + j] = new Vector3(texUIndex + 0.0f, texVIndex + 1.0f, 0);

                    // Top left of quad
                    Indices[k + 1] = (uint)((i + 1) * cols + j);
                    UVs[(i + 1) * cols + j] = new Vector3(texUIndex + 0.0f, texVIndex + 0.0f, 0);

                    // Bottom right of quad
                    Indices[k + 2] = (uint)(i * cols + j + 1);
                    UVs[i * cols + j + 1] = new Vector3(texUIndex + 1.0f, texVIndex + 1.0f, 0);

                    // Top left of quad
                    Indices[k + 3] = (uint)((i + 1) * cols + j);
                    UVs[(i + 1) * cols + j] = new Vector3(texUIndex + 0.0f, texVIndex + 0.0f, 0);

                    // Top right of quad
                    Indices[k + 4] = (uint)((i + 1) * cols + j + 1);
                    UVs[(i + 1) * cols + j + 1] = new Vector3(texUIndex + 1.0f, texVIndex + 0.0f, 0);

                    // Bottom right of quad
                    Indices[k + 5] = (uint)(i * cols + j + 1);
                    UVs[i * cols + j + 1] = new Vector3(texUIndex + 1.0f, texVIndex + 1.0f, 0);

                    k += 6; // next quad

                    texUIndex++;
                }
                texUIndex = 0;
                texVIndex++;
            }

            Flags |= TerrainFlags.UVs;

            GenVertexNormalsProcess.GenMeshVertexNormals2(this);

            Flags |= TerrainFlags.Normals;
            CalcTangentsProcess.ProcessMesh2(this);

            Flags |= TerrainFlags.Tangents | TerrainFlags.Bitangents;
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

                if ((Flags & TerrainFlags.Positions) != 0)
                {
                    vertex.Position = Positions[i];
                }

                if ((Flags & TerrainFlags.UVs) != 0)
                {
                    vertex.UV = UVs[i];
                }

                if ((Flags & TerrainFlags.Normals) != 0)
                {
                    vertex.Normal = Normals[i];
                }

                if ((Flags & TerrainFlags.Tangents) != 0)
                {
                    vertex.Tangent = Tangents[i];
                }

                if ((Flags & TerrainFlags.Bitangents) != 0)
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

                if ((Flags & TerrainFlags.Positions) != 0)
                {
                    vertex.Position = Positions[i];
                }

                if ((Flags & TerrainFlags.UVs) != 0)
                {
                    vertex.UV = UVs[i];
                }

                if ((Flags & TerrainFlags.Normals) != 0)
                {
                    vertex.Normal = Normals[i];
                }

                if ((Flags & TerrainFlags.Tangents) != 0)
                {
                    vertex.Tangent = Tangents[i];
                }

                if ((Flags & TerrainFlags.Bitangents) != 0)
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
        public void AverageEdge(Edge edge, Terrain other)
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