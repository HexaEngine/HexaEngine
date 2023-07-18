namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Mathematics;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Text;
    using System.Threading.Tasks;

    public class Tetrahedron : Primitive<MeshVertex, uint>
    {
        public Tetrahedron(IGraphicsDevice device) : base(device)
        {
        }

        protected override (VertexBuffer<MeshVertex>, IndexBuffer<uint>?) InitializeMesh(IGraphicsDevice device)
        {
            CreateTetrahedron(device, out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer);
            return (vertexBuffer, indexBuffer);
        }

        private static readonly Vector3[] verts =
        {
            new(0.0f, 0.0f, 1.0f),
            new(2.0f * MathUtil.SQRT2 / 3.0f, 0.0f, -1.0f / 3.0f),
            new(-MathUtil.SQRT2 / 3.0f, MathUtil.SQRT6 / 3.0f, -1.0f / 3.0f),
            new(-MathUtil.SQRT2 / 3.0f, -MathUtil.SQRT6 / 3.0f, -1.0f / 3.0f),
        };

        private static readonly uint[] faces =
        {
            0, 1, 2,
            0, 2, 3,
            0, 3, 1,
            1, 3, 2,
        };

        private void CreateTetrahedron(IGraphicsDevice device, out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer, float size = 1)
        {
            MeshVertex[] vertices = new MeshVertex[4 * 3];
            uint[] indices = new uint[4 * 3];

            uint vcounter = 0;
            uint icounter = 0;

            for (uint j = 0; j < faces.Length; j += 3)
            {
                uint v0 = faces[j];
                uint v1 = faces[j + 1];
                uint v2 = faces[j + 2];

                Vector3 normal = Vector3.Cross(verts[v1] - verts[v0], verts[v2] - verts[v0]);
                normal = Vector3.Normalize(normal);

                Vector3 tangent;
                if (Vector3.Dot(Vector3.UnitY, normal) == 1.0f)
                {
                    tangent = Vector3.UnitX;
                }
                else
                {
                    tangent = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, normal));
                }

                Vector3 bitangent = Vector3.Cross(normal, tangent);

                uint vbase = vcounter;
                indices[icounter] = vbase;
                indices[icounter + 1] = vbase + 1;
                indices[icounter + 2] = vbase + 2;
                icounter += 3;

                vertices[vcounter] = new(verts[v0] * size, Vector2.Zero, normal, tangent, bitangent);
                vertices[vcounter + 1] = new(verts[v1] * size, Vector2.UnitX, normal, tangent, bitangent);
                vertices[vcounter + 2] = new(verts[v2] * size, Vector2.UnitY, normal, tangent, bitangent);
                vcounter += 3;
            }

            vertexBuffer = new VertexBuffer<MeshVertex>(device, vertices, CpuAccessFlags.None);
            indexBuffer = new IndexBuffer<uint>(device, indices, CpuAccessFlags.None);
        }
    }
}