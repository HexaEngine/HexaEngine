namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;

    public class Cylinder : Primitive<MeshVertex, uint>
    {
        public Cylinder(IGraphicsDevice device) : base(device)
        {
        }

        protected override (VertexBuffer<MeshVertex>, IndexBuffer<uint>?) InitializeMesh(IGraphicsDevice device)
        {
            CreateCylinder(device, out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer);
            return (vertexBuffer, indexBuffer);
        }

        public static void CreateCylinder(IGraphicsDevice device, out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer, float height = 1, float diameter = 1, uint tessellation = 32)
        {
            if (tessellation < 3)
                throw new ArgumentException("tesselation parameter must be at least 3");

            height /= 2;

            Vector3 topOffset = new(0, height, 0);

            float radius = diameter / 2;
            uint stride = tessellation + 1;

            MeshVertex[] vertices = new MeshVertex[stride * 2 + tessellation * 2];
            uint[] indices = new uint[stride * 6 + (tessellation - 2) * 3 * 2];

            uint vcounter = 0;
            uint icounter = 0;
            for (uint i = 0; i <= tessellation; i++)
            {
                Vector3 normal = GetCircleVector(i, tessellation);

                Vector3 sideOffset = normal * radius;

                float u = (float)i / tessellation;

                Vector3 tangent = GetCircleTangent(i, tessellation);
                Vector3 bitangent = Vector3.Cross(normal, tangent);

                vertices[vcounter] = new(sideOffset + topOffset, new Vector2(u, 0), normal, tangent, bitangent);
                vertices[vcounter + 1] = new(sideOffset - topOffset, new Vector2(u, 1), normal, tangent, bitangent);
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

            vertexBuffer = new VertexBuffer<MeshVertex>(device, vertices, CpuAccessFlags.None);
            indexBuffer = new IndexBuffer<uint>(device, indices, CpuAccessFlags.None);
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

        private static void CreateCylinderCap(MeshVertex[] vertices, uint[] indices, ref uint vcounter, ref uint icounter, uint tessellation, float height, float radius, bool isTop)
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
            Vector3 textureScale = new(-0.5f, -0.5f, -0.5f);

            if (!isTop)
            {
                normal = -normal;
                textureScale *= new Vector3(-1, 1, 1);
            }

            Vector3 bitangent = Vector3.Cross(normal, tangent);

            // Create cap vertices.
            for (uint i = 0; i < tessellation; i++)
            {
                Vector3 circleVector = GetCircleVector(i, tessellation);

                Vector3 position = circleVector * radius + normal * height;

                Vector3 textureCoordinate = new Vector3(circleVector.X, circleVector.Z, 0) * textureScale + new Vector3(0.5f);

                vertices[vcounter++] = new(position, textureCoordinate, normal, tangent, bitangent);
            }
        }
    }
}