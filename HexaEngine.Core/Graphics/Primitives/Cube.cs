namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO;
    using System;
    using System.Numerics;

    /// <summary>
    /// Represents a cube primitive in 3D space.
    /// </summary>
    public sealed class Cube : Primitive<Vector3, ushort>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Cube"/> class.
        /// </summary>
        /// <param name="device">The graphics device used for mesh creation.</param>
        public Cube(IGraphicsDevice device) : base(device)
        {
        }

        /// <summary>
        /// Initializes the cube mesh with vertices and indices.
        /// </summary>
        /// <param name="device">The graphics device used for mesh creation.</param>
        /// <returns>
        /// A tuple containing the vertex buffer and optional index buffer of the cube mesh.
        /// </returns>
        protected override (VertexBuffer<Vector3>, IndexBuffer<ushort>?) InitializeMesh(IGraphicsDevice device)
        {
            VertexBuffer<Vector3> vertexBuffer = new(device, new Vector3[]
            {
                new Vector3(-1.0f, 1.0f, -1.0f),
                new Vector3(-1.0f, -1.0f, -1.0f),
                new Vector3(1.0f, -1.0f, -1.0f),
                new Vector3(1.0f, 1.0f, -1.0f),
                new Vector3(-1.0f, -1.0f, 1.0f),
                new Vector3(-1.0f, 1.0f, 1.0f),
                new Vector3(1.0f, -1.0f, 1.0f),
                new Vector3(1.0f, 1.0f, 1.0f)
            }, CpuAccessFlags.None);

            IndexBuffer<ushort> indexBuffer = new(device, new ushort[]
            {
                0,1,2,2,3,0,
                4,1,0,0,5,4,
                2,6,7,7,3,2,
                4,5,7,7,6,4,
                0,3,7,7,5,0,
                1,4,2,2,4,6,
            }, CpuAccessFlags.None);

            return (vertexBuffer, indexBuffer);
        }

        /// <summary>
        /// Generates vertices and indices for a cube mesh.
        /// </summary>
        /// <param name="device">The graphics device used for mesh creation.</param>
        /// <param name="vertexBuffer">The vertex buffer of the cube mesh.</param>
        /// <param name="indexBuffer">The optional index buffer of the cube mesh.</param>
        /// <param name="width">The width of the cube.</param>
        /// <param name="height">The height of the cube.</param>
        /// <param name="depth">The depth of the cube.</param>
        public static void CreateCube(IGraphicsDevice device, out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer<ushort> indexBuffer, float width = 1, float height = 1, uint depth = 1)
        {
            throw new NotImplementedException();
        }
    }
}