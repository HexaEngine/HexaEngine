namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public class Grid : Primitive<VertexPositionColor, uint>
    {
        private const int width = 256, height = 256;

        private int VertexCount;
        private int IndexCount;

        public Grid(IGraphicsDevice device) : base(device)
        {
        }

        protected override (VertexBuffer<VertexPositionColor>, IndexBuffer<uint>?) InitializeMesh(IGraphicsDevice device)
        {
            (VertexPositionColor[] vertices, uint[] indices) = GenerateGrid();
            return (new VertexBuffer<VertexPositionColor>(device, CpuAccessFlags.None, vertices), new IndexBuffer<uint>(device, indices, CpuAccessFlags.None));
        }

        public static (VertexPositionColor[], uint[]) GenerateGrid()
        {
            // Calculate the number of vertices in the terrain mesh.
            int vertexCount = (width - 1) * (height - 1) * 8;
            // Set the index count to the same as the vertex count.
            int indexCount = vertexCount;

            // Create the vertex array.
            VertexPositionColor[] vertices = new VertexPositionColor[vertexCount];
            // Create the index array.
            uint[] indices = new uint[indexCount];

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

            return (vertices, indices);
        }
    }
}