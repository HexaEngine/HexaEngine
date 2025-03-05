namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using System.Numerics;

    public struct CubeDesc
    {
        public float Width = 1;
        public float Height = 1;
        public float Depth = 1;

        public CubeDesc()
        {
        }

        public CubeDesc(float width, float height, float depth)
        {
            Width = width;
            Height = height;
            Depth = depth;
        }
    }

    /// <summary>
    /// Represents a cube primitive in 3D space.
    /// </summary>
    public sealed class Cube : Primitive<CubeDesc, ushort>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Cube"/> class.
        /// </summary>
        public Cube(CubeDesc desc) : base(desc)
        {
        }

        /// <summary>
        /// Initializes the cube mesh with vertices and indices.
        /// </summary>
        /// <returns>
        /// A tuple containing the vertex buffer and optional index buffer of the cube mesh.
        /// </returns>
        protected override (VertexBuffer<PrimVertex>, IndexBuffer<ushort>?) InitializeMesh(CubeDesc desc)
        {
            CreateCube(out var vertexBuffer, out var indexBuffer, desc.Width, desc.Height, desc.Depth);
            return (vertexBuffer, indexBuffer);
        }

        /// <summary>
        /// Generates vertices and indices for a cube mesh.
        /// </summary>
        /// <param name="vertexBuffer">The vertex buffer of the cube mesh.</param>
        /// <param name="indexBuffer">The optional index buffer of the cube mesh.</param>
        /// <param name="width">The width of the cube.</param>
        /// <param name="height">The height of the cube.</param>
        /// <param name="depth">The depth of the cube.</param>
        public static void CreateCube(out VertexBuffer<PrimVertex> vertexBuffer, out IndexBuffer<ushort> indexBuffer, float width = 1, float height = 1, float depth = 1)
        {
            float halfWidth = width / 2;
            float halfHeight = height / 2;
            float halfDepth = depth / 2;

            vertexBuffer = new(
            [
                // Front face (Z-)
                new PrimVertex(new Vector3(-halfWidth, halfHeight, -halfDepth), uv: new Vector2(0, 0), normal: new Vector3(0, 0, -1), tangent: new Vector3(1, 0, 0)),
                new PrimVertex(new Vector3(halfWidth, halfHeight, -halfDepth), uv: new Vector2(1, 0), normal: new Vector3(0, 0, -1), tangent: new Vector3(1, 0, 0)),
                new PrimVertex(new Vector3(halfWidth, -halfHeight, -halfDepth), uv: new Vector2(1, 1), normal: new Vector3(0, 0, -1), tangent: new Vector3(1, 0, 0)),
                new PrimVertex(new Vector3(-halfWidth, -halfHeight, -halfDepth), uv: new Vector2(0, 1), normal: new Vector3(0, 0, -1), tangent: new Vector3(1, 0, 0)),

                // Back face (Z+)
                new PrimVertex(new Vector3(-halfWidth, halfHeight, halfDepth), uv: new Vector2(0, 0), normal: new Vector3(0, 0, 1), tangent: new Vector3(-1, 0, 0)),
                new PrimVertex(new Vector3(-halfWidth, -halfHeight, halfDepth), uv: new Vector2(0, 1), normal: new Vector3(0, 0, 1), tangent: new Vector3(-1, 0, 0)),
                new PrimVertex(new Vector3(halfWidth, -halfHeight, halfDepth), uv: new Vector2(1, 1), normal: new Vector3(0, 0, 1), tangent: new Vector3(-1, 0, 0)),
                new PrimVertex(new Vector3(halfWidth, halfHeight, halfDepth), uv: new Vector2(1, 0), normal: new Vector3(0, 0, 1), tangent: new Vector3(-1, 0, 0)),

                // Left face (X-)
                new PrimVertex(new Vector3(-halfWidth, halfHeight, halfDepth), uv: new Vector2(0, 0), normal: new Vector3(-1, 0, 0), tangent: new Vector3(0, 0, 1)),
                new PrimVertex(new Vector3(-halfWidth, halfHeight, -halfDepth), uv: new Vector2(1, 0), normal: new Vector3(-1, 0, 0), tangent: new Vector3(0, 0, 1)),
                new PrimVertex(new Vector3(-halfWidth, -halfHeight, -halfDepth), uv: new Vector2(1, 1), normal: new Vector3(-1, 0, 0), tangent: new Vector3(0, 0, 1)),
                new PrimVertex(new Vector3(-halfWidth, -halfHeight, halfDepth), uv: new Vector2(0, 1), normal: new Vector3(-1, 0, 0), tangent: new Vector3(0, 0, 1)),

                // Right face (X+)
                new PrimVertex(new Vector3(halfWidth, halfHeight, -halfDepth), uv: new Vector2(0, 0), normal: new Vector3(1, 0, 0), tangent: new Vector3(0, 0, -1)),
                new PrimVertex(new Vector3(halfWidth, -halfHeight, -halfDepth), uv: new Vector2(0, 1), normal: new Vector3(1, 0, 0), tangent: new Vector3(0, 0, -1)),
                new PrimVertex(new Vector3(halfWidth, -halfHeight, halfDepth), uv: new Vector2(1, 1), normal: new Vector3(1, 0, 0), tangent: new Vector3(0, 0, -1)),
                new PrimVertex(new Vector3(halfWidth, halfHeight, halfDepth), uv: new Vector2(1, 0), normal: new Vector3(1, 0, 0), tangent: new Vector3(0, 0, -1)),

                // Top face (Y+)
                new PrimVertex(new Vector3(-halfWidth, halfHeight, -halfDepth), uv: new Vector2(0, 0), normal: new Vector3(0, 1, 0), tangent: new Vector3(1, 0, 0)),
                new PrimVertex(new Vector3(-halfWidth, halfHeight, halfDepth), uv: new Vector2(0, 1), normal: new Vector3(0, 1, 0), tangent: new Vector3(1, 0, 0)),
                new PrimVertex(new Vector3(halfWidth, halfHeight, halfDepth), uv: new Vector2(1, 1), normal: new Vector3(0, 1, 0), tangent: new Vector3(1, 0, 0)),
                new PrimVertex(new Vector3(halfWidth, halfHeight, -halfDepth), uv: new Vector2(1, 0), normal: new Vector3(0, 1, 0), tangent: new Vector3(1, 0, 0)),

                // Bottom face (Y-)
                new PrimVertex(new Vector3(-halfWidth, -halfHeight, -halfDepth), uv: new Vector2(0, 0), normal: new Vector3(0, -1, 0), tangent: new Vector3(1, 0, 0)),
                new PrimVertex(new Vector3(halfWidth, -halfHeight, -halfDepth), uv: new Vector2(1, 0), normal: new Vector3(0, -1, 0), tangent: new Vector3(1, 0, 0)),
                new PrimVertex(new Vector3(halfWidth, -halfHeight, halfDepth), uv: new Vector2(1, 1), normal: new Vector3(0, -1, 0), tangent: new Vector3(1, 0, 0)),
                new PrimVertex(new Vector3(-halfWidth, -halfHeight, halfDepth), uv: new Vector2(0, 1), normal: new Vector3(0, -1, 0), tangent: new Vector3(1, 0, 0)),
            ], CpuAccessFlags.None);

            indexBuffer = new(
            [
                0, 1, 2, 2, 3, 0,  // Front (Z-)
                4, 5, 6, 6, 7, 4,  // Back (Z+)
                8, 9, 10, 10, 11, 8,  // Left (X-)
                12, 14, 13, 14, 12, 15,  // Right (X+)
                16, 17, 18, 18, 19, 16,  // Top (Y+)
                20, 21, 22, 22, 23, 20   // Bottom (Y-)
            ], CpuAccessFlags.None);
        }
    }
}