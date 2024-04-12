namespace HexaEngine.Core.IO.Binary.Terrains
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using HexaEngine.Mathematics.Noise;
    using System;
    using System.Numerics;

    /// <summary>
    /// Represents a height map used for terrain generation.
    /// </summary>
    public class HeightMap
    {
        private long streamPosition = -1;
        private Compression compression;

        /// <summary>
        /// The width of the height map.
        /// </summary>
        private uint width;

        /// <summary>
        /// The height of the height map.
        /// </summary>
        private uint height;

        /// <summary>
        /// The data of the height map, stored as a 1D array of floats values.
        /// </summary>
        private float[] data;

#nullable disable

        private HeightMap()
        {
        }

#nullable restore

        /// <summary>
        /// Initializes a new instance of the <see cref="HeightMap"/> class with the specified width, height, and data.
        /// </summary>
        /// <param name="width">The width of the height map.</param>
        /// <param name="height">The height of the height map.</param>
        /// <param name="data">The data of the height map.</param>
        public HeightMap(uint width, uint height, float[] data)
        {
            this.width = width;
            this.height = height;
            this.data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HeightMap"/> class with the specified width and height.
        /// </summary>
        /// <param name="width">The width of the height map.</param>
        /// <param name="height">The height of the height map.</param>
        public HeightMap(uint width = 160, uint height = 160)
        {
            this.width = width;
            this.height = height;
            data = new float[width * height];
        }

        /// <summary>
        /// Gets or sets the height map data at the specified index.
        /// </summary>
        /// <param name="index">The index of the height map data.</param>
        /// <returns>The height map data at the specified index.</returns>
        public float this[uint index]
        {
            get => data[index];
            set => data[index] = value;
        }

        /// <summary>
        /// Gets or sets the height map data at the specified coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>The height map data at the specified coordinates.</returns>
        public float this[uint x, uint y]
        {
            get => data[GetIndexFor(x, y)];
            set => data[GetIndexFor(x, y)] = value;
        }

        /// <summary>
        /// Gets the width of the height map.
        /// </summary>
        public uint Width => width;

        /// <summary>
        /// Gets the height of the height map.
        /// </summary>
        public uint Height => height;

        public Vector2 Size => new(width, height);

        /// <summary>
        /// Generates an empty height map.
        /// </summary>
        public void GenerateEmpty()
        {
            // Read the image data into our heightMap array
            for (uint j = 0; j < height; j++)
            {
                for (uint i = 0; i < width; i++)
                {
                    uint index = height * j + i;

                    data[index] = 0;
                }
            }
        }

        /// <summary>
        /// Generates a random height map.
        /// </summary>
        public void GenerateRandom()
        {
            // We divide the height by this number to "water down" the terrains height, otherwise the terrain will
            // appear to be "spikey" and not so smooth.
            float heightFactor = 10.0f;

            // Read the image data into our heightMap array
            for (uint j = 0; j < height; j++)
            {
                for (uint i = 0; i < width; i++)
                {
                    float heightValue = Random.Shared.NextSingle();

                    uint index = height * j + i;

                    data[index] = heightValue * heightFactor;
                }
            }
        }

        /// <summary>
        /// Generates a perlin noise height map.
        /// </summary>
        /// <param filename="x">The x offset of the perlin noise.</param>
        /// <param filename="z">The z offset of the perlin noise.</param>
        /// <param filename="scale">The scale of the perlin noise.</param>
        public void GeneratePerlin(float x, float z, float scale = 0.02f)
        {
            // We divide the height by this number to "water down" the terrains height, otherwise the terrain will
            // appear to be "spikey" and not so smooth.
            float heightFactor = 10.0f;

            PerlinNoise noise = new();

            // Read the image data into our heightMap array
            for (uint j = 0; j < height; j++)
            {
                for (uint i = 0; i < width; i++)
                {
                    float heightValue = (float)noise.Noise(scale * (i + x), scale * (j + z)) * 0.5f + 0.5f;

                    uint index = height * j + i;

                    data[index] = (float)heightValue * heightFactor;
                }
            }
        }

        private bool InBounds(uint row, uint col)
        {
            return row >= 0 && row < height && col >= 0 && col < width;
        }

        /// <summary>
        /// Smoothes the height map.
        /// </summary>
        public void Smooth()
        {
            float[] dest = new float[width * height];
            uint k = 0;
            for (uint j = 0; j < width; j++)
            {
                for (uint i = 0; i < height; i++)
                {
                    dest[k++] = Average(i, j);
                }
            }
            data = dest;
        }

        private float Average(uint row, uint col)
        {
            var avg = 0.0f;
            var num = 0.0f;
            for (uint n = col - 1; n <= col + 1; n++)
            {
                for (uint m = row - 1; m <= row + 1; m++)
                {
                    if (!InBounds(m, n))
                    {
                        continue;
                    }

                    avg += data[n * width + m];
                    num++;
                }
            }
            return avg / num;
        }

        /// <summary>
        /// Gets the index for the specified coordinates in the terrain.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>The index.</returns>
        public uint GetIndexFor(uint x, uint y)
        {
            return y * width + x;
        }

        /// <summary>
        /// Reads a <see cref="HeightMap"/> from a stream.
        /// </summary>
        /// <param name="src">The source stream.</param>
        /// <param name="endianness">The endianness used for reading.</param>
        /// <param name="compression"></param>
        /// <param name="mode"></param>
        /// <returns>The read height map.</returns>
        public static HeightMap ReadFrom(Stream src, Endianness endianness, Compression compression, TerrainLoadMode mode)
        {
            HeightMap heightMap = new();
            heightMap.Read(src, endianness, compression, mode);
            return heightMap;
        }

        /// <summary>
        /// Reads the height map data from a stream.
        /// </summary>
        /// <param name="src">The source stream.</param>
        /// <param name="endianness">The endianness used for reading.</param>
        /// <param name="compression"></param>
        /// <param name="mode"></param>
        public void Read(Stream src, Endianness endianness, Compression compression, TerrainLoadMode mode)
        {
            this.compression = compression;
            width = src.ReadUInt32(endianness);
            height = src.ReadUInt32(endianness);
            uint size = src.ReadUInt32(endianness);
            streamPosition = src.Position;

            if (mode == TerrainLoadMode.Immediate)
            {
                LoadHeightMapData(src, endianness);
            }

            src.Position = streamPosition + size;
        }

        public void LoadHeightMapData(Stream stream, Endianness endianness)
        {
            stream.Position = streamPosition;

            var decompressor = stream.CreateDecompressionStream(compression, out var isCompressed);

            data = new float[width * height];
            decompressor.ReadArrayFloat(data, endianness);

            if (isCompressed)
            {
                decompressor.Dispose();
            }
        }

        /// <summary>
        /// Writes the height map data to a stream.
        /// </summary>
        /// <param name="stream">The destination stream.</param>
        /// <param name="endianness">The endianness used for writing.</param>
        /// <param name="compression"></param>
        public void Write(Stream stream, Endianness endianness, Compression compression)
        {
            stream.WriteUInt32(width, endianness);
            stream.WriteUInt32(height, endianness);
            long basePosition = stream.Position;
            stream.Position += 4;

            var compressor = stream.CreateCompressionStream(compression, out var isCompressed);

            compressor.WriteArrayFloat(data, endianness);

            if (isCompressed)
            {
                compressor.Dispose();
            }

            long endPos = stream.Position;
            uint size = (uint)(endPos - (basePosition + 4));
            stream.Position = basePosition;
            stream.WriteUInt32(size, endianness);
            stream.Position = endPos;
        }

        /// <summary>
        /// Creates a <see cref="Texture2D"/> from the height map.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="gpuAccessFlags">The GPU access flags.</param>
        /// <param name="cpuAccessFlags">The CPU access flags.</param>
        /// <returns>The created height map texture.</returns>
        public unsafe Texture2D CreateHeightMap(IGraphicsDevice device, GpuAccessFlags gpuAccessFlags, CpuAccessFlags cpuAccessFlags)
        {
            Texture2DDescription desc = new(Format.R32Float, (int)width, (int)height, 1, 1, gpuAccessFlags, cpuAccessFlags);

            Texture2D heightMap;
            fixed (float* pData = data)
            {
                heightMap = new(desc, new SubresourceData(pData, (int)(width * sizeof(float))));
            }

            return heightMap;
        }

        /// <summary>
        /// Creates a staging <see cref="Texture2D"/> from the height map.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="accessFlags">The CPU access flags.</param>
        /// <returns>The created staging height map texture.</returns>
        public unsafe Texture2D CreateStagingHeightMap(IGraphicsDevice device, CpuAccessFlags accessFlags = CpuAccessFlags.None)
        {
            Texture2DDescription desc = new(Format.R32Float, (int)width, (int)height, 1, 1, BindFlags.ShaderResource, Usage.Staging, accessFlags);
            Texture2D heightMap;
            fixed (float* pData = data)
            {
                heightMap = new(desc, new SubresourceData(pData, (int)(width * sizeof(float))));
            }

            return heightMap;
        }

        /// <summary>
        /// Writes the height map data to a texture.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        /// <param name="texture">The destination texture.</param>
        public unsafe void WriteHeightMap(IGraphicsContext context, Texture2D texture)
        {
            if ((texture.CpuAccessFlags & CpuAccessFlags.Write) != 0)
            {
                uint bufferSize = height * (uint)texture.RowPitch;
                byte* buffer = AllocT<byte>(bufferSize);

                var pixel = (float*)buffer;
                for (int i = 0; i < data.Length; i++)
                {
                    pixel[i] = data[i];
                }

                texture.Write(context, buffer, bufferSize);
                Free(buffer);
            }
            else
            {
                var staging = CreateStagingHeightMap(context.Device, CpuAccessFlags.None);
                staging.CopyTo(context, texture);
                staging.Dispose();
            }
        }

        /// <summary>
        /// Reads the height map data from a texture.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        /// <param name="texture">The source texture.</param>
        public unsafe void ReadHeightMap(IGraphicsContext context, Texture2D texture)
        {
            if ((texture.CpuAccessFlags & CpuAccessFlags.Read) != 0)
            {
                uint bufferSize = height * (uint)texture.RowPitch;
                byte* buffer = AllocT<byte>(bufferSize);

                texture.Read(context, buffer, bufferSize);

                var pixel = (float*)buffer;
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = pixel[i];
                }

                Free(buffer);
            }
            else
            {
                Texture2DDescription desc = new(Format.R32Float, (int)width, (int)height, 1, 1, BindFlags.ShaderResource, Usage.Staging, CpuAccessFlags.Read);
                Texture2D staging = new(desc);

                uint bufferSize = height * (uint)texture.RowPitch;
                byte* buffer = AllocT<byte>(bufferSize);

                texture.CopyTo(context, staging);

                staging.Read(context, buffer, bufferSize);
                staging.Dispose();

                var pixel = (float*)buffer;
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = pixel[i];
                }

                Free(buffer);
            }
        }
    }
}