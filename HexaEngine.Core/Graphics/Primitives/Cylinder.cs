namespace HexaEngine.Core.Graphics.Primitives
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics.Buffers;
    using System;
    using System.Numerics;

    public struct CylinderDesc
    {
        public float Height = 1;
        public float Diameter = 1;
        public uint Tesselation = 32;

        public CylinderDesc()
        {
        }

        public CylinderDesc(float height, float diameter, uint tesselation = 32)
        {
            Height = height;
            Diameter = diameter;
            Tesselation = tesselation;
        }
    }

    /// <summary>
    /// Represents a cylinder primitive in 3D space.
    /// </summary>
    public sealed class Cylinder : Primitive<CylinderDesc, uint>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Cylinder"/> class.
        /// </summary>
        public Cylinder(CylinderDesc desc) : base(desc)
        {
        }

        /// <summary>
        /// Initializes the cylinder mesh with vertices and indices.
        /// </summary>
        /// <returns>
        /// A tuple containing the vertex buffer and optional index buffer of the cylinder mesh.
        /// </returns>
        protected override (VertexBuffer<PrimVertex>, IndexBuffer<uint>?) InitializeMesh(CylinderDesc desc)
        {
            CreateCylinder(out VertexBuffer<PrimVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer, desc.Height, desc.Diameter, desc.Tesselation);
            return (vertexBuffer, indexBuffer);
        }

        /// <summary>
        /// Generates vertices and indices for a cylinder mesh.
        /// </summary>

        /// <param name="vertexBuffer">The vertex buffer of the cylinder mesh.</param>
        /// <param name="indexBuffer">The optional index buffer of the cylinder mesh.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="diameter">The diameter of the cylinder.</param>
        /// <param name="tessellation">The number of subdivisions around the cylinder.</param>
        public static void CreateCylinder(out VertexBuffer<PrimVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer, float height = 1, float diameter = 1, uint tessellation = 32)
        {
            if (tessellation < 3)
            {
                throw new ArgumentException("tesselation parameter must be at least 3");
            }

            height /= 2;

            Vector3 topOffset = new(0, height, 0);

            float radius = diameter / 2;
            uint stride = tessellation + 1;

            PrimVertex[] vertices = new PrimVertex[stride * 2 + tessellation * 2];
            uint[] indices = new uint[stride * 6 + (tessellation - 2) * 3 * 2];

            uint vcounter = 0;
            uint icounter = 0;
            for (uint i = 0; i <= tessellation; i++)
            {
                Vector3 normal = GetCircleVector(i, tessellation);

                Vector3 sideOffset = normal * radius;

                float u = (float)i / tessellation;

                Vector3 tangent = GetCircleTangent(i, tessellation);

                vertices[vcounter] = new(sideOffset + topOffset, new Vector2(u, 0), normal, tangent);
                vertices[vcounter + 1] = new(sideOffset - topOffset, new Vector2(u, 1), normal, tangent);
                vcounter += 2;

                indices[icounter] = i * 2;
                indices[icounter + 1] = (i * 2 + 2) % (stride * 2);
                indices[icounter + 2] = i * 2 + 1;

                indices[icounter + 3] = i * 2 + 1;
                indices[icounter + 4] = (i * 2 + 2) % (stride * 2);
                indices[icounter + 5] = (i * 2 + 2) % (stride * 2);
                icounter += 6;
            }

            CreateCylinderCap(vertices, indices, ref vcounter, ref icounter, tessellation, height, radius, true);
            CreateCylinderCap(vertices, indices, ref vcounter, ref icounter, tessellation, height, radius, false);

            vertexBuffer = new VertexBuffer<PrimVertex>(vertices, CpuAccessFlags.None);
            indexBuffer = new IndexBuffer<uint>(indices, CpuAccessFlags.None);
        }

        private static Vector3 GetCircleVector(uint i, uint tessellation)
        {
            float angle = i * MathUtil.PI2 / tessellation;
            float dx = MathF.Sin(angle), dz = MathF.Cos(angle);

            return new(dx, 0, dz);
        }

        private static Vector3 GetCircleTangent(uint i, uint tessellation)
        {
            float angle = i * MathUtil.PI2 / tessellation + MathUtil.PIDIV2;
            float dx = MathF.Sin(angle), dz = MathF.Cos(angle);

            return new(dx, 0, dz);
        }

        private static void CreateCylinderCap(PrimVertex[] vertices, uint[] indices, ref uint vcounter, ref uint icounter, uint tessellation, float height, float radius, bool isTop)
        {
            // Create cap indices.
            for (uint i = 0; i < tessellation - 2; i++)
            {
                uint i1 = (i + 1) % tessellation;
                uint i2 = (i + 2) % tessellation;

                if (isTop)
                {
                    (i2, i1) = (i1, i2);
                }

                indices[icounter] = vcounter;
                indices[icounter + 1] = vcounter + i1;
                indices[icounter + 2] = vcounter + i2;
                icounter += 3;
            }

            // Which end of the cylinder is this?
            Vector3 normal = new(0, 1, 0);
            Vector3 tangent = Vector3.UnitX;
            Vector2 textureScale = new(-0.5f, -0.5f);

            if (!isTop)
            {
                normal = -normal;
                textureScale *= new Vector2(-1, 1);
            }

            // Create cap vertices.
            for (uint i = 0; i < tessellation; i++)
            {
                Vector3 circleVector = GetCircleVector(i, tessellation);

                Vector3 position = circleVector * radius + normal * height;

                Vector2 textureCoordinate = new Vector2(circleVector.X, circleVector.Z) * textureScale + new Vector2(0.5f);

                vertices[vcounter++] = new(position, textureCoordinate, normal, tangent);
            }
        }
    }
}