namespace HexaEngine.Objects.Primitives
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Buffers;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using System.Numerics;

    public class Grid : Primitive<VertexPositionColor>
    {
        private const int width = 256, height = 256;

        private int VertexCount;
        private int IndexCount;

        public Grid(IGraphicsDevice device) : base(device)
        {
        }

        protected override (VertexBuffer<VertexPositionColor>, IndexBuffer?) InitializeMesh(IGraphicsDevice device)
        {
            // Calculate the number of vertices in the terrain mesh.
            VertexCount = (width - 1) * (height - 1) * 8;
            // Set the index count to the same as the vertex count.
            IndexCount = VertexCount;

            // Create the vertex array.
            VertexPositionColor[] vertices = new VertexPositionColor[VertexCount];
            // Create the index array.
            uint[] indices = new uint[IndexCount];

            // Initialize the index to the vertex array.
            uint index = 0;

            // Load the vertex and index arrays with the terrain data.
            for (int j = 0; j < height - 1; j++)
            {
                for (int i = 0; i < width - 1; i++)
                {
                    // LINE 1
                    // Upper left.
                    float positionX = i;
                    float positionZ = j + 1;
                    vertices[index].Position = new Vector3(positionX, 0.0f, positionZ);
                    vertices[index].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    indices[index] = index;
                    index++;
                    // Upper right.
                    positionX = i + 1;
                    positionZ = j + 1;
                    vertices[index].Position = new Vector3(positionX, 0.0f, positionZ);
                    vertices[index].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    indices[index] = index;
                    index++;

                    // LINE 2
                    // Upper right.
                    positionX = i + 1;
                    positionZ = j + 1;
                    vertices[index].Position = new Vector3(positionX, 0.0f, positionZ);
                    vertices[index].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    indices[index] = index;
                    index++;
                    // Bottom right.
                    positionX = i + 1;
                    positionZ = j;
                    vertices[index].Position = new Vector3(positionX, 0.0f, positionZ);
                    vertices[index].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    indices[index] = index;
                    index++;

                    // LINE 3
                    // Bottom right.
                    positionX = i + 1;
                    positionZ = j;
                    vertices[index].Position = new Vector3(positionX, 0.0f, positionZ);
                    vertices[index].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    indices[index] = index;
                    index++;
                    // Bottom left.
                    positionX = i;
                    positionZ = j;
                    vertices[index].Position = new Vector3(positionX, 0.0f, positionZ);
                    vertices[index].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    indices[index] = index;
                    index++;

                    // LINE 4
                    // Bottom left.
                    positionX = i;
                    positionZ = j;
                    vertices[index].Position = new Vector3(positionX, 0.0f, positionZ);
                    vertices[index].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    indices[index] = index;
                    index++;
                    // Upper left.
                    positionX = i;
                    positionZ = j + 1;
                    vertices[index].Position = new Vector3(positionX, 0.0f, positionZ);
                    vertices[index].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    indices[index] = index;
                    index++;
                }
            }

            return (new VertexBuffer<VertexPositionColor>(device, CpuAccessFlags.None, vertices), new IndexBuffer(device, CpuAccessFlags.None, indices));
        }

        public static (VertexPositionColor[], int[]) GenerateGrid()
        {
            // Calculate the number of vertices in the terrain mesh.
            int VertexCount = (width - 1) * (height - 1) * 8;
            // Set the index count to the same as the vertex count.
            int IndexCount = VertexCount;

            // Create the vertex array.
            VertexPositionColor[] vertices = new VertexPositionColor[VertexCount];
            // Create the index array.
            int[] indices = new int[IndexCount];

            // Initialize the index to the vertex array.
            int index = 0;

            // Load the vertex and index arrays with the terrain data.
            for (int j = 0; j < height - 1; j++)
            {
                for (int i = 0; i < width - 1; i++)
                {
                    // LINE 1
                    // Upper left.
                    float positionX = i;
                    float positionZ = j + 1;
                    vertices[index].Position = new Vector3(positionX, 0.0f, positionZ);
                    vertices[index].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    indices[index] = index;
                    index++;
                    // Upper right.
                    positionX = i + 1;
                    positionZ = j + 1;
                    vertices[index].Position = new Vector3(positionX, 0.0f, positionZ);
                    vertices[index].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    indices[index] = index;
                    index++;

                    // LINE 2
                    // Upper right.
                    positionX = i + 1;
                    positionZ = j + 1;
                    vertices[index].Position = new Vector3(positionX, 0.0f, positionZ);
                    vertices[index].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    indices[index] = index;
                    index++;
                    // Bottom right.
                    positionX = i + 1;
                    positionZ = j;
                    vertices[index].Position = new Vector3(positionX, 0.0f, positionZ);
                    vertices[index].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    indices[index] = index;
                    index++;

                    // LINE 3
                    // Bottom right.
                    positionX = i + 1;
                    positionZ = j;
                    vertices[index].Position = new Vector3(positionX, 0.0f, positionZ);
                    vertices[index].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    indices[index] = index;
                    index++;
                    // Bottom left.
                    positionX = i;
                    positionZ = j;
                    vertices[index].Position = new Vector3(positionX, 0.0f, positionZ);
                    vertices[index].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    indices[index] = index;
                    index++;

                    // LINE 4
                    // Bottom left.
                    positionX = i;
                    positionZ = j;
                    vertices[index].Position = new Vector3(positionX, 0.0f, positionZ);
                    vertices[index].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    indices[index] = index;
                    index++;
                    // Upper left.
                    positionX = i;
                    positionZ = j + 1;
                    vertices[index].Position = new Vector3(positionX, 0.0f, positionZ);
                    vertices[index].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    indices[index] = index;
                    index++;
                }
            }

            return (vertices, indices);
        }
    }
}