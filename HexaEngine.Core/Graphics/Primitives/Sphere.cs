namespace HexaEngine.Core.Graphics.Primitives
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO;
    using System;
    using System.Numerics;

    /// <summary>
    /// Represents a 3D sphere primitive.
    /// </summary>
    public sealed class Sphere : Primitive<MeshVertex, uint>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sphere"/> class.
        /// </summary>
        public Sphere() : base()
        {
        }

        /// <summary>
        /// Initializes the sphere mesh by creating vertex and index buffers.
        /// </summary>
        /// <returns>A tuple containing the vertex buffer and an optional index buffer.</returns>
        protected override (VertexBuffer<MeshVertex>, IndexBuffer<uint>?) InitializeMesh()
        {
            CreateSphere(out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer);
            return (vertexBuffer, indexBuffer);
        }

        /// <summary>
        /// Creates a sphere mesh with the specified parameters.
        /// </summary>
        /// <param name="vertexBuffer">The vertex buffer of the sphere.</param>
        /// <param name="indexBuffer">The index buffer of the sphere.</param>
        /// <param name="diameter">The diameter of the sphere.</param>
        /// <param name="tessellation">The level of tessellation for the sphere.</param>
        /// <param name="invertn">A flag indicating whether to invert the normals of the sphere.</param>
        public static void CreateSphere(out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer, float diameter = 1, uint tessellation = 16, bool invertn = false)
        {
            if (tessellation < 3)
            {
                throw new ArgumentException("tesselation parameter must be at least 3");
            }

            uint verticalSegments = tessellation;
            uint horizontalSegments = tessellation * 2;

            MeshVertex[] vertices = new MeshVertex[(verticalSegments + 1) * (horizontalSegments + 1)];
            uint[] indices = new uint[verticalSegments * (horizontalSegments + 1) * 6];

            float radius = diameter / 2;

            uint vcounter = 0;
            for (uint i = 0; i <= verticalSegments; i++)
            {
                float v = 1 - (float)i / verticalSegments;
                float latitude = i * MathUtil.PI / verticalSegments - MathUtil.PIDIV2;

                float dy = MathF.Sin(latitude), dxz = MathF.Cos(latitude);

                for (uint j = 0; j <= horizontalSegments; j++)
                {
                    float u = (float)j / horizontalSegments;
                    float longitude = j * MathUtil.PI2 / horizontalSegments;
                    float dx = MathF.Sin(longitude), dz = MathF.Cos(longitude);

                    dx *= dxz;
                    dz *= dxz;

                    Vector3 normal = new(dx, dy, dz);

                    Vector3 tangent;
                    if (Vector3.Dot(Vector3.UnitY, normal) == 1.0f)
                    {
                        tangent = Vector3.UnitX;
                    }
                    else
                    {
                        tangent = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, normal));
                    }

                    Vector2 textureCoordinate = new(u, v);

                    vertices[vcounter++] = new(normal * radius, textureCoordinate, normal, tangent);
                }
            }

            uint stride = horizontalSegments + 1;
            uint icounter = 0;
            for (uint i = 0; i < verticalSegments; i++)
            {
                for (uint j = 0; j <= horizontalSegments; j++)
                {
                    uint nextI = i + 1;
                    uint nextJ = (j + 1) % stride;

                    indices[icounter + 0] = (i * stride + j);
                    indices[icounter + 1] = (nextI * stride + j);
                    indices[icounter + 2] = (i * stride + nextJ);

                    indices[icounter + 3] = (i * stride + nextJ);
                    indices[icounter + 4] = (nextI * stride + j);
                    indices[icounter + 5] = (nextI * stride + nextJ);
                    icounter += 6;
                }
            }

            if (invertn)
            {
                for (uint i = 0; i < vertices.Length; i++)
                {
                    vertices[i].Normal = -vertices[i].Normal;
                }
            }

            vertexBuffer = new(vertices, CpuAccessFlags.None);
            indexBuffer = new(indices, CpuAccessFlags.None);
        }
    }
}