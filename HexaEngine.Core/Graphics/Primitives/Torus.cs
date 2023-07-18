namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;

    internal class Torus : Primitive<MeshVertex, uint>
    {
        public Torus(IGraphicsDevice device) : base(device)
        {
        }

        protected override (VertexBuffer<MeshVertex>, IndexBuffer<uint>?) InitializeMesh(IGraphicsDevice device)
        {
            CreateTorus(device, out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer);
            return (vertexBuffer, indexBuffer);
        }

        public static void CreateTorus(IGraphicsDevice device, out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer, float diameter = 1, float thickness = 0.333f, uint tessellation = 32)
        {
            if (tessellation < 3)
                throw new ArgumentException("tesselation parameter must be at least 3");

            uint stride = tessellation + 1;

            MeshVertex[] vertices = new MeshVertex[stride * stride];
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

                    Vector3 bitangent = Vector3.Cross(normal, tangent);

                    vertices[vcounter++] = new(position, new Vector3(u, v, 0), normal, tangent, bitangent);

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

            vertexBuffer = new VertexBuffer<MeshVertex>(device, vertices, CpuAccessFlags.None);
            indexBuffer = new IndexBuffer<uint>(device, indices, CpuAccessFlags.None);
        }
    }
}