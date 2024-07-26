namespace HexaEngine.UI.Graphics
{
    using Hexa.NET.Mathematics;
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public static class DrawHelper
    {
        private const int COL32_R_SHIFT = 0;
        private const int COL32_G_SHIFT = 8;
        private const int COL32_B_SHIFT = 16;
        private const int COL32_A_SHIFT = 24;
        private const uint COL32_A_MASK = 0xFF000000;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Saturate(float f)
        {
            return f < 0.0f ? 0.0f : f > 1.0f ? 1.0f : f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int F32ToInt8Sat(float val)
        {
            return (int)(Saturate(val) * 255.0f + 0.5f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ColorConvertFloat4ToU32(Vector4 i)
        {
            uint o;
            o = (uint)F32ToInt8Sat(i.X) << COL32_R_SHIFT;
            o |= (uint)F32ToInt8Sat(i.Y) << COL32_G_SHIFT;
            o |= (uint)F32ToInt8Sat(i.Z) << COL32_B_SHIFT;
            o |= (uint)F32ToInt8Sat(i.W) << COL32_A_SHIFT;
            return o;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Col4ToUInt(this Vector4 i)
        {
            return ColorConvertFloat4ToU32(i);
        }

        public static void FillRect(this UICommandList list, RectangleF rectangle, Brush brush)
        {
            list.FillRect(rectangle.Top, rectangle.Bottom, rectangle.Left, rectangle.Right, brush);
        }

        public static void FillRectangle(this UICommandList list, Vector2 origin, Vector2 size, Brush brush)
        {
            list.FillRect(origin.Y, origin.Y + size.Y, origin.X, origin.X + size.X, brush);
        }

        public static unsafe void FillRect(this UICommandList list, float top, float bottom, float left, float right, Brush brush)
        {
            Vector2 p0 = new(left, bottom); // bottom left corner
            Vector2 p1 = new(left, top); // top left corner
            Vector2 p2 = new(right, top); // top right corner
            Vector2 p3 = new(right, bottom); // bottom right corner

            Vector2* controlPoints = stackalloc Vector2[4] { p0, p1, p2, p3 };

            list.FillConvexPath(controlPoints, 4, brush);
        }

        public static void DrawRect(this UICommandList list, Vector2 origin, Vector2 size, Brush brush, float thickness)
        {
            if (thickness == 0)
            {
                return;
            }

            list.DrawRect(origin.Y, origin.Y + size.Y, origin.X, origin.X + size.X, brush, thickness);
        }

        public static unsafe void DrawRect(this UICommandList list, float top, float bottom, float left, float right, Brush brush, float thickness)
        {
            if (thickness == 0)
            {
                return;
            }

            Vector2 p0 = new(left, bottom); // bottom left corner
            Vector2 p1 = new(left, top); // top left corner
            Vector2 p2 = new(right, top); // top right corner
            Vector2 p3 = new(right, bottom); // bottom right corner

            Vector2* controlPoints = stackalloc Vector2[4] { p0, p1, p2, p3 };

            list.DrawPath(controlPoints, 4, brush, thickness, true);
        }

        public static void DrawRoundedRect(this UICommandList list, Vector2 origin, Vector2 size, Vector2 radius, Brush brush, float thickness)
        {
            if (thickness == 0)
            {
                return;
            }

            list.DrawRoundedRect(origin.Y, origin.Y + size.Y, origin.X, origin.X + size.X, radius, brush, thickness);
        }

        public static unsafe void DrawRoundedRect(this UICommandList list, float top, float bottom, float left, float right, Vector2 radius, Brush brush, float thickness)
        {
            if (thickness == 0)
            {
                return;
            }

            const int segments = 32;
            const float step = MathF.PI * 2 / segments;

            Vector2* controlPoints = stackalloc Vector2[segments];

            Vector2 p0 = new(left + radius.X, bottom - radius.Y); // bottom left corner
            Vector2 p1 = new(left + radius.X, top + radius.Y); // top left corner
            Vector2 p2 = new(right - radius.X, top + radius.Y); // top right corner
            Vector2 p3 = new(right - radius.X, bottom - radius.Y); // bottom right corner

            DrawArcInternal(controlPoints, p3, radius, segments / 4, 0, 0, step);
            DrawArcInternal(controlPoints, p0, radius, segments / 4, 8, 8, step);
            DrawArcInternal(controlPoints, p1, radius, segments / 4, 16, 16, step);
            DrawArcInternal(controlPoints, p2, radius, segments / 4, 24, 24, step);

            list.DrawPath(controlPoints, segments, brush, thickness, true);
        }

        public static void FillRoundedRect(this UICommandList list, Vector2 origin, Vector2 size, Vector2 radius, Brush brush)
        {
            list.FillRoundedRect(origin.Y, origin.Y + size.Y, origin.X, origin.X + size.X, radius, brush);
        }

        public static unsafe void FillRoundedRect(this UICommandList list, float top, float bottom, float left, float right, Vector2 radius, Brush brush)
        {
            const int segments = 32;
            const float step = MathF.PI * 2 / segments;

            Vector2* controlPoints = stackalloc Vector2[segments];

            Vector2 p0 = new(left + radius.X, bottom - radius.Y); // bottom left corner
            Vector2 p1 = new(left + radius.X, top + radius.Y); // top left corner
            Vector2 p2 = new(right - radius.X, top + radius.Y); // top right corner
            Vector2 p3 = new(right - radius.X, bottom - radius.Y); // bottom right corner

            DrawArcInternal(controlPoints, p3, radius, segments / 4, 0, 0, step);
            DrawArcInternal(controlPoints, p0, radius, segments / 4, 8, 8, step);
            DrawArcInternal(controlPoints, p1, radius, segments / 4, 16, 16, step);
            DrawArcInternal(controlPoints, p2, radius, segments / 4, 24, 24, step);

            list.FillConvexPath(controlPoints, segments, brush);
        }

        private static unsafe void DrawArcInternal(Vector2* controlPoints, Vector2 center, Vector2 radius, int segments, int offset, int arcOffset, float step)
        {
            for (int i = 0; i < segments; i++)
            {
                float angle = (i + arcOffset) * step;
                var x = center.X + MathF.Cos(angle) * radius.X;
                var y = center.Y + MathF.Sin(angle) * radius.Y;
                controlPoints[i + offset] = new Vector2(x, y);
            }
        }

        public static unsafe void DrawLine(this UICommandList builder, Vector2 start, Vector2 end, Brush brush, float thickness)
        {
            if (thickness == 0)
            {
                return;
            }

            Vector2* controlPoints = stackalloc Vector2[2] { start, end };

            builder.DrawPath(controlPoints, 2, brush, thickness, false);
        }

        public static unsafe void DrawEllipse(this UICommandList list, Vector2 center, Vector2 radius, Brush brush, float thickness)
        {
            if (thickness == 0)
            {
                return;
            }

            const int segments = 32;
            const float step = MathF.PI * 2 / segments;

            var controlPoints = stackalloc Vector2[segments];

            for (int i = 0; i < segments; i++)
            {
                var angle = step * i;
                var x = center.X + MathF.Cos(angle) * radius.X;
                var y = center.Y + MathF.Sin(angle) * radius.Y;
                controlPoints[i] = new Vector2(x, y);
            }

            list.DrawPath(controlPoints, segments, brush, thickness, true);
        }

        public static unsafe void FillEllipse(this UICommandList list, Vector2 center, Vector2 radius, Brush brush)
        {
            const int segments = 32;
            const float step = MathF.PI * 2 / segments;
            Vector2* controlPoints = stackalloc Vector2[segments];
            for (int i = 0; i < segments; i++)
            {
                var angle = step * i;
                var x = center.X + MathF.Cos(angle) * radius.X;
                var y = center.Y + MathF.Sin(angle) * radius.Y;
                controlPoints[i] = new(x, y);
            }

            list.FillConvexPath(controlPoints, segments, brush);
        }

        public static unsafe void DrawArc(this UICommandList list, Vector2 center, Vector2 radius, float startAngle, float endAngle, Brush brush, float thickness)
        {
            if (thickness == 0)
            {
                return;
            }

            const int segments = 32;
            float angleRange = endAngle - startAngle;
            float step = angleRange / segments;
            Vector2* controlPoints = stackalloc Vector2[segments + 1];
            for (int i = 0; i < segments + 1; i++)
            {
                float angle = step * i + startAngle;
                var x = center.X + MathF.Cos(angle) * radius.X;
                var y = center.Y + MathF.Sin(angle) * radius.Y;
                controlPoints[i] = new(x, y);
            }

            list.DrawPath(controlPoints, segments + 1, brush, thickness, false);
        }

        public static unsafe void FillArc(this UICommandList list, Vector2 center, Vector2 radius, float startAngle, float endAngle, Brush brush)
        {
            const int segments = 32;
            float angleRange = endAngle - startAngle;
            float step = angleRange / segments;
            Vector2* controlPoints = stackalloc Vector2[segments + 2];
            controlPoints[0] = center;
            for (int i = 0; i < segments + 1; i++)
            {
                float angle = step * i + startAngle;
                var x = center.X + MathF.Cos(angle) * radius.X;
                var y = center.Y + MathF.Sin(angle) * radius.Y;
                controlPoints[i + 1] = new(x, y);
            }

            list.FillConvexPath(controlPoints, segments + 2, brush);
        }

        public static void PrimRect(this UICommandList list, Vector2 a, Vector2 c, uint color)
        {
            Vector2 uv = Vector2.Zero;
            Vector2 b = new(c.X, a.Y), d = new(a.X, c.Y);

            uint idx0 = list.AddVertex(new(a, uv, color));
            uint idx1 = list.AddVertex(new(b, uv, color));
            uint idx2 = list.AddVertex(new(c, uv, color));
            uint idx3 = list.AddVertex(new(d, uv, color));
            list.AddFace(idx0, idx1, idx2);
            list.AddFace(idx0, idx2, idx3);
        }

        public static void PrimRect(this UICommandList list, Vector2 a, Vector2 c, Vector2 uvA, Vector2 uvC, uint color)
        {
            Vector2 b = new(c.X, a.Y), d = new(a.X, c.Y), uvB = new(uvC.X, uvA.Y), uvD = new(uvA.X, uvC.Y);
            uint idx0 = list.AddVertex(new(a, uvA, color));
            uint idx1 = list.AddVertex(new(b, uvB, color));
            uint idx2 = list.AddVertex(new(c, uvC, color));
            uint idx3 = list.AddVertex(new(d, uvD, color));
            list.AddFace(idx0, idx1, idx2);
            list.AddFace(idx0, idx2, idx3);
        }

        public static void PrimQuad(this UICommandList list, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 uvA, Vector2 uvB, Vector2 uvC, Vector2 uvD, uint color)
        {
            uint idx0 = list.AddVertex(new(a, uvA, color));
            uint idx1 = list.AddVertex(new(b, uvB, color));
            uint idx2 = list.AddVertex(new(c, uvC, color));
            uint idx3 = list.AddVertex(new(d, uvD, color));
            list.AddFace(idx0, idx1, idx2);
            list.AddFace(idx0, idx2, idx3);
        }

        private static Vector2 NormalizeOverZero(Vector2 normal)
        {
            float d2 = normal.X * normal.X + normal.Y * normal.Y;
            if (d2 > 0.0f)
            {
                float inv_len = 1 / MathF.Sqrt(d2);
                normal.X *= inv_len;
                normal.Y *= inv_len;
            }
            return normal;
        }

        private const float IM_FIXNORMAL2F_MAX_INVLEN2 = 100.0f;// 500.0f (see #4053, #3366)

        private static Vector2 FixNormal(Vector2 normal)
        {
            float d2 = normal.X * normal.X + normal.Y * normal.Y;
            if (d2 > 0.000001f)
            {
                float inv_len2 = 1.0f / d2;
                if (inv_len2 > IM_FIXNORMAL2F_MAX_INVLEN2)
                {
                    inv_len2 = IM_FIXNORMAL2F_MAX_INVLEN2;
                }

                normal.X *= inv_len2; normal.Y *= inv_len2;
            }
            return normal;
        }

        public static bool AntiAliasing { get; set; } = true;

        public static float AntiAliasSize { get; set; } = 1f;

        public static unsafe void DrawPath(this UICommandList list, Vector2* points, int pointCount, Brush brush, float thickness, bool closed)
        {
            uint color = uint.MaxValue;
            if (pointCount < 2 || (color & COL32_A_MASK) == 0)
            {
                return;
            }

            Vector2 uv = Vector2.Zero;
            int count = closed ? pointCount : pointCount - 1; // The number of line segments we need to draw
            bool thickLine = thickness > AntiAliasSize;

            if (AntiAliasing)
            {
                // Anti-aliased stroke
                float aaSize = AntiAliasSize;
                uint colTrans = color & ~COL32_A_MASK;

                // Thicknesses <1.0 should behave like thickness 1.0
                thickness = MathF.Max(thickness, 1.0f);
                int integerThickness = (int)thickness;
                float fractionalThickness = thickness - integerThickness;

                int idxCount = thickLine ? count * 18 : count * 12;
                int vtxCount = thickLine ? pointCount * 4 : pointCount * 3;
                list.PrimReserve(idxCount, vtxCount);

                Vector2* tempNormals;

                int tempCount = pointCount * 5;
                int byteCount = tempCount * sizeof(Vector2);

                bool isStack = byteCount <= 1024;

                if (isStack)
                {
                    var temp_normal = stackalloc Vector2[tempCount];
                    tempNormals = temp_normal;
                }
                else
                {
                    tempNormals = (Vector2*)Marshal.AllocHGlobal(byteCount);
                }

                Vector2* tempPoints = tempNormals + pointCount;

                // Calculate normals (tangents) for each line segment
                for (int i1 = 0; i1 < count; i1++)
                {
                    int i2 = i1 + 1 == pointCount ? 0 : i1 + 1;
                    Vector2 d = points[i2] - points[i1];
                    Vector2 n = NormalizeOverZero(d);
                    tempNormals[i1].X = n.Y;
                    tempNormals[i1].Y = -n.X;
                }
                if (!closed)
                {
                    tempNormals[pointCount - 1] = tempNormals[pointCount - 2];
                }

                float halfInnerThickness = (thickness - aaSize) * 0.5f;

                // If line is not closed, the first and last points need to be generated differently as there are no normals to blend
                if (!closed)
                {
                    int pointsLast = pointCount - 1;
                    tempPoints[0] = points[0] + tempNormals[0] * (halfInnerThickness + aaSize);
                    tempPoints[1] = points[0] + tempNormals[0] * halfInnerThickness;
                    tempPoints[2] = points[0] - tempNormals[0] * halfInnerThickness;
                    tempPoints[3] = points[0] - tempNormals[0] * (halfInnerThickness + aaSize);
                    tempPoints[pointsLast * 4 + 0] = points[pointsLast] + tempNormals[pointsLast] * (halfInnerThickness + aaSize);
                    tempPoints[pointsLast * 4 + 1] = points[pointsLast] + tempNormals[pointsLast] * halfInnerThickness;
                    tempPoints[pointsLast * 4 + 2] = points[pointsLast] - tempNormals[pointsLast] * halfInnerThickness;
                    tempPoints[pointsLast * 4 + 3] = points[pointsLast] - tempNormals[pointsLast] * (halfInnerThickness + aaSize);
                }

                // Generate the indices to form a number of triangles for each line segment, and the vertices for the line edges
                // This takes points n and n+1 and writes into n+1, with the first point in a closed line being generated from the final one (as n+1 wraps)
                // FIXME-OPT: Merge the different loops, possibly remove the temporary buffer.
                uint idx1 = 0; // Vertex index for start of line segment
                for (int i1 = 0; i1 < count; i1++) // i1 is the first point of the line segment
                {
                    int i2 = i1 + 1 == pointCount ? 0 : i1 + 1; // i2 is the second point of the line segment
                    uint idx2 = i1 + 1 == pointCount ? 0 : idx1 + 4; // Vertex index for end of segment

                    // Average normals
                    Vector2 normal = (tempNormals[i1] + tempNormals[i2]) * 0.5f;
                    normal = FixNormal(normal);
                    Vector2 dmOut = normal * (halfInnerThickness + aaSize);
                    Vector2 dmIn = normal * halfInnerThickness;

                    // Add temporary vertices
                    Vector2* outVtx = &tempPoints[i2 * 4];
                    outVtx[0] = points[i2] + dmOut;
                    outVtx[1] = points[i2] + dmIn;
                    outVtx[2] = points[i2] - dmIn;
                    outVtx[3] = points[i2] - dmOut;

                    // Add indexes
                    list.AddFace(idx2 + 1, idx1 + 1, idx1 + 2);
                    list.AddFace(idx1 + 2, idx2 + 2, idx2 + 1);
                    list.AddFace(idx2 + 1, idx1 + 1, idx1 + 0);
                    list.AddFace(idx1 + 0, idx2 + 0, idx2 + 1);
                    list.AddFace(idx2 + 2, idx1 + 2, idx1 + 3);
                    list.AddFace(idx1 + 3, idx2 + 3, idx2 + 2);

                    idx1 = idx2;
                }

                // Add vertices
                for (int i = 0; i < pointCount; i++)
                {
                    list.AddVertex(new(tempPoints[i * 4 + 0], uv, colTrans));
                    list.AddVertex(new(tempPoints[i * 4 + 1], uv, color));
                    list.AddVertex(new(tempPoints[i * 4 + 2], uv, color));
                    list.AddVertex(new(tempPoints[i * 4 + 3], uv, colTrans));
                }
            }
            else
            {
                // Non anti-aliased lines
                int idx_count = count * 6;
                int vtx_count = count * 4;
                list.PrimReserve(idx_count, vtx_count);

                int vtxCurrentIdx = 0;
                for (int i1 = 0; i1 < count; i1++)
                {
                    int i2 = i1 + 1 == pointCount ? 0 : i1 + 1;
                    Vector2 p1 = points[i1];
                    Vector2 p2 = points[i2];

                    Vector2 d = p2 - p1;
                    Vector2 n = NormalizeOverZero(d);
                    n *= thickness * 0.5f;

                    list.AddVertex(new(new(p1.X + n.Y, p1.Y - n.X), uv, color));
                    list.AddVertex(new(new(p2.X + n.Y, p2.Y - n.X), uv, color));
                    list.AddVertex(new(new(p2.X - n.Y, p2.Y + n.X), uv, color));
                    list.AddVertex(new(new(p1.X - n.Y, p1.Y + n.X), uv, color));

                    list.AddFace((uint)vtxCurrentIdx, (uint)(vtxCurrentIdx + 1), (uint)(vtxCurrentIdx + 2));
                    list.AddFace((uint)vtxCurrentIdx, (uint)(vtxCurrentIdx + 2), (uint)(vtxCurrentIdx + 3));
                    vtxCurrentIdx += 4;
                }
            }

            list.RecordDraw(UICommandType.DrawPrimitive, brush);
        }

        public static unsafe void FillConvexPath(this UICommandList list, Vector2* points, int pointCount, Brush brush)
        {
            uint color = uint.MaxValue;

            if (pointCount < 3 || (color & COL32_A_MASK) == 0)
            {
                return;
            }

            Vector2 uv = Vector2.Zero;

            if (AntiAliasing)
            {
                // Anti-aliased Fill
                float aaSize = AntiAliasSize;
                uint colorTrans = color & ~COL32_A_MASK;
                int idxCount = (pointCount - 2) * 3 + pointCount * 6;
                int vtxCount = pointCount * 2;
                list.PrimReserve(idxCount, vtxCount);

                // Add indexes for fill
                const uint vtxInnerIdx = 0;
                const uint vtxOuterIdx = 1;
                for (int i = 2; i < pointCount; i++)
                {
                    list.AddFace(vtxInnerIdx, (uint)(vtxInnerIdx + (i - 1 << 1)), (uint)(vtxInnerIdx + (i << 1)));
                }

                Vector2* tempNormal;
                bool isStack = pointCount * sizeof(Vector2) <= 1024;

                if (isStack)
                {
                    var temp_normal = stackalloc Vector2[pointCount];
                    tempNormal = temp_normal;
                }
                else
                {
                    tempNormal = (Vector2*)Marshal.AllocHGlobal(sizeof(Vector2) * pointCount);
                }

                // Compute normals
                for (int i0 = pointCount - 1, i1 = 0; i1 < pointCount; i0 = i1++)
                {
                    Vector2 p0 = points[i0];
                    Vector2 p1 = points[i1];
                    Vector2 delta = p1 - p0;
                    Vector2 n = NormalizeOverZero(delta);
                    tempNormal[i0] = new(n.Y, -n.X);
                }

                for (int i0 = pointCount - 1, i1 = 0; i1 < pointCount; i0 = i1++)
                {
                    // Average normals
                    var normal = (tempNormal[i0] + tempNormal[i1]) * 0.5f;
                    normal = FixNormal(normal);
                    normal *= aaSize * 0.5f;

                    // Add vertices
                    list.AddVertex(new(points[i1] - normal, uv, color));         // Inner
                    list.AddVertex(new(points[i1] + normal, uv, colorTrans));    // Outer

                    // Add indexes for fringes
                    list.AddFace((uint)(vtxInnerIdx + (i1 << 1)), (uint)(vtxInnerIdx + (i0 << 1)), (uint)(vtxOuterIdx + (i0 << 1)));
                    list.AddFace((uint)(vtxOuterIdx + (i0 << 1)), (uint)(vtxOuterIdx + (i1 << 1)), (uint)(vtxInnerIdx + (i1 << 1)));
                }

                if (!isStack)
                {
                    Marshal.FreeHGlobal((nint)tempNormal);
                }
            }
            else
            {
                // Non Anti-aliased Fill
                int idxCount = (pointCount - 2) * 3;
                int vtxCount = pointCount;
                list.PrimReserve(idxCount, vtxCount);
                var vtxPtr = list.VtxWritePtr;
                var idxPtr = list.IdxWritePtr;
                for (int i = 0; i < vtxCount; i++)
                {
                    list.AddVertex(new(points[i], uv, color));
                }

                for (int i = 2; i < pointCount; i++)
                {
                    list.AddFace(0, (uint)(i - 1), (uint)i);
                }
            }

            list.RecordDraw(UICommandType.DrawPrimitive, brush);
        }
    }
}