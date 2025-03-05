namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using System.Numerics;

    public struct QuadDesc
    {
        public float Size = 1;

        public QuadDesc()
        {
        }

        public QuadDesc(float size)
        {
            Size = size;
        }
    }

    /// <summary>
    /// Represents a simple quad in 3D space.
    /// </summary>
    public sealed class Quad : Primitive<QuadDesc, ushort>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Quad"/> class.
        /// </summary>
        public Quad(QuadDesc desc) : base(desc)
        {
        }

        protected override (VertexBuffer<PrimVertex>, IndexBuffer<ushort>?) InitializeMesh(QuadDesc desc)
        {
            CreateQuad(out var vertexBuffer, out var indexBuffer, desc.Size);
            return (vertexBuffer, indexBuffer);
        }

        /// <summary>
        /// Creates a plane mesh.
        /// </summary>
        /// <param name="vertexBuffer">The vertex buffer.</param>
        /// <param name="indexBuffer">The index buffer.</param>
        /// <param name="size">The size of the plane.</param>
        public static void CreateQuad(out VertexBuffer<PrimVertex> vertexBuffer, out IndexBuffer<ushort> indexBuffer, float size = 1)
        {
            vertexBuffer = new(
            [
                new(new(-1 * size, 1 * size, 0), new Vector2(0, 0), new(0, 0, -1), new(1, 0, 0)),
                new(new(-1 * size, -1 * size, 0), new Vector2(0, 1), new(0, 0, -1), new(1, 0, 0)),
                new(new(1 * size, 1 * size, 0), new Vector2(1, 0), new(0, 0, -1), new(1, 0, 0)),
                new(new(1 * size, -1 * size, 0), new Vector2(1, 1), new(0, 0, -1), new(1, 0, 0))
            ], CpuAccessFlags.None);

            indexBuffer = new(
            [
                0,
                3,
                1,
                0,
                2,
                3
            ], CpuAccessFlags.None);
        }
    }
}