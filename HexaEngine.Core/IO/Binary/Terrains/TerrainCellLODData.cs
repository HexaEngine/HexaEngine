namespace HexaEngine.Core.IO.Binary.Terrains
{
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Meshes.Processing;
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Numerics;
    using HexaEngine.Core.IO.Binary.Meshes;
    using System.Diagnostics;

    public struct GenerationStats
    {
        public double TimeTotal;
        public double TimePositions;
        public double TimeNormals;
        public double TimeTangents;
    }

    /// <summary>
    /// Represents the data associated with a terrain cell at a specific level of detail (LOD).
    /// </summary>
    public class TerrainCellLODData : ILODData
    {
        private uint lodLevel;
        private uint vertexCount;
        private uint indexCount;
        private uint width;
        private uint height;
        private uint rows;
        private uint columns;
        private BoundingBox box;
        private BoundingSphere sphere;
        private uint[] indices;
        private Vector3[] positions;
        private Vector2[] uvs;
        private Vector3[] normals;
        private Vector3[] tangents;

        /// <summary>
        /// Gets or sets the level of detail for the terrain cell.
        /// </summary>
        public uint LODLevel { get => lodLevel; set => lodLevel = value; }

        /// <summary>
        /// Gets or sets the number of vertices in the terrain cell.
        /// </summary>
        public uint VertexCount { get => vertexCount; set => vertexCount = value; }

        /// <summary>
        /// Gets or sets the number of indices in the terrain cell.
        /// </summary>
        public uint IndexCount { get => indexCount; set => indexCount = value; }

        /// <summary>
        /// Gets the number of faces.
        /// </summary>
        public uint FaceCount => indexCount / 3;

        /// <summary>
        /// Gets or sets the width (units) of the terrain cell.
        /// </summary>
        public uint Width { get => width; set => width = value; }

        /// <summary>
        /// Gets or sets the height (units) of the terrain cell.
        /// </summary>
        public uint Height { get => height; set => height = value; }

        /// <summary>
        /// Gets or sets the rows (vertices) of the terrain cell.
        /// </summary>
        public uint Rows { get => rows; set => rows = value; }

        /// <summary>
        /// Gets or sets the columns (vertices) of the terrain cell.
        /// </summary>
        public uint Columns { get => columns; set => columns = value; }

        /// <summary>
        /// The bounding box of the mesh.
        /// </summary>
        public BoundingBox Box { get => box; set => box = value; }

        /// <summary>
        /// The bounding sphere of the mesh.
        /// </summary>
        public BoundingSphere Sphere { get => sphere; set => sphere = value; }

        /// <summary>
        /// Gets or sets an array of indices for the terrain cell.
        /// </summary>
        public uint[] Indices { get => indices; set => indices = value; }

        /// <summary>
        /// Gets or sets an array of positions for the terrain cell vertices.
        /// </summary>
        public Vector3[] Positions { get => positions; set => positions = value; }

        /// <summary>
        /// Gets or sets an array of UV coordinates for the terrain cell vertices.
        /// </summary>
        public Vector2[] UVs { get => uvs; set => uvs = value; }

        /// <summary>
        /// Gets or sets an array of normals for the terrain cell vertices.
        /// </summary>
        public Vector3[] Normals { get => normals; set => normals = value; }

        /// <summary>
        /// Gets or sets an array of tangents for the terrain cell vertices.
        /// </summary>
        public Vector3[] Tangents { get => tangents; set => tangents = value; }

#nullable disable

        private TerrainCellLODData()
        {
        }

#nullable restore

        /// <summary>
        /// Initializes a new instance of the <see cref="TerrainCellLODData"/> class with specified parameters.
        /// </summary>
        /// <param name="lodLevel">The level of detail for the terrain cell.</param>
        /// <param name="vertexCount">The number of vertices in the terrain cell.</param>
        /// <param name="indexCount">The number of indices in the terrain cell.</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        /// <param name="box">The bounding box of the terrain cell.</param>
        /// <param name="sphere">The bounding sphere of the terrain cell.</param>
        /// <param name="indices">An array of indices for the terrain cell.</param>
        /// <param name="positions">An array of positions for the terrain cell vertices.</param>
        /// <param name="uvs">An array of UV coordinates for the terrain cell vertices.</param>
        /// <param name="normals">An array of normals for the terrain cell vertices.</param>
        /// <param name="tangents">An array of tangents for the terrain cell vertices.</param>
        public TerrainCellLODData(uint lodLevel, uint vertexCount, uint indexCount, uint width, uint height, uint rows, uint columns, BoundingBox box, BoundingSphere sphere, uint[] indices, Vector3[] positions, Vector2[] uvs, Vector3[] normals, Vector3[] tangents)
        {
            this.lodLevel = lodLevel;
            this.vertexCount = vertexCount;
            this.indexCount = indexCount;
            this.width = width;
            this.height = height;
            this.rows = rows;
            this.columns = columns;
            this.box = box;
            this.sphere = sphere;
            this.indices = indices;
            this.positions = positions;
            this.uvs = uvs;
            this.normals = normals;
            this.tangents = tangents;
        }

        public TerrainCellLODData(uint lodLevel, uint width, uint height, HeightMap heightMap, float tessellationFactor = 5)
        {
            this.lodLevel = lodLevel;

            this.width = width;
            this.height = height;

            rows = (uint)(width * tessellationFactor);
            columns = (uint)(height * tessellationFactor);

            // Create the grid
            vertexCount = columns * rows;
            indexCount = (columns - 1) * (rows - 1) * 2 * 3;

            indices = new uint[indexCount];
            positions = new Vector3[vertexCount];
            uvs = new Vector2[vertexCount];
            normals = new Vector3[vertexCount];
            tangents = new Vector3[vertexCount];

            Generate(heightMap);
            GenerateIndicesAndUVs();
        }

        /// <summary>
        /// Reads terrain cell LOD data from a stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="src">The source stream to read from.</param>
        /// <param name="endianness">The endianness to use for reading binary data.</param>
        /// <returns>A <see cref="TerrainCellLODData"/> instance populated with data from the stream.</returns>
        public static TerrainCellLODData Read(Stream src, Endianness endianness)
        {
            uint lodLevel = src.ReadUInt32(endianness);
            uint indexCount = src.ReadUInt32(endianness);
            uint vertexCount = src.ReadUInt32(endianness);
            uint width = src.ReadUInt32(endianness);
            uint height = src.ReadUInt32(endianness);
            uint rows = src.ReadUInt32(endianness);
            uint columns = src.ReadUInt32(endianness);
            BoundingBox box = BoundingBox.Read(src, endianness);
            BoundingSphere sphere = BoundingSphere.Read(src, endianness);

            uint[] indices = new uint[indexCount];
            Vector3[] positions = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            Vector3[] tangents = new Vector3[vertexCount];
            src.ReadArrayUInt32(indices, endianness);
            src.ReadArrayVector3(positions, endianness);
            src.ReadArrayVector2(uvs, endianness);
            src.ReadArrayVector3(normals, endianness);
            src.ReadArrayVector3(tangents, endianness);

            return new(lodLevel, vertexCount, indexCount, width, height, rows, columns, box, sphere, indices, positions, uvs, normals, tangents);
        }

        /// <summary>
        /// Writes the terrain cell data to a stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="dst">The destination stream to write to.</param>
        /// <param name="endianness">The endianness to use for writing binary data.</param>
        public void Write(Stream dst, Endianness endianness)
        {
            dst.WriteUInt32(lodLevel, endianness);
            dst.WriteUInt32(indexCount, endianness);
            dst.WriteUInt32(vertexCount, endianness);
            dst.WriteUInt32(width, endianness);
            dst.WriteUInt32(height, endianness);
            dst.WriteUInt32(rows, endianness);
            dst.WriteUInt32(columns, endianness);
            box.Write(dst, endianness);
            sphere.Write(dst, endianness);

            dst.WriteArrayUInt32(indices, endianness);
            dst.WriteArrayVector3(positions, endianness);
            dst.WriteArrayVector2(uvs, endianness);
            dst.WriteArrayVector3(normals, endianness);
            dst.WriteArrayVector3(tangents, endianness);
        }

        /// <summary>
        /// Recalculates the terrain data, including bounding box, vertex normals, tangents, and bitangents.
        /// </summary>
        public void Recalculate()
        {
            box = BoundingBoxHelper.Compute(positions);
            sphere = BoundingSphere.CreateFromBoundingBox(box);
            GenVertexNormalsProcess.GenMeshVertexNormals2(this);
            CalcTangentsProcess.ProcessMesh2(this);
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
            if (!box.Intersects(ray).HasValue)
            {
                // No intersection with the bounding box, so no intersection with the mesh.
                return null;
            }

            // Initialize a vector representing the minimum intersection point.
            Vector3 minPos = new(float.MaxValue);

            // Iterate through each triangle in the mesh.
            for (uint i = 0; i < indexCount / 3; i++)
            {
                // Get the vertices of the current triangle.
                var pos0 = positions[indices[i * 3]];
                var pos1 = positions[indices[i * 3 + 1]];
                var pos2 = positions[indices[i * 3 + 2]];

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
        /// Performs a ray intersection test with the terrain.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="pointInTerrain">The point of intersection in terrain coordinates, if the ray intersects.</param>
        /// <returns><c>true</c> if the ray intersects the terrain; otherwise, <c>false</c>.</returns>
        public bool IntersectRay(Ray ray, out Vector3 pointInTerrain)
        {
            pointInTerrain = default;
            if (!box.Intersects(ray).HasValue)
            {
                return false;
            }

            for (uint i = 0; i < indexCount / 3; i++)
            {
                Vector3 pos0 = positions[indices[i * 3]];
                Vector3 pos1 = positions[indices[i * 3 + 1]];
                Vector3 pos2 = positions[indices[i * 3 + 2]];

                if (ray.Intersects(pos0, pos1, pos2, out pointInTerrain))
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
            return y * rows + x;
        }

        /// <summary>
        /// Gets the index for the specified coordinates in the terrain.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>The index.</returns>
        public int GetIndexFor(int x, int y)
        {
            return (int)(x * rows + y);
        }

        /// <summary>
        /// Averages the edge of the terrain with the corresponding edge of another terrain.
        /// </summary>
        /// <param name="edge">The edge to average.</param>
        /// <param name="other">The other terrain.</param>
        public void AverageEdge(Edge edge, TerrainCellLODData other)
        {
            if (other.lodLevel != lodLevel || other.rows != rows || other.columns != columns)
            {
                throw new InvalidOperationException($"Terrain cell dimensions must be the same. (LOD: {lodLevel}, Width: {rows}, Height: {columns}), but other was (LOD: {other.lodLevel}, Width: {other.rows}, Height: {other.columns})");
            }

            if (edge == Edge.ZPos)
            {
                for (uint i = 0; i < rows; i++)
                {
                    var indexA = GetIndexFor(i, columns - 1);
                    var indexB = GetIndexFor(i, 0);

                    normals[indexA] = other.normals[indexB] = Vector3.Normalize(normals[indexA] + other.normals[indexB]);
                    tangents[indexA] = other.tangents[indexB] = Vector3.Normalize(tangents[indexA] + other.tangents[indexB]);
                }
            }
            if (edge == Edge.ZNeg)
            {
                for (uint i = 0; i < rows; i++)
                {
                    var indexA = GetIndexFor(i, 0);
                    var indexB = GetIndexFor(i, columns - 1);

                    normals[indexA] = other.normals[indexB] = Vector3.Normalize(normals[indexA] + other.normals[indexB]);
                    tangents[indexA] = other.tangents[indexB] = Vector3.Normalize(tangents[indexA] + other.tangents[indexB]);
                }
            }
            if (edge == Edge.XPos)
            {
                for (uint i = 0; i < columns; i++)
                {
                    var indexA = GetIndexFor(rows - 1, i);
                    var indexB = GetIndexFor(0, i);

                    normals[indexA] = other.normals[indexB] = Vector3.Normalize(normals[indexA] + other.normals[indexB]);
                    tangents[indexA] = other.tangents[indexB] = Vector3.Normalize(tangents[indexA] + other.tangents[indexB]);
                }
            }
            if (edge == Edge.XNeg)
            {
                for (uint i = 0; i < columns; i++)
                {
                    var indexA = GetIndexFor(0, i);
                    var indexB = GetIndexFor(rows - 1, i);

                    normals[indexA] = other.normals[indexB] = Vector3.Normalize(normals[indexA] + other.normals[indexB]);
                    tangents[indexA] = other.tangents[indexB] = Vector3.Normalize(tangents[indexA] + other.tangents[indexB]);
                }
            }
        }

        public void FuseEdge(Edge edge, TerrainCellLODData other)
        {
            if (other.lodLevel != lodLevel || other.rows != rows || other.columns != columns)
            {
                throw new InvalidOperationException($"Terrain cell dimensions must be the same. (LOD: {lodLevel}, Width: {rows}, Height: {columns}), but other was (LOD: {other.lodLevel}, Width: {other.rows}, Height: {other.columns})");
            }

            if (edge == Edge.ZPos)
            {
                for (uint i = 0; i < rows; i++)
                {
                    var indexA = GetIndexFor(i, columns - 1);
                    var indexB = GetIndexFor(i, 0);
                    positions[indexA] = other.positions[indexB] = (positions[indexA] + other.positions[indexB]) / 2;
                }
            }
            if (edge == Edge.ZNeg)
            {
                for (uint i = 0; i < rows; i++)
                {
                    var indexA = GetIndexFor(i, 0);
                    var indexB = GetIndexFor(i, columns - 1);
                    positions[indexA] = other.positions[indexB] = (positions[indexA] + other.positions[indexB]) / 2;
                }
            }
            if (edge == Edge.XPos)
            {
                for (uint i = 0; i < columns; i++)
                {
                    var indexA = GetIndexFor(rows - 1, i);
                    var indexB = GetIndexFor(0, i);
                    positions[indexA] = other.positions[indexB] = (positions[indexA] + other.positions[indexB]) / 2;
                }
            }
            if (edge == Edge.XNeg)
            {
                for (uint i = 0; i < columns; i++)
                {
                    var indexA = GetIndexFor(0, i);
                    var indexB = GetIndexFor(rows - 1, i);
                    positions[indexA] = other.positions[indexB] = (positions[indexA] + other.positions[indexB]) / 2;
                }
            }
        }

        public Face GetFaceAtIndex(uint index)
        {
            var i = index * 3;
            var idx1 = indices[i];
            var idx2 = indices[i + 1];
            var idx3 = indices[i + 2];

            return new(idx1, idx2, idx3);
        }

        public IIndexBuffer CreateIndexBuffer(IGraphicsDevice device, CpuAccessFlags accessFlags)
        {
            if (indexCount > ushort.MaxValue)
            {
                return new IndexBuffer<uint>(device, indices, accessFlags);
            }
            else
            {
                ushort[] indices = new ushort[indexCount];
                for (int i = 0; i < indexCount; i++)
                {
                    indices[i] = (ushort)this.indices[i];
                }

                return new IndexBuffer<ushort>(device, indices, accessFlags);
            }
        }

        public unsafe IVertexBuffer CreateVertexBuffer(IGraphicsDevice device, CpuAccessFlags accessFlags)
        {
            var stride = sizeof(TerrainVertex);
            var size = stride * (int)vertexCount;
            var vertices = (TerrainVertex*)Alloc(size);
            ZeroMemory(vertices, size);

            for (int i = 0; i < vertexCount; i++)
            {
                TerrainVertex vertex = default;
                vertex.Position = positions[i];
                vertex.UV = uvs[i];
                vertex.Normal = normals[i];
                vertex.Tangent = tangents[i];
                vertices[i] = vertex;
            }

            VertexBuffer<TerrainVertex> vertexBuffer = new(device, vertices, vertexCount, accessFlags);
            Free(vertices);
            return vertexBuffer;
        }

        public bool WriteIndexBuffer(IGraphicsContext context, IIndexBuffer ib)
        {
            if (indexCount > ushort.MaxValue && ib.Format == IndexFormat.UInt16)
            {
                return false;
            }
            if (ib is IIndexBuffer<ushort> indexBufferU16)
            {
                for (uint i = 0; i < indexCount; i++)
                {
                    indexBufferU16[i] = (ushort)indices[i];
                }
                return indexBufferU16.Update(context);
            }
            else if (ib is IIndexBuffer<uint> indexBufferU32)
            {
                for (uint i = 0; i < indexCount; i++)
                {
                    indexBufferU32[i] = indices[i];
                }
                return indexBufferU32.Update(context);
            }
            return false;
        }

        public bool WriteVertexBuffer(IGraphicsContext context, IVertexBuffer vb)
        {
            if (vb is not IVertexBuffer<TerrainVertex> vertexBuffer)
            {
                return false;
            }

            for (int i = 0; i < vertexCount; i++)
            {
                TerrainVertex vertex = default;
                vertex.Position = positions[i];
                vertex.UV = uvs[i];
                vertex.Normal = normals[i];
                vertex.Tangent = tangents[i];
                vertexBuffer[i] = vertex;
            }

            return vb.Update(context);
        }

        public void Generate(HeightMap heightMap)
        {
            float targetScaleFactor = width / (float)(rows - 1);

            Vector2 heightMapTexel = new Vector2(1) / new Vector2((float)rows / heightMap.Width, (float)columns / heightMap.Height);

            float minY = float.MaxValue;
            float maxY = float.MinValue;
            for (uint x = 0; x < columns; x++)
            {
                for (uint z = 0; z < rows; z++)
                {
                    uint xH = (uint)(x * heightMapTexel.X);
                    uint yH = (uint)(z * heightMapTexel.Y);

                    float y = heightMap[yH, xH];

                    minY = MathF.Min(y, minY);
                    maxY = MathF.Max(y, maxY);

                    Positions[x * rows + z] = new Vector3(z * targetScaleFactor, y, x * targetScaleFactor);
                }
            }

            Vector3 min = new(0, minY, 0);
            Vector3 max = new(width, maxY, height);

            box = new(min, max);
            sphere = BoundingSphere.CreateFromBoundingBox(box);

            GenVertexNormalsProcess.GenMeshVertexNormals2(this);
            CalcTangentsProcess.ProcessMesh2Parallel(this);
        }

        public void GenerateIndicesAndUVs()
        {
            float texel = width / (float)(rows - 1);
            Vector2 uvScale = new(texel, texel);

            int k = 0;
            int texUIndex = 0;
            int texVIndex = 0;
            for (uint i = 0; i < columns - 1; i++)
            {
                for (uint j = 0; j < rows - 1; j++)
                {
                    Vector2 uv = new(texUIndex, rows - 2 - texVIndex);
                    // Bottom left of quad
                    indices[k] = i * rows + j;
                    uvs[i * rows + j] = (uv + new Vector2(0.0f, 1.0f)) * uvScale;

                    // Top left of quad
                    indices[k + 1] = (i + 1) * rows + j;
                    uvs[(i + 1) * rows + j] = (uv + new Vector2(0.0f, 0.0f)) * uvScale;

                    // Bottom right of quad
                    indices[k + 2] = i * rows + j + 1;
                    uvs[i * rows + j + 1] = (uv + new Vector2(1.0f, 1.0f)) * uvScale;

                    // Top left of quad
                    indices[k + 3] = (i + 1) * rows + j;
                    uvs[(i + 1) * rows + j] = (uv + new Vector2(0.0f, 0.0f)) * uvScale;

                    // Top right of quad
                    indices[k + 4] = (i + 1) * rows + j + 1;
                    uvs[(i + 1) * rows + j + 1] = (uv + new Vector2(1.0f, 0.0f)) * uvScale;

                    // Bottom right of quad
                    indices[k + 5] = i * rows + j + 1;
                    uvs[i * rows + j + 1] = (uv + new Vector2(1.0f, 1.0f)) * uvScale;

                    k += 6; // next quad

                    texUIndex++;
                }
                texUIndex = 0;
                texVIndex++;
            }
        }
    }
}