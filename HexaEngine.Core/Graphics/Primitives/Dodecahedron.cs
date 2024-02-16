namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.Numerics;

    /// <summary>
    /// Represents a dodecahedron primitive in 3D space.
    /// </summary>
    public sealed class Dodecahedron : Primitive<MeshVertex, uint>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Dodecahedron"/> class.
        /// </summary>
        /// <param name="device">The graphics device used for mesh creation.</param>
        public Dodecahedron(IGraphicsDevice device) : base(device)
        {
        }

        /// <summary>
        /// Initializes the dodecahedron mesh with vertices and indices.
        /// </summary>
        /// <param name="device">The graphics device used for mesh creation.</param>
        /// <returns>
        /// A tuple containing the vertex buffer and optional index buffer of the dodecahedron mesh.
        /// </returns>
        protected override (VertexBuffer<MeshVertex>, IndexBuffer<uint>?) InitializeMesh(IGraphicsDevice device)
        {
            CreateDodecahedron(device, out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer);
            return (vertexBuffer, indexBuffer);
        }

        private const float a = 1.0f / MathUtil.SQRT3;
        private const float b = 0.356822089773089931942f; // sqrt( ( 3 - sqrt(5) ) / 6 )
        private const float c = 0.934172358962715696451f; // sqrt( ( 3 + sqrt(5) ) / 6 );

        private static readonly Vector3[] verts =
        {
            new(  a,  a,  a),
            new(  a,  a, -a),
            new(  a, -a,  a),
            new(  a, -a, -a),
            new( -a,  a,  a),
            new( -a,  a, -a),
            new( -a, -a,  a),
            new( -a, -a, -a),
            new(  b,  c,  0),
            new( -b,  c,  0),
            new(  b, -c,  0),
            new( -b, -c,  0),
            new(  c,  0,  b),
            new(  c,  0, -b),
            new( -c,  0,  b),
            new( -c,  0, -b),
            new(  0,  b,  c),
            new(  0, -b,  c),
            new(  0,  b, -c),
            new(  0, -b, -c),
        };

        private static readonly uint[] faces =
        {
            0, 8, 9, 4, 16,
            0, 16, 17, 2, 12,
            12, 2, 10, 3, 13,
            9, 5, 15, 14, 4,
            3, 19, 18, 1, 13,
            7, 11, 6, 14, 15,
            0, 12, 13, 1, 8,
            8, 1, 18, 5, 9,
            16, 4, 14, 6, 17,
            6, 11, 10, 2, 17,
            7, 15, 5, 18, 19,
            7, 19, 3, 10, 11,
        };

        private static readonly Vector2[] textureCoordinates =
        {
            new(  0.654508f, 0.0244717f),
            new( 0.0954915f,  0.206107f),
            new( 0.0954915f,  0.793893f),
            new(  0.654508f,  0.975528f),
            new(       1.0f,       0.5f)
        };

        private static readonly uint[][] textureIndex =
        {
            new uint[] { 0, 1, 2, 3, 4 },
            new uint[] { 2, 3, 4, 0, 1 },
            new uint[] { 4, 0, 1, 2, 3 },
            new uint[] { 1, 2, 3, 4, 0 },
            new uint[] { 2, 3, 4, 0, 1 },
            new uint[] { 0, 1, 2, 3, 4 },
            new uint[] { 1, 2, 3, 4, 0 },
            new uint[] { 4, 0, 1, 2, 3 },
            new uint[] { 4, 0, 1, 2, 3 },
            new uint[] { 1, 2, 3, 4, 0 },
            new uint[] { 0, 1, 2, 3, 4 },
            new uint[] { 2, 3, 4, 0, 1 },
        };

        /// <summary>
        /// Generates vertices and indices for a dodecahedron mesh.
        /// </summary>
        /// <param name="device">The graphics device used for mesh creation.</param>
        /// <param name="vertexBuffer">The vertex buffer of the dodecahedron mesh.</param>
        /// <param name="indexBuffer">The optional index buffer of the dodecahedron mesh.</param>
        /// <param name="size">The size of the dodecahedron.</param>
        public static void CreateDodecahedron(IGraphicsDevice device, out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer, float size = 1)
        {
            MeshVertex[] vertices = new MeshVertex[5 * 12];
            uint[] indices = new uint[9 * 12];

            uint vcounter = 0;
            uint icounter = 0;

            uint t = 0;
            for (int j = 0; j < faces.Length; j += 5, ++t)
            {
                uint v0 = faces[j];
                uint v1 = faces[j + 1];
                uint v2 = faces[j + 2];
                uint v3 = faces[j + 3];
                uint v4 = faces[j + 4];

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

                indices[icounter + 3] = vbase;
                indices[icounter + 4] = vbase + 2;
                indices[icounter + 5] = vbase + 3;

                indices[icounter + 6] = vbase;
                indices[icounter + 7] = vbase + 3;
                indices[icounter + 8] = vbase + 4;
                icounter += 9;

                vertices[vcounter + 0] = new(verts[v0] * size, textureCoordinates[textureIndex[t][0]], normal, tangent);
                vertices[vcounter + 1] = new(verts[v1] * size, textureCoordinates[textureIndex[t][0]], normal, tangent);
                vertices[vcounter + 2] = new(verts[v2] * size, textureCoordinates[textureIndex[t][0]], normal, tangent);
                vertices[vcounter + 3] = new(verts[v3] * size, textureCoordinates[textureIndex[t][0]], normal, tangent);
                vertices[vcounter + 4] = new(verts[v4] * size, textureCoordinates[textureIndex[t][0]], normal, tangent);
                vcounter += 5;
            }

            vertexBuffer = new(device, vertices, CpuAccessFlags.None);
            indexBuffer = new(device, indices, CpuAccessFlags.None);
        }
    }
}