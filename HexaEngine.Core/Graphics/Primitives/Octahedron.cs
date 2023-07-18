namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Meshes;
    using System.Numerics;

    public class Octahedron : Primitive<MeshVertex, uint>
    {
        public Octahedron(IGraphicsDevice device) : base(device)
        {
        }

        protected override (VertexBuffer<MeshVertex>, IndexBuffer<uint>?) InitializeMesh(IGraphicsDevice device)
        {
            CreateOctahedron(device, out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer);
            return (vertexBuffer, indexBuffer);
        }

        private static readonly Vector3[] verts =
        {
            new(1,  0,  0),
            new(-1,  0,  0),
            new(0,  1,  0),
            new(0, -1,  0),
            new(0,  0,  1),
            new(0,  0, -1),
        };

        private static readonly uint[] faces =
        {
            4, 0, 2,
            4, 2, 1,
            4, 1, 3,
            4, 3, 0,
            5, 2, 0,
            5, 1, 2,
            5, 3, 1,
            5, 0, 3
        };

        public static void CreateOctahedron(IGraphicsDevice device, out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer, float size = 1)
        {
            MeshVertex[] vertices = new MeshVertex[8 * 3];
            uint[] indices = new uint[8 * 3];

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