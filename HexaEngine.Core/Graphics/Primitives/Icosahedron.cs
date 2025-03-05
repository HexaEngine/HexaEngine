namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using System.Numerics;

    public struct IcosahedronDesc
    {
        public float Size = 1;

        public IcosahedronDesc()
        {
        }

        public IcosahedronDesc(float size)
        {
            Size = size;
        }
    }

    /// <summary>
    /// Represents an icosahedron primitive in 3D space.
    /// </summary>
    public sealed class Icosahedron : Primitive<IcosahedronDesc, uint>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Icosahedron"/> class.
        /// </summary>
        public Icosahedron(IcosahedronDesc desc) : base(desc)
        {
        }

        /// <summary>
        /// Initializes the icosahedron mesh with vertices and indices.
        /// </summary>
        /// <returns>
        /// A tuple containing the vertex buffer and optional index buffer of the icosahedron mesh.
        /// </returns>
        protected override (VertexBuffer<PrimVertex>, IndexBuffer<uint>?) InitializeMesh(IcosahedronDesc desc)
        {
            CreateIcosahedron(out VertexBuffer<PrimVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer, desc.Size);
            return (vertexBuffer, indexBuffer);
        }

        private const float t = 1.618033988749894848205f; // (1 + sqrt(5)) / 2
        private const float t2 = 1.519544995837552493271f; // sqrt( 1 + sqr( (1 + sqrt(5)) / 2 ) )

        private static readonly Vector3[] verts =
         [
                new(t / t2, 1.0f / t2, 0),
                new(-t / t2, 1.0f / t2, 0),
                new(t / t2, -1.0f / t2, 0),
                new(-t / t2, -1.0f / t2, 0),
                new(1.0f / t2, 0, t / t2),
                new(1.0f / t2, 0, -t / t2),
                new(-1.0f / t2, 0, t / t2),
                new(-1.0f / t2, 0, -t / t2),
                new(0, t / t2, 1.0f / t2),
                new(0, -t / t2, 1.0f / t2),
                new(0, t / t2, -1.0f / t2),
                new(0, -t / t2, -1.0f / t2)
        ];

        private static readonly uint[] faces =
        [
                0, 8, 4,
                0, 5, 10,
                2, 4, 9,
                2, 11, 5,
                1, 6, 8,
                1, 10, 7,
                3, 9, 6,
                3, 7, 11,
                0, 10, 8,
                1, 8, 10,
                2, 9, 11,
                3, 11, 9,
                4, 2, 0,
                5, 0, 2,
                6, 1, 3,
                7, 3, 1,
                8, 6, 4,
                9, 4, 6,
                10, 5, 7,
                11, 7, 5
        ];

        /// <summary>
        /// Generates vertices and indices for an icosahedron mesh.
        /// </summary>
        /// <param name="vertexBuffer">The vertex buffer of the icosahedron mesh.</param>
        /// <param name="indexBuffer">The optional index buffer of the icosahedron mesh.</param>
        /// <param name="size">The size of the icosahedron.</param>
        public static void CreateIcosahedron(out VertexBuffer<PrimVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer, float size = 1)
        {
            PrimVertex[] vertices = new PrimVertex[20 * 3];
            uint[] indices = new uint[20 * 3];

            uint vcounter = 0;
            uint icounter = 0;
            for (uint j = 0; j < indices.Length; j += 3)
            {
                uint v0 = indices[j + 0];
                uint v1 = indices[j + 1];
                uint v2 = indices[j + 2];

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

                uint vbase = vcounter;
                indices[icounter] = vbase;
                indices[icounter + 1] = vbase + 1;
                indices[icounter + 2] = vbase + 2;
                icounter += 3;

                vertices[vcounter] = new(verts[v0] * size, Vector2.Zero, normal, tangent);
                vertices[vcounter + 1] = new(verts[v1] * size, Vector2.UnitX, normal, tangent);
                vertices[vcounter + 2] = new(verts[v2] * size, Vector2.UnitY, normal, tangent);
                vcounter += 3;
            }

            vertexBuffer = new VertexBuffer<PrimVertex>(vertices, CpuAccessFlags.None);
            indexBuffer = new IndexBuffer<uint>(indices, CpuAccessFlags.None);
        }
    }
}