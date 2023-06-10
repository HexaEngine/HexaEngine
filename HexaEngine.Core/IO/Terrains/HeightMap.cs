namespace HexaEngine.Core.IO.Terrains
{
    using HexaEngine.Mathematics.Noise;
    using System;
    using System.Numerics;

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

                    int index = Height * j + i;

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

                    int index = Height * j + i;

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

                    int index = Height * j + i;

                    Data[index].X = i;
                    Data[index].Y = (float)height * heightFactor;
                    Data[index].Z = j;
                }
            }
        }
    }
}