namespace HexaEngine.Objects
{
    using HexaEngine.Core.Meshes;
    using System.Numerics;

    public class DynamicTerrain
    {
        public readonly TerrainVertex[] Vertices;
        public readonly int[] Indices;
        public readonly int Height;
        public readonly int Width;
        public readonly int Scale;

        public DynamicTerrain(int height, int width, int scale)
        {
            Height = height;
            Width = width;
            Scale = scale;
            Vertices = new TerrainVertex[(height - 1) * (width - 1) * 4];
            Indices = new int[(height - 1) * (width - 1) * 6];
            CalculateVertices();
        }

        private void CalculateVertices()
        {
            int iindex = 0;
            int vindex = 0;

            for (int j = 0; j < Height - 1; j++)
            {
                for (int i = 0; i < Width - 1; i++)
                {
                    Vector3 pos1 = new(i + 1, 0, j + 1);
                    Vector3 pos2 = new(i + 1, 0, j);
                    Vector3 pos3 = new(i, 0, j + 1);
                    Vector3 pos4 = new(i, 0, j);

                    Vertices[vindex + 0].Position = pos1;
                    Vertices[vindex + 0].Texture = new Vector2(0.0f, 0.0f);
                    Vertices[vindex + 0].CTexture = new(pos1.X / (Height - 1f), pos1.Z / (Width - 1f));
                    Vertices[vindex + 1].Position = pos2;
                    Vertices[vindex + 1].Texture = new Vector2(1.0f, 0.0f);
                    Vertices[vindex + 1].CTexture = new(pos2.X / (Height - 1f), pos2.Z / (Width - 1f));
                    Vertices[vindex + 2].Position = pos3;
                    Vertices[vindex + 2].Texture = new Vector2(0.0f, 1.0f);
                    Vertices[vindex + 2].CTexture = new(pos3.X / (Height - 1f), pos3.Z / (Width - 1f));
                    Vertices[vindex + 3].Position = pos4;
                    Vertices[vindex + 3].Texture = new Vector2(1.0f, 1.0f);
                    Vertices[vindex + 3].CTexture = new(pos4.X / (Height - 1f), pos4.Z / (Width - 1f));
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