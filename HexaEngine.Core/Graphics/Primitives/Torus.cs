namespace HexaEngine.Core.Graphics.Primitives
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics.Buffers;
    using System;
    using System.Numerics;

    public struct TorusDesc
    {
        public float Diameter = 1;
        public float Thickness = 0.333f;
        public uint Tesselation = 32;

        public TorusDesc()
        {
        }

        public TorusDesc(float diameter, float thickness, uint tesselation = 32)
        {
            Diameter = diameter;
            Thickness = thickness;
            Tesselation = tesselation;
        }
    }

    /// <summary>
    /// Represents a 3D torus primitive.
    /// </summary>
    public sealed class Torus : Primitive<TorusDesc, uint>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Torus"/> class.
        /// </summary>
        public Torus(TorusDesc desc) : base(desc)
        {
        }

        /// <summary>
        /// Initializes the torus mesh by creating vertex and index buffers.
        /// </summary>
        /// <returns>A tuple containing the vertex buffer and an optional index buffer.</returns>
        protected override (VertexBuffer<PrimVertex>, IndexBuffer<uint>?) InitializeMesh(TorusDesc desc)
        {
            CreateTorus(out VertexBuffer<PrimVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer, desc.Diameter, desc.Thickness, desc.Tesselation);
            return (vertexBuffer, indexBuffer);
        }

        /// <summary>
        /// Creates a torus mesh with the specified parameters.
        /// </summary>
        /// <param name="vertexBuffer">The vertex buffer of the torus.</param>
        /// <param name="indexBuffer">The index buffer of the torus.</param>
        /// <param name="diameter">The diameter of the torus.</param>
        /// <param name="thickness">The thickness of the torus.</param>
        /// <param name="tessellation">The tessellation level of the torus.</param>
        public static void CreateTorus(out VertexBuffer<PrimVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer, float diameter = 1, float thickness = 0.333f, uint tessellation = 32)
        {
            if (tessellation < 3)
            {
                throw new ArgumentException("tesselation parameter must be at least 3");
            }

            uint stride = tessellation + 1;

            PrimVertex[] vertices = new PrimVertex[stride * stride];
            uint[] indices = new uint[stride * stride * 6];

            uint vcounter = 0;
            uint icounter = 0;
            for (uint i = 0; i <= tessellation; i++)
            {
                float u = i / tessellation;
                float outerAngle = i * MathUtil.PI2 / tessellation - MathUtil.PIDIV2;

                Matrix4x4 transform = Matrix4x4.CreateTranslation(diameter / 2, 0, 0) * Matrix4x4.CreateRotationY(outerAngle);

                for (uint j = 0; j <= tessellation; j++)
                {
                    float v = 1 - j / tessellation;
                    float innerAngle = j * MathUtil.PI2 / tessellation + MathUtil.PI;
                    float dx = MathF.Sin(innerAngle), dy = MathF.Cos(innerAngle);

                    // Create a vertex.
                    Vector3 normal = new(dx, dy, 0);
                    Vector3 tangent = new(-dy, dx, 0);
                    Vector3 position = normal * (thickness / 2);

                    position = Vector3.Transform(position, transform);
                    normal = Vector3.TransformNormal(normal, transform);
                    tangent = Vector3.TransformNormal(tangent, transform);

                    if (i == tessellation) u = 0;
                    if (j == tessellation) v = 0;

                    vertices[vcounter++] = new(position, new Vector2(u, v), normal, tangent);

                    // And create indices for two triangles.
                    uint nextI = (i + 1) % stride;
                    uint nextJ = (j + 1) % stride;

                    indices[icounter + 0] = i * stride + j;
                    indices[icounter + 1] = i * stride + nextJ;
                    indices[icounter + 2] = nextI * stride + j;

                    indices[icounter + 3] = i * stride + nextJ;
                    indices[icounter + 4] = nextI * stride + nextJ;
                    indices[icounter + 5] = nextI * stride + j;
                    icounter += 6;
                }
            }

            vertexBuffer = new VertexBuffer<PrimVertex>(vertices, CpuAccessFlags.None);
            indexBuffer = new IndexBuffer<uint>(indices, CpuAccessFlags.None);
        }
    }
}