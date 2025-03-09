namespace HexaEngine.Core.Graphics.Primitives
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics.Buffers;
    using System;
    using System.Numerics;

    public struct CapsuleDesc
    {
        public float Height = 1;
        public float Diameter = 1;
        public uint Tessellation = 16;

        public CapsuleDesc()
        {
        }

        public CapsuleDesc(float height, float diameter, uint tessellation)
        {
            Height = height;
            Diameter = diameter;
            Tessellation = tessellation;
        }
    }

    public class Capsule : Primitive<CapsuleDesc, uint>
    {
        public Capsule(CapsuleDesc desc) : base(desc)
        {
        }

        protected override (VertexBuffer<PrimVertex>, IndexBuffer<uint>?) InitializeMesh(CapsuleDesc desc)
        {
            CreateCapsule(out VertexBuffer<PrimVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer, desc.Height, desc.Diameter, desc.Tessellation);
            return (vertexBuffer, indexBuffer);
        }

        public static unsafe void CreateCapsule(out VertexBuffer<PrimVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer, float height = 1, float diameter = 1, uint tessellation = 16)
        {
            if (tessellation < 3)
            {
                throw new ArgumentException("tesselation parameter must be at least 3");
            }

            height /= 2;

            Vector3 topOffset = new(0, height, 0);

            uint verticalSegments = tessellation / 2;
            uint horizontalSegments = tessellation;

            float radius = diameter / 2;
            uint stride = tessellation + 1;
            uint strideSquared = stride * 2;

            uint vertexCount = stride * 2 + (verticalSegments + 1) * (horizontalSegments + 1) * 2;
            uint indexCount = stride * 6 + verticalSegments * (horizontalSegments + 1) * 6 * 2;

            PrimVertex* vertices = AllocT<PrimVertex>(vertexCount);
            uint* indices = AllocT<uint>(indexCount);

            PrimVertex* vtxWritePtr = vertices;
            uint* idxWritePtr = indices;

            for (uint i = 0; i <= tessellation; i++)
            {
                Vector3 normal = GetCircleVector(i, tessellation);

                Vector3 sideOffset = normal * radius;

                float u = (float)i / tessellation;

                Vector3 tangent = GetCircleTangent(i, tessellation);

                *vtxWritePtr++ = new(sideOffset + topOffset, new Vector2(u, 0), normal, tangent);
                *vtxWritePtr++ = new(sideOffset - topOffset, new Vector2(u, 1), normal, tangent);

                uint baseIdx = i * 2;

                *idxWritePtr++ = baseIdx;
                *idxWritePtr++ = (baseIdx + 2) % strideSquared;
                *idxWritePtr++ = baseIdx + 1;

                *idxWritePtr++ = baseIdx + 1;
                *idxWritePtr++ = (baseIdx + 2) % strideSquared;
                *idxWritePtr++ = (baseIdx + 3) % strideSquared;
            }

            CreateCapsuleCap(diameter, topOffset, verticalSegments, horizontalSegments, vertices, indices, &vtxWritePtr, &idxWritePtr, true);
            CreateCapsuleCap(diameter, topOffset, verticalSegments, horizontalSegments, vertices, indices, &vtxWritePtr, &idxWritePtr, false);

            vertexBuffer = new VertexBuffer<PrimVertex>(vertices, vertexCount, CpuAccessFlags.None);
            indexBuffer = new IndexBuffer<uint>(indices, indexCount, CpuAccessFlags.None);

            Free(indices);
            Free(vertices);
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

        private static unsafe void CreateCapsuleCap(float diameter, Vector3 topOffset, uint verticalSegments, uint horizontalSegments, PrimVertex* vertices, uint* indices, PrimVertex** pVtxWritePtr, uint** pIdxWritePtr, bool top)
        {
            PrimVertex* vtxWritePtr = *pVtxWritePtr;
            uint* idxWritePtr = *pIdxWritePtr;

            float radius = diameter / 2;

            float latitudeOffset = top ? MathUtil.PIDIV2 : 0;
            topOffset = top ? topOffset : -topOffset;

            uint idxBase = (uint)(vtxWritePtr - vertices);

            for (uint i = 0; i <= verticalSegments; i++)
            {
                float v = 1 - (float)i / verticalSegments;
                float latitude = i * (MathUtil.PI / 2) / verticalSegments - MathUtil.PIDIV2 + latitudeOffset;

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

                    *vtxWritePtr++ = new(normal * radius + topOffset, textureCoordinate, normal, tangent);
                }
            }

            uint stride = horizontalSegments + 1;

            for (uint i = 0; i < verticalSegments; i++)
            {
                for (uint j = 0; j <= horizontalSegments; j++)
                {
                    uint nextI = i + 1;
                    uint nextJ = (j + 1) % stride;

                    *idxWritePtr++ = idxBase + i * stride + j;
                    *idxWritePtr++ = idxBase + i * stride + nextJ;
                    *idxWritePtr++ = idxBase + nextI * stride + j;
                    *idxWritePtr++ = idxBase + i * stride + nextJ;
                    *idxWritePtr++ = idxBase + nextI * stride + nextJ;
                    *idxWritePtr++ = idxBase + nextI * stride + j;
                }
            }

            *pVtxWritePtr = vtxWritePtr;
            *pIdxWritePtr = idxWritePtr;
        }
    }
}