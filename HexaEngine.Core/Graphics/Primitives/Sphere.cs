namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;

    public class Sphere : Primitive<MeshVertex, uint>
    {
        public Sphere(IGraphicsDevice device) : base(device)
        {
        }

        protected override (VertexBuffer<MeshVertex>, IndexBuffer<uint>?) InitializeMesh(IGraphicsDevice device)
        {
            CreateSphere(device, out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer);
            return (vertexBuffer, indexBuffer);
        }

        /* public static void CreateSphere(IGraphicsDevice device, out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer, uint LatLines = 32, uint LongLines = 32)
         {
             float radius = 1;
             float x, y, z;                              // vertex position
             float nx, ny, nz, lengthInv = 1.0f / radius;    // vertex normal
             float s, t;                                     // vertex texCoord

             float deltaLatitude = MathF.PI / LatLines;
             float deltaLongitude = 2 * MathF.PI / LongLines;
             float longitudeAngle, latitudeAngle;

             MeshVertex[] vertices = new MeshVertex[(LatLines + 1) * (LongLines + 1)];

             int vcounter = 0;
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

                     v.Bitangent = Vector3.Cross(v.Normal, v.Tangent);

                     // vertex tex coord (s, t) range between [0, 1]
                     s = (float)j / LongLines;
                     t = (float)i / LatLines;

                     v.UV = new(MathF.Abs(1 - s), t, 0);
                     vertices[vcounter++] = v;
                 }
             }

             vertexBuffer = new(device, vertices, CpuAccessFlags.None);

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

             indexBuffer = new(device, indices, CpuAccessFlags.None);
         }*/

        public static void CreateSphere(IGraphicsDevice device, out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer, float diameter = 1, uint tessellation = 16, bool invertn = false)
        {
            if (tessellation < 3)
                throw new ArgumentException("tesselation parameter must be at least 3");

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

                    Vector3 bitangent = Vector3.Cross(normal, tangent);

                    Vector2 textureCoordinate = new(u, v);

                    vertices[vcounter++] = new(normal * radius, textureCoordinate, normal, tangent, bitangent);
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

            vertexBuffer = new(device, vertices, CpuAccessFlags.None);
            indexBuffer = new(device, indices, CpuAccessFlags.None);
        }
    }
}