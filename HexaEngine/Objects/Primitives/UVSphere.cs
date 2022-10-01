﻿namespace HexaEngine.Objects.Primitives
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Objects;
    using System;

    public class UVSphere : Primitive<Vertex>
    {
        public UVSphere(IGraphicsDevice device) : base(device)
        {
        }

        protected override (VertexBuffer<Vertex>, IndexBuffer?, InstanceBuffer?) InitializeMesh(IGraphicsDevice device)
        {
            CreateSphere(out VertexBuffer<Vertex> vertexBuffer, out IndexBuffer indexBuffer);
            return (vertexBuffer, indexBuffer, null);
        }

        public static void CreateSphere(out VertexBuffer<Vertex> vertexBuffer, out IndexBuffer indexBuffer, int LatLines = 10, int LongLines = 10)
        {
            float radius = 1;
            float x, y, z, xy;                              // vertex position
            float nx, ny, nz, lengthInv = 1.0f / radius;    // vertex normal
            float s, t;                                     // vertex texCoord

            float sectorStep = 2 * MathF.PI / LongLines;
            float stackStep = MathF.PI / LatLines;
            float sectorAngle, stackAngle;

            vertexBuffer = new();
            indexBuffer = new();

            for (int i = 0; i <= LatLines; ++i)
            {
                stackAngle = MathF.PI / 2 - i * stackStep;        // starting from pi/2 to -pi/2
                xy = radius * MathF.Cos(stackAngle);             // r * cos(u)
                z = radius * MathF.Sin(stackAngle);              // r * sin(u)

                // add (sectorCount+1) vertices per stack
                // the first and last vertices have same position and normal, but different tex coords
                for (int j = 0; j <= LongLines; ++j)
                {
                    Vertex v = new();
                    sectorAngle = j * sectorStep;           // starting from 0 to 2pi

                    // vertex position (x, y, z)
                    x = xy * MathF.Cos(sectorAngle);             // r * cos(u) * cos(v)
                    y = xy * MathF.Sin(sectorAngle);             // r * cos(u) * sin(v)
                    v.Position = new(x, y, z);

                    // normalized vertex normal (nx, ny, nz)
                    nx = x * lengthInv;
                    ny = y * lengthInv;
                    nz = z * lengthInv;
                    v.Normal = new(nx, ny, nz);

                    // vertex tex coord (s, t) range between [0, 1]
                    s = (float)j / LongLines;
                    t = (float)i / LatLines;
                    v.Texture = new(s, t, 0);
                    vertexBuffer.Append(v);
                }
            }

            int k1, k2;

            for (int i = 0; i < LatLines; ++i)
            {
                k1 = i * (LongLines + 1);     // beginning of current stack
                k2 = k1 + LongLines + 1;      // beginning of next stack

                for (int j = 0; j < LongLines; ++j, ++k1, ++k2)
                {
                    // 2 triangles per sector excluding first and last stacks
                    // k1 => k2 => k1+1
                    if (i != 0)
                    {
                        indexBuffer.Append(k1);
                        indexBuffer.Append(k2);
                        indexBuffer.Append(k1 + 1);
                    }

                    // k1+1 => k2 => k2+1
                    if (i != LatLines - 1)
                    {
                        indexBuffer.Append(k1 + 1);
                        indexBuffer.Append(k2);
                        indexBuffer.Append(k2 + 1);
                    }
                }
            }
        }

        public static void CreateSphere(out Vertex[] vertices, out int[] indexBuffer, int LatLines = 10, int LongLines = 10)
        {
            float radius = 1;
            float x, y, z, xy;                              // vertex position
            float nx, ny, nz, lengthInv = 1.0f / radius;    // vertex normal
            float s, t;                                     // vertex texCoord

            float sectorStep = 2 * MathF.PI / LongLines;
            float stackStep = MathF.PI / LatLines;
            float sectorAngle, stackAngle;

            vertices = new Vertex[(LatLines + 1) * (LongLines + 1)];
            //vertices = new Vertex[LatLines * LongLines];
            int vi = 0;
            for (int i = 0; i <= LatLines; ++i)
            {
                stackAngle = MathF.PI / 2 - i * stackStep;        // starting from pi/2 to -pi/2
                xy = radius * MathF.Cos(stackAngle);             // r * cos(u)
                z = radius * MathF.Sin(stackAngle);              // r * sin(u)

                // add (sectorCount+1) vertices per stack
                // the first and last vertices have same position and normal, but different tex coords
                for (int j = 0; j <= LongLines; ++j)
                {
                    Vertex v = new();
                    sectorAngle = j * sectorStep;           // starting from 0 to 2pi

                    // vertex position (x, y, z)
                    x = xy * MathF.Cos(sectorAngle);             // r * cos(u) * cos(v)
                    y = xy * MathF.Sin(sectorAngle);             // r * cos(u) * sin(v)
                    v.Position = new(x, y, z);

                    // normalized vertex normal (nx, ny, nz)
                    nx = x * lengthInv;
                    ny = y * lengthInv;
                    nz = z * lengthInv;
                    v.Normal = new(nx, ny, nz);

                    // vertex tex coord (s, t) range between [0, 1]
                    s = (float)j / LongLines;
                    t = (float)i / LatLines;
                    v.Texture = new(s, t, 0);
                    vertices[vi++] = v;
                }
            }

            int k1, k2;
            int ii = 0;
            indexBuffer = new int[LatLines * LongLines * 6 - 6];
            for (int i = 0; i < LatLines; ++i)
            {
                k1 = i * (LongLines + 1);     // beginning of current stack
                k2 = k1 + LongLines + 1;      // beginning of next stack

                for (int j = 0; j < LongLines; ++j, ++k1, ++k2)
                {
                    // 2 triangles per sector excluding first and last stacks
                    // k1 => k2 => k1+1
                    if (i != 0)
                    {
                        indexBuffer[ii++] = k1;
                        indexBuffer[ii++] = k2;
                        indexBuffer[ii++] = k1 + 1;
                    }

                    // k1+1 => k2 => k2+1
                    if (i != LatLines - 1)
                    {
                        indexBuffer[ii++] = k1 + 1;
                        indexBuffer[ii++] = k2;
                        indexBuffer[ii++] = k2 + 1;
                    }
                }
            }
        }

        public static void CreateSphere(out MeshVertex[] vertices, out int[] indexBuffer, int LatLines = 10, int LongLines = 10)
        {
            float radius = 1;
            float x, y, z, xy;                              // vertex position
            float nx, ny, nz, lengthInv = 1.0f / radius;    // vertex normal
            float s, t;                                     // vertex texCoord

            float sectorStep = 2 * MathF.PI / LongLines;
            float stackStep = MathF.PI / LatLines;
            float sectorAngle, stackAngle;

            vertices = new MeshVertex[(LatLines + 1) * (LongLines + 1)];
            //vertices = new Vertex[LatLines * LongLines];
            int vi = 0;
            for (int i = 0; i <= LatLines; ++i)
            {
                stackAngle = MathF.PI / 2 - i * stackStep;        // starting from pi/2 to -pi/2
                xy = radius * MathF.Cos(stackAngle);             // r * cos(u)
                z = radius * MathF.Sin(stackAngle);              // r * sin(u)

                // add (sectorCount+1) vertices per stack
                // the first and last vertices have same position and normal, but different tex coords
                for (int j = 0; j <= LongLines; ++j)
                {
                    MeshVertex v = new();
                    sectorAngle = j * sectorStep;           // starting from 0 to 2pi

                    // vertex position (x, y, z)
                    x = xy * MathF.Cos(sectorAngle);             // r * cos(u) * cos(v)
                    y = xy * MathF.Sin(sectorAngle);             // r * cos(u) * sin(v)
                    v.Position = new(x, y, z);

                    // normalized vertex normal (nx, ny, nz)
                    nx = x * lengthInv;
                    ny = y * lengthInv;
                    nz = z * lengthInv;
                    v.Normal = new(nx, ny, nz);

                    // vertex tex coord (s, t) range between [0, 1]
                    s = (float)j / LongLines;
                    t = (float)i / LatLines;
                    v.Texture = new(s, t);
                    vertices[vi++] = v;
                }
            }

            int k1, k2;
            int ii = 0;
            indexBuffer = new int[LatLines * LongLines * 6 - 6];
            for (int i = 0; i < LatLines; ++i)
            {
                k1 = i * (LongLines + 1);     // beginning of current stack
                k2 = k1 + LongLines + 1;      // beginning of next stack

                for (int j = 0; j < LongLines; ++j, ++k1, ++k2)
                {
                    // 2 triangles per sector excluding first and last stacks
                    // k1 => k2 => k1+1
                    if (i != 0)
                    {
                        indexBuffer[ii++] = k1;
                        indexBuffer[ii++] = k2;
                        indexBuffer[ii++] = k1 + 1;
                    }

                    // k1+1 => k2 => k2+1
                    if (i != LatLines - 1)
                    {
                        indexBuffer[ii++] = k1 + 1;
                        indexBuffer[ii++] = k2;
                        indexBuffer[ii++] = k2 + 1;
                    }
                }
            }
        }
    }
}