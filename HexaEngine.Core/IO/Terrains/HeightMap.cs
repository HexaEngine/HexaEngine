namespace HexaEngine.Core.IO.Terrains
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Mathematics.Noise;
    using System;
    using System.Text;

    /// <summary>
    /// Represents a height map used for terrain generation.
    /// </summary>
    public class HeightMap
    {
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
        /// <param filename="width">The width of the height map.</param>
        /// <param filename="height">The height of the height map.</param>
        /// <param filename="data">The data of the height map.</param>
        public HeightMap(uint width, uint height, float[] data)
        {
            this.width = width;
            this.height = height;
            this.data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HeightMap"/> class with the specified width and height.
        /// </summary>
        /// <param filename="width">The width of the height map.</param>
        /// <param filename="height">The height of the height map.</param>
        public HeightMap(uint width = 32, uint height = 32)
        {
            this.width = width;
            this.height = height;
            data = new float[width * height];
        }

        /// <summary>
        /// Gets or sets the height map data at the specified index.
        /// </summary>
        /// <param filename="index">The index of the height map data.</param>
        /// <returns>The height map data at the specified index.</returns>
        public float this[uint index]
        {
            get => data[index];
            set => data[index] = value;
        }

        /// <summary>
        /// Gets or sets the height map data at the specified index.
        /// </summary>
        /// <param filename="index">The index of the height map data.</param>
        /// <returns>The height map data at the specified index.</returns>
        public float this[int index]
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
                    if (!InBounds(m, n)) continue;

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
        /// <param name="encoding">The encoding used for reading.</param>
        /// <param name="endianness">The endianness used for reading.</param>
        /// <returns>The read height map.</returns>
        public static HeightMap ReadFrom(Stream src, Encoding encoding, Endianness endianness)
        {
            HeightMap heightMap = new();
            heightMap.Read(src, encoding, endianness);
            return heightMap;
        }

        /// <summary>
        /// Reads the height map data from a stream.
        /// </summary>
        /// <param name="src">The source stream.</param>
        /// <param name="encoding">The encoding used for reading.</param>
        /// <param name="endianness">The endianness used for reading.</param>
        public void Read(Stream src, Encoding encoding, Endianness endianness)
        {
            width = src.ReadUInt32(endianness);
            height = src.ReadUInt32(endianness);
            data = new float[width * height];

            for (uint i = 0; i < height; i++)
            {
                for (uint j = 0; j < width; j++)
                {
                    uint index = height * j + i;

                    data[index] = src.ReadFloat(endianness);
                }
            }
        }

        /// <summary>
        /// Writes the height map data to a stream.
        /// </summary>
        /// <param name="dst">The destination stream.</param>
        /// <param name="encoding">The encoding used for writing.</param>
        /// <param name="endianness">The endianness used for writing.</param>
        public void Write(Stream dst, Encoding encoding, Endianness endianness)
        {
            dst.WriteUInt32(width, endianness);
            dst.WriteUInt32(height, endianness);

            for (uint i = 0; i < height; i++)
            {
                for (uint j = 0; j < width; j++)
                {
                    uint index = height * j + i;
                    dst.WriteFloat(data[index], endianness);
                }
            }
        }

        /// <summary>
        /// Creates a <see cref="Texture2D"/> from the height map.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="accessFlags">The CPU access flags.</param>
        /// <param name="uav">Indicates whether to create an unordered access view.</param>
        /// <param name="immutable">Indicates whether the texture is immutable.</param>
        /// <returns>The created height map texture.</returns>
        public unsafe Texture2D CreateHeightMap(IGraphicsDevice device, CpuAccessFlags accessFlags = CpuAccessFlags.None, bool uav = false, bool immutable = true)
        {
            Half* pixel = AllocT<Half>(data.Length);

            for (int i = 0; i < data.Length; i++)
            {
                pixel[i] = (Half)data[i];
            }

            Texture2DDescription desc = new(Format.R16Float, (int)width, (int)height, 1, 1, BindFlags.ShaderResource, immutable ? Usage.Immutable : Usage.Default, accessFlags);
            if ((accessFlags & CpuAccessFlags.Write) != 0)
            {
                desc.Usage = Usage.Dynamic;
            }
            if ((accessFlags & CpuAccessFlags.Read) != 0)
            {
                desc.Usage = Usage.Staging;
            }
            if (accessFlags != CpuAccessFlags.None && uav)
            {
                throw new ArgumentException("Cannot access from cpu and uav at the same time");
            }
            if (uav)
            {
                desc.Usage = Usage.Default;
                desc.BindFlags |= BindFlags.UnorderedAccess;
            }

            Texture2D heightMap = new(device, desc, new SubresourceData(pixel, (int)(width * sizeof(Half))));
            Free(pixel);
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
            Half* pixel = AllocT<Half>(data.Length);

            for (int i = 0; i < data.Length; i++)
            {
                pixel[i] = (Half)data[i];
            }

            Texture2DDescription desc = new(Format.R16Float, (int)width, (int)height, 1, 1, BindFlags.ShaderResource, Usage.Staging, accessFlags);

            Texture2D heightMap = new(device, desc, new SubresourceData(pixel, (int)(width * sizeof(Half))));
            Free(pixel);
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
                var pixel = (Half*)texture.Local;
                for (int i = 0; i < data.Length; i++)
                {
                    pixel[i] = (Half)data[i];
                }
                texture.Write(context);
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
                texture.Read(context);
                var pixel = (Half*)texture.Local;
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = (float)pixel[i];
                }
            }
            else
            {
                Texture2DDescription desc = new(Format.R16Float, (int)width, (int)height, 1, 1, BindFlags.ShaderResource, Usage.Staging, CpuAccessFlags.Read);
                Texture2D staging = new(context.Device, desc);

                texture.CopyTo(context, staging);

                staging.Read(context);
                var pixel = (Half*)texture.Local;
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = (float)pixel[i];
                }
                staging.Dispose();
            }
        }
    }
}