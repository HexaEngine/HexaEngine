namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Meshes;
    using System;
    using System.Numerics;

    public class Sphere : Primitive<MeshVertex>
    {
        public Sphere(IGraphicsDevice device) : base(device)
        {
        }

        protected override (VertexBuffer<MeshVertex>, IndexBuffer?) InitializeMesh(IGraphicsDevice device)
        {
            CreateSphere(device, out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer indexBuffer);
            return (vertexBuffer, indexBuffer);
        }

        public static void CreateSphere(IGraphicsDevice device, out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer indexBuffer, uint LatLines = 20, uint LongLines = 20)
        {
            float radius = 1;
            float x, y, z;                              // vertex position
            float nx, ny, nz, lengthInv = 1.0f / radius;    // vertex normal
            float s, t;                                     // vertex texCoord

            float deltaLatitude = MathF.PI / LatLines;
            float deltaLongitude = 2 * MathF.PI / LongLines;
            float longitudeAngle, latitudeAngle;

            MeshVertex[] vertices = new MeshVertex[(LatLines + 1) * (LongLines + 1)];
            //vertices = new Vertex[LatLines * LongLines];
            int vi = 0;
            for (int i = 0; i <= LatLines; ++i)
            {
                latitudeAngle = i * deltaLatitude;        // starting from pi/2 to -pi/2
                float latSin = radius * MathF.Sin(latitudeAngle);             // r * sin(u)
                float latCos = radius * MathF.Cos(latitudeAngle);              // r * cos(u)

                // add (sectorCount+1) vertices per stack
                // the first and last vertices have same position and normal, but different tex coords
                for (int j = 0; j <= LongLines; ++j)
                {
                    MeshVertex v = new();
                    longitudeAngle = j * deltaLongitude;           // starting from 0 to 2pi

                    // vertex position (x, y, z)
                    x = latSin * MathF.Cos(longitudeAngle);
                    y = latCos;
                    z = -latSin * MathF.Sin(longitudeAngle);
                    v.Position = new(x, y, z);

                    // normalized vertex normal (nx, ny, nz)
                    nx = x * lengthInv;
                    ny = y * lengthInv;
                    nz = z * lengthInv;
                    v.Normal = new(nx, ny, nz);

                    if (Vector3.Dot(Vector3.UnitY, v.Normal) == 1.0f)
                    {
                        v.Tangent = Vector3.UnitX;
                    }
                    else
                    {
                        v.Tangent = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, v.Normal));
                    }

                    // vertex tex coord (s, t) range between [0, 1]
                    s = (float)j / LongLines;
                    t = (float)i / LatLines;

                    v.UV = new(MathF.Abs(1 - s), t, 0);
                    vertices[vi++] = v;
                }
            }

            vertexBuffer = new(device, CpuAccessFlags.None, vertices);

            uint k1, k2;
            int ii = 0;
            uint[] indices = new uint[LatLines * LongLines * 6 - 6];
            for (uint i = 0; i < LatLines; ++i)
            {
                k1 = i * (LongLines + 1);     // beginning of current stack
                k2 = k1 + LongLines + 1;      // beginning of next stack

                for (int j = 0; j < LongLines; ++j, ++k1, ++k2)
                {
                    // 2 triangles per sector excluding first and last stacks
                    // k1 => k2 => k1+1
                    if (i != 0)
                    {
                        indices[ii++] = k1;
                        indices[ii++] = k2;
                        indices[ii++] = k1 + 1;
                    }

                    // k1+1 => k2 => k2+1
                    if (i != LatLines - 1)
                    {
                        indices[ii++] = k1 + 1;
                        indices[ii++] = k2;
                        indices[ii++] = k2 + 1;
                    }
                }
            }

            indexBuffer = new(device, CpuAccessFlags.None, indices);
        }
    }
}