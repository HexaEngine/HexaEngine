namespace HexaEngine.Objects
{
    using HexaEngine.Core.Meshes;
    using System.Numerics;

    public class StaticTerrain
    {
#pragma warning disable CS8618 // Non-nullable field 'Vertices' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        public TerrainVertexStatic[] Vertices;
#pragma warning restore CS8618 // Non-nullable field 'Vertices' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS8618 // Non-nullable field 'Indices' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        public int[] Indices;
#pragma warning restore CS8618 // Non-nullable field 'Indices' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        public readonly int Height;
        public readonly int Width;
        public readonly int Scale;
#pragma warning disable CS8618 // Non-nullable field 'Heights' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        public float[] Heights;
#pragma warning restore CS8618 // Non-nullable field 'Heights' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.

        private void CalculateVertices()
        {
            int iindex = 0;
            int vindex = 0;

            for (int j = 0; j < Height - 1; j++)
            {
                for (int i = 0; i < Width - 1; i++)
                {
                    int index1 = Width * j + i;             // Bottom left.
                    int index2 = Width * j + i + 1;         // Bottom right.
                    int index3 = Width * (j + 1) + i;       // Upper left.
                    int index4 = Width * (j + 1) + i + 1;   // Upper right.

                    Vector3 pos1 = new(i, 0, j);
                    Vector3 pos2 = new(i + 1, 0, j);
                    Vector3 pos3 = new(i, 0, j + 1);
                    Vector3 pos4 = new(i + 1, 0, j + 1);

                    var u1 = pos1 - pos3;
                    //var normal = Vector3.Cross();

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

                    Indices[iindex + 0] = vindex - 1;
                    Indices[iindex + 1] = vindex - 3;
                    Indices[iindex + 2] = vindex - 2;
                    Indices[iindex + 3] = vindex - 2;
                    Indices[iindex + 4] = vindex - 3;
                    Indices[iindex + 5] = vindex - 4;
                    iindex += 6;
                }
            }
        }
    }
}