namespace HexaEngine.Core.IO.Terrains
{
    using HexaEngine.Mathematics.Noise;
    using System;
    using System.Numerics;

    /// <summary>
    /// Represents a height map used for terrain generation.
    /// </summary>
    public class HeightMap
    {
        /// <summary>
        /// The width of the height map.
        /// </summary>
        public int Width;

        /// <summary>
        /// The height of the height map.
        /// </summary>
        public int Height;

        /// <summary>
        /// The data of the height map, stored as a 1D array of Vector3 values.
        /// </summary>
        public Vector3[] Data;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeightMap"/> class with the specified width, height, and data.
        /// </summary>
        /// <param name="width">The width of the height map.</param>
        /// <param name="height">The height of the height map.</param>
        /// <param name="data">The data of the height map.</param>
        public HeightMap(int width, int height, Vector3[] data)
        {
            Width = width;
            Height = height;
            Data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HeightMap"/> class with the specified width and height.
        /// </summary>
        /// <param name="width">The width of the height map.</param>
        /// <param name="height">The height of the height map.</param>
        public HeightMap(int width, int height)
        {
            Width = width;
            Height = height;
            Data = new Vector3[width * height];
        }

        /// <summary>
        /// Gets or sets the height map data at the specified index.
        /// </summary>
        /// <param name="index">The index of the height map data.</param>
        /// <returns>The height map data at the specified index.</returns>
        public Vector3 this[int index]
        {
            get => Data[index];
            set => Data[index] = value;
        }

        /// <summary>
        /// Generates an empty height map.
        /// </summary>
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

                    int index = Height * j + i;

                    Data[index].X = i;
                    Data[index].Y = (float)height / heightFactor;
                    Data[index].Z = j;
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
            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                {
                    float height = Random.Shared.NextSingle();

                    int index = Height * j + i;

                    Data[index].X = i;
                    Data[index].Y = (float)height * heightFactor;
                    Data[index].Z = j;
                }
            }
        }

        /// <summary>
        /// Generates a perlin noise height map.
        /// </summary>
        /// <param name="x">The x offset of the perlin noise.</param>
        /// <param name="z">The z offset of the perlin noise.</param>
        /// <param name="scale">The scale of the perlin noise.</param>
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

                    int index = Height * j + i;

                    Data[index].X = i;
                    Data[index].Y = (float)height * heightFactor;
                    Data[index].Z = j;
                }
            }
        }
    }
}