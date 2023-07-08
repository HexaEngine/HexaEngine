namespace HexaEngine.Core.IO.Terrains
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO.Meshes.Processing;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public unsafe class Terrain
    {
        public uint VerticesCount;
        public uint IndicesCount;
        public uint Width;
        public uint Height;
        public uint[] Indices;
        public Vector3[] Positions;
        public Vector3[] UVs;
        public Vector3[] Normals;
        public Vector3[] Tangents;
        public Vector3[] Bitangents;
        public BoundingBox Box;
        public TerrainFlags Flags;

        public Terrain(HeightMap map)
        {
            Width = (uint)map.Width;
            Height = (uint)map.Height;
            int cols = map.Width;
            int rows = map.Height;

            //Create the grid
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

        public IndexBuffer CreateIndexBuffer(IGraphicsDevice device, CpuAccessFlags accessFlags = CpuAccessFlags.None)
        {
            return new(device, Indices, accessFlags);
        }

        public bool WriteIndexBuffer(IGraphicsContext context, IndexBuffer ib)
        {
            for (int i = 0; i < IndicesCount; i++)
            {
                ib[i] = Indices[i];
            }

            return ib.Update(context);
        }

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

            VertexBuffer<TerrainVertex> vertexBuffer = new(device, accessFlags, vertices, VerticesCount);
            Free(vertices);
            return vertexBuffer;
        }

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

        public static readonly InputElementDescription[] InputElements =
{
            new InputElementDescription("POSITION", 0, Format.R32G32B32Float, 0),
            new InputElementDescription("TEXCOORD", 0, Format.R32G32B32Float, 0),
            new InputElementDescription("NORMAL", 0, Format.R32G32B32Float, 0),
            new InputElementDescription("TANGENT", 0, Format.R32G32B32Float, 0),
            new InputElementDescription("BINORMAL", 0, Format.R32G32B32Float, 0),
        };

        public void Recalculate()
        {
            Box = BoundingBoxHelper.Compute(Positions);
            GenVertexNormalsProcess.GenMeshVertexNormals2(this);
            CalcTangentsProcess.ProcessMesh2(this);
        }

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

        public uint GetIndexFor(uint x, uint y)
        {
            return y * Width + x;
        }

        public int GetIndexFor(int x, int y)
        {
            return (int)(x * Width + y);
        }

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

    public enum Edge
    {
        XPos,
        XNeg,
        ZPos,
        ZNeg,
    }
}