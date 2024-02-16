namespace HexaEngine.Core.IO.Binary.Terrains
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Meshes;
    using HexaEngine.Core.IO.Binary.Meshes.Processing;
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Numerics;
    using System.Text;

    /// <summary>
    /// Represents a terrain mesh generated from a height heightMap.
    /// </summary>
    public unsafe class TerrainCellData
    {
        /// <summary>
        /// Gets or sets the name of the terrain cell.
        /// </summary>
        public string Name;

        /// <summary>
        /// Gets or sets an array of material names associated with the terrain cell.
        /// </summary>
        public List<string> Materials;

        /// <summary>
        /// Gets or sets the number of vertices in the terrain cell.
        /// </summary>
        public uint VerticesCount;

        /// <summary>
        /// Gets or sets the number of indices in the terrain cell.
        /// </summary>
        public uint IndicesCount;

        /// <summary>
        /// Gets or sets the width of the terrain cell.
        /// </summary>
        private uint width;

        /// <summary>
        /// Gets or sets the height of the terrain cell.
        /// </summary>
        private uint height;

        /// <summary>
        /// Gets or sets the level of detail for the terrain cell.
        /// </summary>
        private uint lodLevel;

        /// <summary>
        /// Gets or sets the bounding box of the terrain cell.
        /// </summary>
        public BoundingBox Box;

        /// <summary>
        /// Gets or sets the bounding sphere of the terrain cell.
        /// </summary>
        public BoundingSphere Sphere;

        public uint ActualHeight { get; }

        public uint ActualWidth { get; }

        /// <summary>
        /// Gets or sets the flags associated with the terrain cell vertices.
        /// </summary>
        public TerrainVertexFlags Flags;

        /// <summary>
        /// Gets or sets an array of indices for the terrain cell.
        /// </summary>
        public uint[] Indices;

        /// <summary>
        /// Gets or sets an array of positions for the terrain cell vertices.
        /// </summary>
        public Vector3[] Positions;

        /// <summary>
        /// Gets or sets an array of UV coordinates for the terrain cell vertices.
        /// </summary>
        public Vector3[] UVs;

        /// <summary>
        /// Gets or sets an array of normals for the terrain cell vertices.
        /// </summary>
        public Vector3[] Normals;

        /// <summary>
        /// Gets or sets an array of tangents for the terrain cell vertices.
        /// </summary>
        public Vector3[] Tangents;

#nullable disable

        private TerrainCellData()
        {
        }

#nullable restore

        /// <summary>
        /// Initializes a new instance of the <see cref="TerrainCellData"/> class based on the provided <see cref="HeightMap"/>.
        /// </summary>
        /// <param name="heightMap">The height map used for terrain generation.</param>
        /// <param name="width">The width of the terrain.</param>
        /// <param name="height">The height of the terrain.</param>
        /// <param name="tessellationFactor">The factor by which to tessellate the terrain (default is 1).</param>
        /// <param name="lodLevel">The level of detail for the terrain (default is 0).</param>
        public TerrainCellData(HeightMap heightMap, uint width, uint height, float tessellationFactor = 5, uint lodLevel = 0)
        {
            Name = string.Empty;
            Materials = [];
            this.lodLevel = lodLevel;
            this.width = width;
            this.height = height;

            uint actualWidth = (uint)(width * tessellationFactor);
            uint actualHeight = (uint)(height * tessellationFactor);

            // Create the grid
            VerticesCount = actualHeight * actualWidth;
            IndicesCount = (actualHeight - 1) * (actualWidth - 1) * 2 * 3;

            Indices = new uint[IndicesCount];
            Positions = new Vector3[VerticesCount];
            UVs = new Vector3[VerticesCount];
            Normals = new Vector3[VerticesCount];
            Tangents = new Vector3[VerticesCount];

            int k = 0;
            int texUIndex = 0;
            int texVIndex = 0;

            float scaleFactorW = width / heightMap.Width / tessellationFactor;
            float scaleFactorH = height / heightMap.Height / tessellationFactor;

            float targetScaleFactor = 1 / tessellationFactor;

            for (uint targetX = 0; targetX < actualHeight; ++targetX)
            {
                for (uint targetY = 0; targetY < actualWidth; ++targetY)
                {
                    float sum = 0;

                    for (int subX = 0; subX < scaleFactorW; ++subX)
                    {
                        for (int subY = 0; subY < scaleFactorH; ++subY)
                        {
                            uint sourceX = (uint)(targetX * scaleFactorW + subX);
                            uint sourceY = (uint)(targetY * scaleFactorH + subY);
                            sum += heightMap[sourceX, sourceY];
                        }
                    }

                    float averageHeight = sum / (scaleFactorW * scaleFactorH);

                    Positions[targetX * actualWidth + targetY] = new(targetY * targetScaleFactor, averageHeight, targetX * targetScaleFactor);
                }
            }

            Flags |= TerrainVertexFlags.Positions;

            Box = BoundingBoxHelper.Compute(Positions);
            Sphere = BoundingSphere.CreateFromBoundingBox(Box);

            ActualHeight = actualHeight;
            ActualWidth = actualWidth;

            Vector3 uvScale = new Vector3(scaleFactorW, scaleFactorH, 0) * 0.1f;

            for (uint i = 0; i < actualHeight - 1; i++)
            {
                for (uint j = 0; j < actualWidth - 1; j++)
                {
                    // Bottom left of quad
                    Indices[k] = i * actualWidth + j;
                    UVs[i * actualWidth + j] = new Vector3(texUIndex, (texVIndex + 1.0f), 0) * uvScale;

                    // Top left of quad
                    Indices[k + 1] = (i + 1) * actualWidth + j;
                    UVs[(i + 1) * actualWidth + j] = new Vector3(texUIndex, texVIndex, 0) * uvScale;

                    // Bottom right of quad
                    Indices[k + 2] = i * actualWidth + j + 1;
                    UVs[i * actualWidth + j + 1] = new Vector3((texUIndex + 1.0f), (texVIndex + 1.0f), 0) * uvScale;

                    // Top left of quad
                    Indices[k + 3] = (i + 1) * actualWidth + j;
                    UVs[(i + 1) * actualWidth + j] = new Vector3(texUIndex, texVIndex, 0) * uvScale;

                    // Top right of quad
                    Indices[k + 4] = (i + 1) * actualWidth + j + 1;
                    UVs[(i + 1) * actualWidth + j + 1] = new Vector3((texUIndex + 1.0f), texVIndex, 0) * uvScale;

                    // Bottom right of quad
                    Indices[k + 5] = i * actualWidth + j + 1;
                    UVs[i * actualWidth + j + 1] = new Vector3((texUIndex + 1.0f), (texVIndex + 1.0f), 0) * uvScale;

                    k += 6; // next quad

                    texUIndex++;
                }
                texUIndex = 0;
                texVIndex++;
            }

            Flags |= TerrainVertexFlags.UVs;

            GenVertexNormalsProcess.GenMeshVertexNormals2(this);

            Flags |= TerrainVertexFlags.Normals;
            CalcTangentsProcess.ProcessMesh2Parallel(this);

            Flags |= TerrainVertexFlags.Tangents;
        }

        /// <summary>
        /// Reads terrain cell data from a stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="src">The source stream to read from.</param>
        /// <param name="encoding">The encoding to use for string representation.</param>
        /// <param name="endianness">The endianness to use for reading binary data.</param>
        /// <returns>A <see cref="TerrainCellData"/> instance populated with data from the stream.</returns>
        public static TerrainCellData Read(Stream src, Encoding encoding, Endianness endianness)
        {
            TerrainCellData data = new();
            data.Name = src.ReadString(encoding, endianness) ?? string.Empty;
            var matCount = src.ReadInt32(endianness);
            data.Materials = new List<string>(matCount);
            for (int i = 0; i < matCount; i++)
            {
                data.Materials.Add(src.ReadString(encoding, endianness) ?? string.Empty);
            }
            data.VerticesCount = src.ReadUInt32(endianness);
            data.IndicesCount = src.ReadUInt32(endianness);
            data.width = src.ReadUInt32(endianness);
            data.height = src.ReadUInt32(endianness);
            data.lodLevel = src.ReadUInt32(endianness);
            data.Box = BoundingBox.Read(src, endianness);
            data.Sphere = BoundingSphere.Read(src, endianness);
            data.Flags = (TerrainVertexFlags)src.ReadInt32(endianness);

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

            return data;
        }

        /// <summary>
        /// Writes the terrain cell data to a stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="dst">The destination stream to write to.</param>
        /// <param name="encoding">The encoding to use for string representation.</param>
        /// <param name="endianness">The endianness to use for writing binary data.</param>
        public void Write(Stream dst, Encoding encoding, Endianness endianness)
        {
            dst.WriteString(Name, encoding, endianness);
            dst.WriteInt32(Materials.Count, endianness);
            for (int i = 0; i < Materials.Count; i++)
            {
                dst.WriteString(Materials[i], encoding, endianness);
            }
            dst.WriteUInt32(IndicesCount, endianness);
            dst.WriteUInt32(VerticesCount, endianness);
            dst.WriteUInt32(lodLevel, endianness);
            Box.Write(dst, endianness);
            Sphere.Write(dst, endianness);
            dst.WriteInt32((int)Flags, endianness);

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
        }

        /// <summary>
        /// Creates an index buffer for the terrain.
        /// </summary>
        /// <param filename="device">The graphics device.</param>
        /// <param filename="accessFlags">The CPU access flags.</param>
        /// <returns>The index buffer.</returns>
        public IndexBuffer<uint> CreateIndexBuffer(IGraphicsDevice device, CpuAccessFlags accessFlags = CpuAccessFlags.None)
        {
            return new IndexBuffer<uint>(device, Indices, accessFlags);
        }

        /// <summary>
        /// Writes the terrain indices to an existing index buffer.
        /// </summary>
        /// <param filename="context">The graphics context.</param>
        /// <param filename="ib">The index buffer to write to.</param>
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
        /// <param filename="device">The graphics device.</param>
        /// <param filename="accessFlags">The CPU access flags.</param>
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

                vertices[i] = vertex;
            }

            VertexBuffer<TerrainVertex> vertexBuffer = new VertexBuffer<TerrainVertex>(device, vertices, VerticesCount, accessFlags);
            Free(vertices);
            return vertexBuffer;
        }

        /// <summary>
        /// Writes the terrain vertices to an existing vertex buffer.
        /// </summary>
        /// <param filename="context">The graphics context.</param>
        /// <param filename="vb">The vertex buffer to write to.</param>
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
        /// <param filename="ray">The ray to test.</param>
        /// <param filename="pointInTerrain">The point of intersection in terrain coordinates, if the ray intersects.</param>
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

                if (ray.Intersects(pos0, pos1, pos2, out pointInTerrain))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Performs a ray intersection test with the transformed terrain.
        /// </summary>
        /// <param filename="ray">The ray to test.</param>
        /// <param filename="transform">The transformation matrix to apply to the terrain.</param>
        /// <param filename="pointInTerrain">The point of intersection in terrain coordinates, if the ray intersects.</param>
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

                if (ray.Intersects(Vector3.Transform(pos0, transform), Vector3.Transform(pos1, transform), Vector3.Transform(pos2, transform), out pointInTerrain))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the index for the specified coordinates in the terrain.
        /// </summary>
        /// <param filename="x">The x-coordinate.</param>
        /// <param filename="y">The y-coordinate.</param>
        /// <returns>The index.</returns>
        public uint GetIndexFor(uint x, uint y)
        {
            return y * width + x;
        }

        /// <summary>
        /// Gets the index for the specified coordinates in the terrain.
        /// </summary>
        /// <param filename="x">The x-coordinate.</param>
        /// <param filename="y">The y-coordinate.</param>
        /// <returns>The index.</returns>
        public int GetIndexFor(int x, int y)
        {
            return (int)(x * width + y);
        }

        /// <summary>
        /// Averages the edge of the terrain with the corresponding edge of another terrain.
        /// </summary>
        /// <param filename="edge">The edge to average.</param>
        /// <param filename="other">The other terrain.</param>
        public void AverageEdge(Edge edge, TerrainCellData other)
        {
            if (edge == Edge.ZPos)
            {
                for (uint i = 0; i < width; i++)
                {
                    var indexA = GetIndexFor(i, height - 1);
                    var indexB = GetIndexFor(i, 0);

                    Normals[indexA] = other.Normals[indexB] = Vector3.Normalize(Normals[indexA] + other.Normals[indexB]);
                    Tangents[indexA] = other.Tangents[indexB] = Vector3.Normalize(Tangents[indexA] + other.Tangents[indexB]);
                }
            }
            if (edge == Edge.ZNeg)
            {
                for (uint i = 0; i < width; i++)
                {
                    var indexA = GetIndexFor(i, 0);
                    var indexB = GetIndexFor(i, height - 1);

                    Normals[indexA] = other.Normals[indexB] = Vector3.Normalize(Normals[indexA] + other.Normals[indexB]);
                    Tangents[indexA] = other.Tangents[indexB] = Vector3.Normalize(Tangents[indexA] + other.Tangents[indexB]);
                }
            }
            if (edge == Edge.XPos)
            {
                for (uint i = 0; i < height; i++)
                {
                    var indexA = GetIndexFor(width - 1, i);
                    var indexB = GetIndexFor(0, i);

                    Normals[indexA] = other.Normals[indexB] = Vector3.Normalize(Normals[indexA] + other.Normals[indexB]);
                    Tangents[indexA] = other.Tangents[indexB] = Vector3.Normalize(Tangents[indexA] + other.Tangents[indexB]);
                }
            }
            if (edge == Edge.XNeg)
            {
                for (uint i = 0; i < height; i++)
                {
                    var indexA = GetIndexFor(0, i);
                    var indexB = GetIndexFor(width - 1, i);

                    Normals[indexA] = other.Normals[indexB] = Vector3.Normalize(Normals[indexA] + other.Normals[indexB]);
                    Tangents[indexA] = other.Tangents[indexB] = Vector3.Normalize(Tangents[indexA] + other.Tangents[indexB]);
                }
            }
        }

        public Face GetFaceAtIndex(uint index)
        {
            var i = index * 3;
            var idx1 = Indices[i];
            var idx2 = Indices[i + 1];
            var idx3 = Indices[i + 2];

            return new(idx1, idx2, idx3);
        }
    }
}