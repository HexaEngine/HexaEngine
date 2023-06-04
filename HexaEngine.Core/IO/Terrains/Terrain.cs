namespace HexaEngine.Core.IO.Terrains
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Lights;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;
    using System.Reflection;

    public class HeightMap
    {
        public int Width;
        public int Height;
        public Vector3[] Data;

        public HeightMap(int width, int height, Vector3[] data)
        {
            Width = width;
            Height = height;
            Data = data;
        }

        public HeightMap(int width, int height)
        {
            Width = width;
            Height = height;
            Data = new Vector3[width * height];
        }

        public Vector3 this[int index]
        {
            get => Data[index];
            set => Data[index] = value;
        }

        public void GenerateEmpty()
        {
            // We divide the height by this number to "water down" the terrains height, otherwise the terrain will
            // appear to be "spikey" and not so smooth.
            float heightFactor = 10.0f;

            // Read the image data into our heightMap array
            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                {
                    float height = 0;

                    int index = (Height * j) + i;

                    Data[index].X = i;
                    Data[index].Y = (float)height / heightFactor;
                    Data[index].Z = j;
                }
            }
        }

        public void GenerateRandom()
        {
            // We divide the height by this number to "water down" the terrains height, otherwise the terrain will
            // appear to be "spikey" and not so smooth.
            float heightFactor = 10.0f;

            // Read the image data into our heightMap array
            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                {
                    float height = Random.Shared.NextSingle();

                    int index = (Height * j) + i;

                    Data[index].X = i;
                    Data[index].Y = (float)height * heightFactor;
                    Data[index].Z = j;
                }
            }
        }

        public void GeneratePerlin(float x, float z, float scale = 0.02f)
        {
            // We divide the height by this number to "water down" the terrains height, otherwise the terrain will
            // appear to be "spikey" and not so smooth.
            float heightFactor = 10.0f;

            PerlinNoise noise = new();

            // Read the image data into our heightMap array
            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                {
                    float height = (float)noise.Function2D(scale * (i + x), scale * (j + z)) * 0.5f + 0.5f;

                    int index = (Height * j) + i;

                    Data[index].X = i;
                    Data[index].Y = (float)height * heightFactor;
                    Data[index].Z = j;
                }
            }
        }
    }

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
                if ((Flags & TerrainFlags.Positions) != 0)
                {
                    Positions[i].CopyTo(&m, buffer);
                }

                if ((Flags & TerrainFlags.UVs) != 0)
                {
                    UVs[i].CopyTo(&m, buffer);
                }

                if ((Flags & TerrainFlags.Normals) != 0)
                {
                    Normals[i].CopyTo(&m, buffer);
                }

                if ((Flags & TerrainFlags.Tangents) != 0)
                {
                    Tangents[i].CopyTo(&m, buffer);
                }

                if ((Flags & TerrainFlags.Bitangents) != 0)
                {
                    Bitangents[i].CopyTo(&m, buffer);
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
                if ((Flags & TerrainFlags.Positions) != 0)
                {
                    Positions[i].CopyTo(&m, buffer);
                }

                if ((Flags & TerrainFlags.UVs) != 0)
                {
                    UVs[i].CopyTo(&m, buffer);
                }

                if ((Flags & TerrainFlags.Normals) != 0)
                {
                    Normals[i].CopyTo(&m, buffer);
                }

                if ((Flags & TerrainFlags.Tangents) != 0)
                {
                    Tangents[i].CopyTo(&m, buffer);
                }

                if ((Flags & TerrainFlags.Bitangents) != 0)
                {
                    Bitangents[i].CopyTo(&m, buffer);
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

        public unsafe uint GetStride()
        {
            int result = 0;

            if ((Flags & TerrainFlags.Positions) != 0)
            {
                result += sizeof(Vector3);
            }

            if ((Flags & TerrainFlags.UVs) != 0)
            {
                result += sizeof(Vector3);
            }

            if ((Flags & TerrainFlags.Normals) != 0)
            {
                result += sizeof(Vector3);
            }

            if ((Flags & TerrainFlags.Tangents) != 0)
            {
                result += sizeof(Vector3);
            }

            if ((Flags & TerrainFlags.Bitangents) != 0)
            {
                result += sizeof(Vector3);
            }

            return (uint)result;
        }

        public InputElementDescription[] GetInputElements()
        {
            var count = ((uint)Flags).Bitcount();

            var elements = new InputElementDescription[count];
            int i = 0;

            if ((Flags & TerrainFlags.Positions) != 0)
            {
                elements[i++] = new InputElementDescription("POSITION", 0, Format.R32G32B32Float, 0);
            }

            if ((Flags & TerrainFlags.UVs) != 0)
            {
                elements[i++] = new InputElementDescription("TEXCOORD", 0, Format.R32G32B32Float, 0);
            }

            if ((Flags & TerrainFlags.Normals) != 0)
            {
                elements[i++] = new InputElementDescription("NORMAL", 0, Format.R32G32B32Float, 0);
            }

            if ((Flags & TerrainFlags.Tangents) != 0)
            {
                elements[i++] = new InputElementDescription("TANGENT", 0, Format.R32G32B32Float, 0);
            }

            if ((Flags & TerrainFlags.Bitangents) != 0)
            {
                elements[i++] = new InputElementDescription("BINORMAL", 0, Format.R32G32B32Float, 0);
            }

            return elements;
        }

        public ShaderMacro[] GetShaderMacros()
        {
            var count = ((uint)Flags).Bitcount() + 1;
            var macros = new ShaderMacro[count];
            int i = 0;

            macros[i++] = new ShaderMacro("TILESIZE", $"float2({Width},{Height})");

            if ((Flags & TerrainFlags.Positions) != 0)
            {
                macros[i++] = new ShaderMacro("VtxPosition", "1");
            }

            if ((Flags & TerrainFlags.UVs) != 0)
            {
                macros[i++] = new ShaderMacro("VtxUV", "1");
            }

            if ((Flags & TerrainFlags.Normals) != 0)
            {
                macros[i++] = new ShaderMacro("VtxNormal", "1");
            }

            if ((Flags & TerrainFlags.Tangents) != 0)
            {
                macros[i++] = new ShaderMacro("VtxTangent", "1");
            }

            if ((Flags & TerrainFlags.Bitangents) != 0)
            {
                macros[i++] = new ShaderMacro("VtxBitangent", "1");
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
    }
}