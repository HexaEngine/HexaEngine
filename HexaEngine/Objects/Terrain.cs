namespace HexaEngine.Objects
{
    using HexaEngine.Meshes;
    using System.Numerics;

    public class Terrain
    {
        public readonly TerrainVertex[] Vertices;
        public readonly int[] Indices;
        public readonly int Height;
        public readonly int Width;
        public readonly int Scale;

        public Terrain(int height, int width, int scale)
        {
            Height = height;
            Width = width;
            Scale = scale;
            Vertices = new TerrainVertex[(height - 1) * (width - 1) * 4];
            Indices = new int[(height - 1) * (width - 1) * 6];
            CalculateVertices();
        }

        public static Vector3[] GenerateFlat(int height, int width)
        {
            Vector3[] positions = new Vector3[height * width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int idx = i * height + j;
                    positions[idx] = new Vector3(j, 0, i);
                }
            }
            return positions;
        }

        private void CalculateVertices()
        {
            int pindex = 0;
            int iindex = 0;
            int vindex = 0;

            Vector3[] positions = new Vector3[Height * Width];
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    positions[pindex++] = new Vector3(j, 0, i);
                }
            }

            for (int j = 0; j < Height - 1; j++)
            {
                for (int i = 0; i < Width - 1; i++)
                {
                    int index1 = Width * j + i;             // Bottom left.
                    int index2 = Width * j + i + 1;         // Bottom right.
                    int index3 = Width * (j + 1) + i;       // Upper left.
                    int index4 = Width * (j + 1) + i + 1;   // Upper right.

                    Vertices[vindex + 0].Position = positions[index3];
                    Vertices[vindex + 0].Texture = new Vector2(0.0f, 0.0f);
                    Vertices[vindex + 0].CTexture = new(positions[index3].X / (Height - 1f), positions[index3].Z / (Width - 1f));
                    Vertices[vindex + 1].Position = positions[index4];
                    Vertices[vindex + 1].Texture = new Vector2(1.0f, 0.0f);
                    Vertices[vindex + 1].CTexture = new(positions[index4].X / (Height - 1f), positions[index3].Z / (Width - 1f));
                    Vertices[vindex + 2].Position = positions[index1];
                    Vertices[vindex + 2].Texture = new Vector2(0.0f, 1.0f);
                    Vertices[vindex + 2].CTexture = new(positions[index1].X / (Height - 1f), positions[index3].Z / (Width - 1f));
                    Vertices[vindex + 3].Position = positions[index2];
                    Vertices[vindex + 3].Texture = new Vector2(1.0f, 1.0f);
                    Vertices[vindex + 3].CTexture = new(positions[index2].X / (Height - 1f), positions[index3].Z / (Width - 1f));
                    vindex += 4;

                    Indices[iindex + 0] = vindex - 4;
                    Indices[iindex + 1] = vindex - 3;
                    Indices[iindex + 2] = vindex - 2;
                    Indices[iindex + 3] = vindex - 2;
                    Indices[iindex + 4] = vindex - 3;
                    Indices[iindex + 5] = vindex - 1;
                    iindex += 6;
                }
            }
        }
    }
}