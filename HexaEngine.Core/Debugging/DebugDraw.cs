#nullable disable

namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public static unsafe class DebugDraw
    {
        private static readonly DebugDrawCommandQueue queue = new();

        private static Viewport viewport;
        private static Matrix4x4 camera;

        private const int COL32_R_SHIFT = 0;
        private const int COL32_G_SHIFT = 8;
        private const int COL32_B_SHIFT = 16;
        private const int COL32_A_SHIFT = 24;
        private const uint COL32_A_MASK = 0xFF000000;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Saturate(float f)
        {
            return (f < 0.0f) ? 0.0f : (f > 1.0f) ? 1.0f : f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int F32ToInt8Sat(float val)
        {
            return ((int)(Saturate(val) * 255.0f + 0.5f));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ColorConvertFloat4ToU32(Vector4 i)
        {
            uint o;
            o = ((uint)F32ToInt8Sat(i.X)) << COL32_R_SHIFT;
            o |= ((uint)F32ToInt8Sat(i.Y)) << COL32_G_SHIFT;
            o |= ((uint)F32ToInt8Sat(i.Z)) << COL32_B_SHIFT;
            o |= ((uint)F32ToInt8Sat(i.W)) << COL32_A_SHIFT;
            return o;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void TransformWithColor(DebugDrawVert* verts, Vector3[] positions, uint count, Matrix4x4 matrix, uint color)
        {
            for (uint i = 0; i < count; i++)
            {
                verts[i].Position = Vector3.Transform(positions[i], matrix);
                verts[i].Color = color;
            }
        }

        public static void NewFrame()
        {
            queue.Clear();
        }

        public static void Render()
        {
            queue.ClearUnused();
        }

        public static void SetViewport(Viewport viewport)
        {
            DebugDraw.viewport = viewport;
        }

        public static Viewport GetViewport()
        {
            return viewport;
        }

        public static void SetCamera(Matrix4x4 camera)
        {
            DebugDraw.camera = camera;
        }

        public static Matrix4x4 GetCamera()
        {
            return camera;
        }

        public static DebugDrawCommandQueue GetQueue()
        {
            return queue;
        }

        public static void DrawFrustum(string id, BoundingFrustum frustum, Vector4 col)
        {
            uint color = ColorConvertFloat4ToU32(col);
            if (queue.Draw(id, PrimitiveTopology.LineList, 24, BoundingFrustum.CornerCount, out var cmd))
            {
                cmd.Vertices = AllocT<DebugDrawVert>(BoundingFrustum.CornerCount);
                cmd.Indices = AllocCopyT(new ushort[]
                {
                0,1,
                1,2,
                2,3,
                3,0,
                0,4,
                1,5,
                2,6,
                3,7,
                4,5,
                5,6,
                6,7,
                7,4
                });
            }

            var corners = frustum.Corners;
            for (int i = 0; i < BoundingFrustum.CornerCount; i++)
            {
                cmd.Vertices[i].Color = color;
                cmd.Vertices[i].Position = corners[i];
            }
        }

        public static void DrawBoundingBox(string id, BoundingBox box, Vector4 col)
        {
            const uint vertexCount = 8;
            uint color = ColorConvertFloat4ToU32(col);
            if (queue.Draw(id, PrimitiveTopology.LineList, 24, vertexCount, out var cmd))
            {
                cmd.Indices = AllocCopyT(new ushort[] { 0, 1, 1, 2, 2, 3, 3, 0, 0, 4, 1, 5, 2, 6, 3, 7, 4, 5, 5, 6, 6, 7, 7, 4 });
                cmd.Vertices = AllocT<DebugDrawVert>(vertexCount);
            }

            cmd.Vertices[0].Position = new Vector3(box.Min.X, box.Max.Y, box.Min.Z);
            cmd.Vertices[1].Position = new Vector3(box.Min.X, box.Min.Y, box.Min.Z);
            cmd.Vertices[2].Position = new Vector3(box.Max.X, box.Min.Y, box.Min.Z);
            cmd.Vertices[3].Position = new Vector3(box.Max.X, box.Max.Y, box.Min.Z);
            cmd.Vertices[4].Position = new Vector3(box.Min.X, box.Max.Y, box.Max.Z);
            cmd.Vertices[5].Position = new Vector3(box.Min.X, box.Min.Y, box.Max.Z);
            cmd.Vertices[6].Position = new Vector3(box.Max.X, box.Min.Y, box.Max.Z);
            cmd.Vertices[7].Position = new Vector3(box.Max.X, box.Max.Y, box.Max.Z);

            for (int i = 0; i < vertexCount; i++)
            {
                cmd.Vertices[i].Color = color;
            }
        }

        private static readonly Vector3[] spherePositions =
        {
            new Vector3(-0.195090f,0.000000f,0.980785f),
            new Vector3(-0.382683f,0.000000f,0.923880f),
            new Vector3(-0.555570f,0.000000f,0.831470f),
            new Vector3(-0.707107f,0.000000f,0.707107f),
            new Vector3(-0.831470f,0.000000f,0.555570f),
            new Vector3(-0.923880f,0.000000f,0.382683f),
            new Vector3(-0.980785f,0.000000f,0.195090f),
            new Vector3(-1.000000f,0.000000f,0.000000f),
            new Vector3(-0.980785f,0.000000f,-0.195090f),
            new Vector3(-0.923880f,0.000000f,-0.382683f),
            new Vector3(-0.831470f,0.000000f,-0.555570f),
            new Vector3(-0.707107f,0.000000f,-0.707107f),
            new Vector3(-0.555570f,0.000000f,-0.831470f),
            new Vector3(-0.382683f,0.000000f,-0.923880f),
            new Vector3(-0.195090f,0.000000f,-0.980785f),
            new Vector3(-0.000000f,0.000000f,-1.000000f),
            new Vector3(0.195090f,0.000000f,-0.980785f),
            new Vector3(0.382683f,0.000000f,-0.923880f),
            new Vector3(0.555570f,0.000000f,-0.831470f),
            new Vector3(0.707107f,0.000000f,-0.707107f),
            new Vector3(0.831470f,0.000000f,-0.555570f),
            new Vector3(0.923880f,0.000000f,-0.382683f),
            new Vector3(0.980785f,0.000000f,-0.195090f),
            new Vector3(1.000000f,0.000000f,0.000000f),
            new Vector3(0.980785f,0.000000f,0.195090f),
            new Vector3(0.923880f,0.000000f,0.382683f),
            new Vector3(0.831470f,0.000000f,0.555570f),
            new Vector3(0.707107f,0.000000f,0.707107f),
            new Vector3(0.555570f,0.000000f,0.831470f),
            new Vector3(0.382683f,0.000000f,0.923880f),
            new Vector3(0.195090f,0.000000f,0.980785f),
            new Vector3(0.000000f,0.000000f,1.000000f),
            new Vector3(-0.000000f,1.000000f,-0.000000f),
            new Vector3(0.195090f,0.980785f,-0.000000f),
            new Vector3(0.382683f,0.923880f,-0.000000f),
            new Vector3(0.555570f,0.831470f,-0.000000f),
            new Vector3(0.707107f,0.707107f,-0.000000f),
            new Vector3(0.831470f,0.555570f,-0.000000f),
            new Vector3(0.923880f,0.382683f,-0.000000f),
            new Vector3(0.980785f,0.195090f,-0.000000f),
            new Vector3(0.980785f,-0.195090f,0.000000f),
            new Vector3(0.923880f,-0.382683f,0.000000f),
            new Vector3(0.831470f,-0.555570f,0.000000f),
            new Vector3(0.707107f,-0.707107f,0.000000f),
            new Vector3(0.555570f,-0.831470f,0.000000f),
            new Vector3(0.382683f,-0.923880f,0.000000f),
            new Vector3(0.195090f,-0.980785f,0.000000f),
            new Vector3(0.000000f,-1.000000f,0.000000f),
            new Vector3(-0.195090f,-0.980785f,0.000000f),
            new Vector3(-0.382683f,-0.923880f,0.000000f),
            new Vector3(-0.555570f,-0.831470f,0.000000f),
            new Vector3(-0.707107f,-0.707107f,0.000000f),
            new Vector3(-0.831470f,-0.555570f,0.000000f),
            new Vector3(-0.923880f,-0.382683f,0.000000f),
            new Vector3(-0.980785f,-0.195090f,0.000000f),
            new Vector3(-0.980785f,0.195090f,-0.000000f),
            new Vector3(-0.923880f,0.382683f,-0.000000f),
            new Vector3(-0.831470f,0.555570f,-0.000000f),
            new Vector3(-0.707107f,0.707107f,-0.000000f),
            new Vector3(-0.555570f,0.831470f,-0.000000f),
            new Vector3(-0.382683f,0.923880f,-0.000000f),
            new Vector3(-0.195090f,0.980785f,-0.000000f),
            new Vector3(-0.000000f,0.980785f,-0.195090f),
            new Vector3(-0.000000f,0.923880f,-0.382683f),
            new Vector3(-0.000000f,0.831470f,-0.555570f),
            new Vector3(-0.000000f,0.707107f,-0.707107f),
            new Vector3(-0.000000f,0.555570f,-0.831470f),
            new Vector3(-0.000000f,0.382683f,-0.923880f),
            new Vector3(-0.000000f,0.195090f,-0.980785f),
            new Vector3(-0.000000f,-0.195090f,-0.980785f),
            new Vector3(-0.000000f,-0.382683f,-0.923880f),
            new Vector3(-0.000000f,-0.555570f,-0.831470f),
            new Vector3(0.000000f,-0.707107f,-0.707107f),
            new Vector3(0.000000f,-0.831470f,-0.555570f),
            new Vector3(0.000000f,-0.923880f,-0.382683f),
            new Vector3(0.000000f,-0.980785f,-0.195090f),
            new Vector3(0.000000f,-0.980785f,0.195090f),
            new Vector3(0.000000f,-0.923880f,0.382683f),
            new Vector3(0.000000f,-0.831470f,0.555570f),
            new Vector3(0.000000f,-0.707107f,0.707107f),
            new Vector3(0.000000f,-0.555570f,0.831470f),
            new Vector3(0.000000f,-0.382683f,0.923880f),
            new Vector3(0.000000f,-0.195090f,0.980785f),
            new Vector3(0.000000f,0.195090f,0.980785f),
            new Vector3(0.000000f,0.382683f,0.923880f),
            new Vector3(0.000000f,0.555570f,0.831470f),
            new Vector3(0.000000f,0.707107f,0.707107f),
            new Vector3(-0.000000f,0.831470f,0.555570f),
            new Vector3(-0.000000f,0.923880f,0.382683f),
            new Vector3(-0.000000f,0.980785f,0.195090f),
        };

        public static void DrawBoundingSphere(string id, BoundingSphere sphere, Vector4 col)
        {
            const uint vertexCount = 90;

            uint color = ColorConvertFloat4ToU32(col);
            if (queue.Draw(id, PrimitiveTopology.LineList, 96 * 2, vertexCount, out var cmd))
            {
                cmd.Indices = AllocCopyT(new ushort[]
                {
0,1,
1,2,
2,3,
3,4,
4,5,
5,6,
6,7,
7,8,
8,9,
9,10,
10,11,
11,12,
12,13,
13,14,
14,15,
15,16,
16,17,
17,18,
18,19,
19,20,
20,21,
21,22,
22,23,
23,24,
24,25,
25,26,
26,27,
27,28,
28,29,
29,30,
31,0,
33,32,
34,33,
35,34,
36,35,
37,36,
38,37,
39,38,
40,23,
41,40,
42,41,
43,42,
44,43,
45,44,
46,45,
47,46,
48,47,
49,48,
50,49,
51,50,
52,51,
53,52,
54,53,
55,7,
56,55,
57,56,
58,57,
59,58,
60,59,
61,60,
62,32,
63,62,
64,63,
65,64,
66,65,
67,66,
15,68,
69,15,
70,69,
71,70,
72,71,
73,72,
74,73,
47,75,
76,47,
77,76,
78,77,
79,78,
80,79,
81,80,
82,81,
83,31,
84,83,
85,84,
86,85,
87,86,
88,87,
32,89,
7,54,
68,67,
23,39,
89,88,
30,31,
31,82,
75,74,
32,61,
            });
                cmd.Vertices = AllocT<DebugDrawVert>(vertexCount);
            }

            TransformWithColor(cmd.Vertices, spherePositions, vertexCount, Matrix4x4.CreateScale(sphere.Radius) * Matrix4x4.CreateTranslation(sphere.Center), color);
        }

        public static void DrawRay(string id, Vector3 origin, Vector3 direction, bool normalize, Vector4 col)
        {
            uint color = ColorConvertFloat4ToU32(col);
            if (queue.Draw(id, PrimitiveTopology.LineStrip, 3, 3, out var cmd))
            {
                cmd.Vertices = AllocT<DebugDrawVert>(3);
                cmd.Indices = AllocCopyT(new ushort[] { 0, 1, 2 });
            }

            cmd.Vertices[0].Position = origin;

            Vector3 normDirection = Vector3.Normalize(direction);
            Vector3 rayDirection = normalize ? normDirection : direction;

            Vector3 perpVector = Vector3.Cross(normDirection, Vector3.UnitY);

            if (perpVector.LengthSquared() == 0f)
            {
                perpVector = Vector3.Cross(normDirection, Vector3.UnitZ);
            }
            perpVector = Vector3.Normalize(perpVector);

            cmd.Vertices[1].Position = rayDirection + origin;
            perpVector *= 0.0625f;
            normDirection *= -0.25f;
            rayDirection = perpVector + rayDirection;
            rayDirection = normDirection + rayDirection;
            cmd.Vertices[2].Position = rayDirection + origin;

            cmd.Vertices[0].Color = color;
            cmd.Vertices[1].Color = color;
            cmd.Vertices[2].Color = color;
        }

        public static void DrawLine(string id, Vector3 origin, Vector3 direction, bool normalize, Vector4 col)
        {
            uint color = ColorConvertFloat4ToU32(col);
            if (queue.Draw(id, PrimitiveTopology.LineStrip, 2, 2, out var cmd))
            {
                cmd.Vertices = AllocT<DebugDrawVert>(2);
                cmd.Indices = AllocCopyT(new ushort[] { 0, 1 });
            }

            cmd.Vertices[0].Position = origin;

            Vector3 normDirection = Vector3.Normalize(direction);
            Vector3 rayDirection = normalize ? normDirection : direction;

            cmd.Vertices[1].Position = rayDirection + origin;

            cmd.Vertices[0].Color = color;
            cmd.Vertices[1].Color = color;
        }

        public static void DrawLine(string id, Vector3 origin, Vector3 destination, Vector4 col)
        {
            uint color = ColorConvertFloat4ToU32(col);
            if (queue.Draw(id, PrimitiveTopology.LineStrip, 2, 2, out var cmd))
            {
                cmd.Vertices = AllocT<DebugDrawVert>(2);
                cmd.Indices = AllocCopyT(new ushort[] { 0, 1 });
            }

            cmd.Vertices[0].Position = origin;
            cmd.Vertices[1].Position = destination;

            cmd.Vertices[0].Color = color;
            cmd.Vertices[1].Color = color;
        }

        public static void DrawRing(string id, Vector3 origin, Quaternion orientation, Vector3 majorAxis, Vector3 minorAxis, Vector4 col)
        {
            uint color = ColorConvertFloat4ToU32(col);
            const int c_ringSegments = 32;

            if (queue.Draw(id, PrimitiveTopology.LineStrip, c_ringSegments + 1, c_ringSegments + 1, out var cmd))
            {
                cmd.Vertices = AllocT<DebugDrawVert>(c_ringSegments + 1);
                cmd.Indices = AllocT<ushort>(c_ringSegments + 1);
                for (ushort i = 0; i < c_ringSegments + 1; i++)
                {
                    cmd.Indices[i] = i;
                }
            }

            float fAngleDelta = MathUtil.PI2 / c_ringSegments;

            // Instead of calling cos/sin for each segment we calculate
            // the sign of the angle delta and then incrementally calculate sin
            // and cosine from then on.
            Vector3 cosDelta = new(MathF.Cos(fAngleDelta));
            Vector3 sinDelta = new(MathF.Sin(fAngleDelta));
            Vector3 incrementalSin = Vector3.Zero;
            Vector3 incrementalCos = new(1.0f, 1.0f, 1.0f);
            for (int i = 0; i < c_ringSegments; i++)
            {
                Vector3 pos = majorAxis * incrementalCos;
                pos = minorAxis * incrementalSin + pos;
                cmd.Vertices[i].Position = Vector3.Transform(pos, orientation) + origin;
                cmd.Vertices[i].Color = color;
                // Standard formula to rotate a vector.
                Vector3 newCos = incrementalCos * cosDelta - incrementalSin * sinDelta;
                Vector3 newSin = incrementalCos * sinDelta + incrementalSin * cosDelta;
                incrementalCos = newCos;
                incrementalSin = newSin;
            }
            cmd.Vertices[c_ringSegments] = cmd.Vertices[0];
        }

        public static void DrawRing(string id, Vector3 origin, Vector3 majorAxis, Vector3 minorAxis, Vector4 col)
        {
            uint color = ColorConvertFloat4ToU32(col);
            const int c_ringSegments = 32;

            if (queue.Draw(id, PrimitiveTopology.LineStrip, c_ringSegments + 1, c_ringSegments + 1, out var cmd))
            {
                cmd.Vertices = AllocT<DebugDrawVert>(c_ringSegments + 1);
                cmd.Indices = AllocT<ushort>(c_ringSegments + 1);
                for (ushort i = 0; i < c_ringSegments + 1; i++)
                {
                    cmd.Indices[i] = i;
                }
            }

            float fAngleDelta = MathUtil.PI2 / c_ringSegments;

            // Instead of calling cos/sin for each segment we calculate
            // the sign of the angle delta and then incrementally calculate sin
            // and cosine from then on.
            Vector3 cosDelta = new(MathF.Cos(fAngleDelta));
            Vector3 sinDelta = new(MathF.Sin(fAngleDelta));
            Vector3 incrementalSin = Vector3.Zero;
            Vector3 incrementalCos = new(1.0f, 1.0f, 1.0f);
            for (int i = 0; i < c_ringSegments; i++)
            {
                Vector3 pos = majorAxis * incrementalCos + origin;
                pos = minorAxis * incrementalSin + pos;
                cmd.Vertices[i].Position = pos;
                cmd.Vertices[i].Color = color;
                // Standard formula to rotate a vector.
                Vector3 newCos = incrementalCos * cosDelta - incrementalSin * sinDelta;
                Vector3 newSin = incrementalCos * sinDelta + incrementalSin * cosDelta;
                incrementalCos = newCos;
                incrementalSin = newSin;
            }
            cmd.Vertices[c_ringSegments] = cmd.Vertices[0];
        }

        public static void DrawRing(string id, Vector3 origin, (Vector3, Vector3) ellipse, Vector4 col)
        {
            uint color = ColorConvertFloat4ToU32(col);
            const int c_ringSegments = 32;

            if (queue.Draw(id, PrimitiveTopology.LineStrip, c_ringSegments + 1, c_ringSegments + 1, out var cmd))
            {
                cmd.Vertices = AllocT<DebugDrawVert>(c_ringSegments + 1);
                cmd.Indices = AllocT<ushort>(c_ringSegments + 1);
                for (ushort i = 0; i < c_ringSegments + 1; i++)
                {
                    cmd.Indices[i] = i;
                }
            }

            Vector3 majorAxis = ellipse.Item1;
            Vector3 minorAxis = ellipse.Item2;
            float fAngleDelta = MathUtil.PI2 / c_ringSegments;

            // Instead of calling cos/sin for each segment we calculate
            // the sign of the angle delta and then incrementally calculate sin
            // and cosine from then on.
            Vector3 cosDelta = new(MathF.Cos(fAngleDelta));
            Vector3 sinDelta = new(MathF.Sin(fAngleDelta));
            Vector3 incrementalSin = Vector3.Zero;
            Vector3 incrementalCos = new(1.0f, 1.0f, 1.0f);
            for (int i = 0; i < c_ringSegments; i++)
            {
                Vector3 pos = majorAxis * incrementalCos + origin;
                pos = minorAxis * incrementalSin + pos;
                cmd.Vertices[i].Position = pos;
                cmd.Vertices[i].Color = color;
                // Standard formula to rotate a vector.
                Vector3 newCos = incrementalCos * cosDelta - incrementalSin * sinDelta;
                Vector3 newSin = incrementalCos * sinDelta + incrementalSin * cosDelta;
                incrementalCos = newCos;
                incrementalSin = newSin;
            }
            cmd.Vertices[c_ringSegments] = cmd.Vertices[0];
        }

        public static void DrawBox(string id, Vector3 origin, Quaternion orientation, float width, float height, float depth, Vector4 col)
        {
            const uint vertexCount = 8;
            Vector3[] pos =
          {
new Vector3(-1, +1, -1),
new Vector3(-1, -1, -1),
new Vector3(+1, -1, -1),
new Vector3(+1, +1, -1),
new Vector3(-1, +1, +1),
new Vector3(-1, -1, +1),
new Vector3(+1, -1, +1),
new Vector3(+1, +1, +1),
            };
            uint color = ColorConvertFloat4ToU32(col);
            if (queue.Draw(id, PrimitiveTopology.LineList, 24, vertexCount, out var cmd))
            {
                cmd.Indices = AllocCopyT(new ushort[] { 0, 1, 1, 2, 2, 3, 3, 0, 0, 4, 1, 5, 2, 6, 3, 7, 4, 5, 5, 6, 6, 7, 7, 4 });
                cmd.Vertices = AllocT<DebugDrawVert>(vertexCount);
            }

            TransformWithColor(cmd.Vertices, pos, vertexCount, Matrix4x4.CreateScale(width, height, depth) * Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(origin), color);
        }

        public static void DrawSphere(string id, Vector3 origin, Quaternion orientation, float radius, Vector4 col)
        {
            const uint vertexCount = 90;
            uint color = ColorConvertFloat4ToU32(col);
            if (queue.Draw(id, PrimitiveTopology.LineList, 96 * 2, vertexCount, out var cmd))
            {
                cmd.Indices = AllocCopyT(new ushort[] { 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13, 14, 14, 15, 15, 16, 16, 17, 17, 18, 18, 19, 19, 20, 20, 21, 21, 22, 22, 23, 23, 24, 24, 25, 25, 26, 26, 27, 27, 28, 28, 29, 29, 30, 31, 0, 33, 32, 34, 33, 35, 34, 36, 35, 37, 36, 38, 37, 39, 38, 40, 23, 41, 40, 42, 41, 43, 42, 44, 43, 45, 44, 46, 45, 47, 46, 48, 47, 49, 48, 50, 49, 51, 50, 52, 51, 53, 52, 54, 53, 55, 7, 56, 55, 57, 56, 58, 57, 59, 58, 60, 59, 61, 60, 62, 32, 63, 62, 64, 63, 65, 64, 66, 65, 67, 66, 15, 68, 69, 15, 70, 69, 71, 70, 72, 71, 73, 72, 74, 73, 47, 75, 76, 47, 77, 76, 78, 77, 79, 78, 80, 79, 81, 80, 82, 81, 83, 31, 84, 83, 85, 84, 86, 85, 87, 86, 88, 87, 32, 89, 7, 54, 68, 67, 23, 39, 89, 88, 30, 31, 31, 82, 75, 74, 32, 61 });
                cmd.Vertices = AllocT<DebugDrawVert>(vertexCount);
            }

            TransformWithColor(cmd.Vertices, spherePositions, vertexCount, Matrix4x4.CreateScale(radius) * Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(origin), color);
        }

        private static readonly Vector3[] capsulePositions =
        {
            new Vector3(0.000000f,-0.500000f,1.000000f),
            new Vector3(-0.195090f,-0.500000f,0.980785f),
            new Vector3(-0.382683f,-0.500000f,0.923879f),
            new Vector3(-0.555570f,-0.500000f,0.831469f),
            new Vector3(-0.707107f,-0.500000f,0.707107f),
            new Vector3(-0.831470f,-0.500000f,0.555570f),
            new Vector3(-0.923879f,-0.500000f,0.382683f),
            new Vector3(-0.980785f,-0.500000f,0.195090f),
            new Vector3(-1.000000f,-0.500000f,0.000000f),
            new Vector3(-0.980785f,-0.500000f,-0.195090f),
            new Vector3(-0.923879f,-0.500000f,-0.382683f),
            new Vector3(-0.831470f,-0.500000f,-0.555570f),
            new Vector3(-0.707107f,-0.500000f,-0.707107f),
            new Vector3(-0.555570f,-0.500000f,-0.831469f),
            new Vector3(-0.382683f,-0.500000f,-0.923879f),
            new Vector3(-0.195090f,-0.500000f,-0.980785f),
            new Vector3(0.000000f,-0.500000f,-1.000000f),
            new Vector3(0.195090f,-0.500000f,-0.980785f),
            new Vector3(0.382684f,-0.500000f,-0.923879f),
            new Vector3(0.555570f,-0.500000f,-0.831469f),
            new Vector3(0.707107f,-0.500000f,-0.707107f),
            new Vector3(0.831470f,-0.500000f,-0.555570f),
            new Vector3(0.923880f,-0.500000f,-0.382683f),
            new Vector3(0.980785f,-0.500000f,-0.195090f),
            new Vector3(1.000000f,-0.500000f,0.000000f),
            new Vector3(0.980785f,-0.500000f,0.195090f),
            new Vector3(0.923880f,-0.500000f,0.382683f),
            new Vector3(0.831470f,-0.500000f,0.555570f),
            new Vector3(0.707107f,-0.500000f,0.707107f),
            new Vector3(0.555570f,-0.500000f,0.831469f),
            new Vector3(0.382684f,-0.500000f,0.923879f),
            new Vector3(0.195090f,-0.500000f,0.980785f),
            new Vector3(0.000000f,0.500000f,1.000000f),
            new Vector3(-0.195090f,0.500000f,0.980785f),
            new Vector3(-0.382684f,0.500000f,0.923879f),
            new Vector3(-0.555570f,0.500000f,0.831470f),
            new Vector3(-0.707107f,0.500000f,0.707107f),
            new Vector3(-0.831470f,0.500000f,0.555570f),
            new Vector3(-0.923880f,0.500000f,0.382683f),
            new Vector3(-0.980785f,0.500000f,0.195090f),
            new Vector3(-1.000000f,0.500000f,0.000000f),
            new Vector3(-0.980785f,0.500000f,-0.195090f),
            new Vector3(-0.923880f,0.500000f,-0.382683f),
            new Vector3(-0.831470f,0.500000f,-0.555570f),
            new Vector3(-0.707107f,0.500000f,-0.707107f),
            new Vector3(-0.555570f,0.500000f,-0.831470f),
            new Vector3(-0.382684f,0.500000f,-0.923879f),
            new Vector3(-0.195090f,0.500000f,-0.980785f),
            new Vector3(0.000000f,0.500000f,-1.000000f),
            new Vector3(0.195090f,0.500000f,-0.980785f),
            new Vector3(0.382683f,0.500000f,-0.923879f),
            new Vector3(0.555570f,0.500000f,-0.831470f),
            new Vector3(0.707107f,0.500000f,-0.707107f),
            new Vector3(0.831470f,0.500000f,-0.555570f),
            new Vector3(0.923879f,0.500000f,-0.382683f),
            new Vector3(0.980785f,0.500000f,-0.195090f),
            new Vector3(1.000000f,0.500000f,-0.000000f),
            new Vector3(0.980785f,0.500000f,0.195090f),
            new Vector3(0.923879f,0.500000f,0.382683f),
            new Vector3(0.831470f,0.500000f,0.555570f),
            new Vector3(0.707107f,0.500000f,0.707107f),
            new Vector3(0.555570f,0.500000f,0.831470f),
            new Vector3(0.382683f,0.500000f,0.923879f),
            new Vector3(0.195090f,0.500000f,0.980785f),
            new Vector3(0.000000f,-0.597545f,-0.980785f),
            new Vector3(0.000000f,-0.691342f,-0.923879f),
            new Vector3(0.000000f,-0.777785f,-0.831470f),
            new Vector3(0.000000f,-0.853553f,-0.707107f),
            new Vector3(0.000000f,-0.915735f,-0.555570f),
            new Vector3(0.000000f,-0.961940f,-0.382683f),
            new Vector3(0.000000f,-0.990393f,-0.195090f),
            new Vector3(0.000000f,-1.000000f,-0.000000f),
            new Vector3(0.000000f,-0.990393f,0.195090f),
            new Vector3(0.000000f,-0.961940f,0.382683f),
            new Vector3(0.000000f,-0.915735f,0.555570f),
            new Vector3(0.000000f,-0.853553f,0.707107f),
            new Vector3(0.000000f,-0.777785f,0.831470f),
            new Vector3(0.000000f,-0.691342f,0.923879f),
            new Vector3(0.000000f,-0.597545f,0.980785f),
            new Vector3(0.000000f,-1.000000f,-0.000000f),
            new Vector3(-0.195090f,-0.990393f,-0.000000f),
            new Vector3(-0.382683f,-0.961940f,-0.000000f),
            new Vector3(-0.555570f,-0.915735f,-0.000000f),
            new Vector3(-0.707107f,-0.853553f,-0.000000f),
            new Vector3(-0.831470f,-0.777785f,-0.000000f),
            new Vector3(-0.923879f,-0.691342f,-0.000000f),
            new Vector3(-0.980785f,-0.597545f,-0.000000f),
            new Vector3(0.980785f,-0.597545f,-0.000000f),
            new Vector3(0.923880f,-0.691342f,-0.000000f),
            new Vector3(0.831470f,-0.777785f,-0.000000f),
            new Vector3(0.707107f,-0.853553f,-0.000000f),
            new Vector3(0.555570f,-0.915735f,-0.000000f),
            new Vector3(0.382684f,-0.961940f,-0.000000f),
            new Vector3(0.195090f,-0.990393f,-0.000000f),
            new Vector3(-0.000000f,0.597545f,0.980785f),
            new Vector3(-0.000000f,0.691342f,0.923879f),
            new Vector3(-0.000000f,0.777785f,0.831470f),
            new Vector3(-0.000000f,0.853553f,0.707107f),
            new Vector3(-0.000000f,0.915735f,0.555570f),
            new Vector3(-0.000000f,0.961940f,0.382683f),
            new Vector3(-0.000000f,0.990393f,0.195090f),
            new Vector3(0.000000f,1.000000f,-0.000000f),
            new Vector3(-0.000000f,0.990393f,-0.195090f),
            new Vector3(-0.000000f,0.961940f,-0.382683f),
            new Vector3(-0.000000f,0.915735f,-0.555570f),
            new Vector3(-0.000000f,0.853553f,-0.707107f),
            new Vector3(-0.000000f,0.777785f,-0.831470f),
            new Vector3(-0.000000f,0.691342f,-0.923879f),
            new Vector3(-0.000000f,0.597545f,-0.980785f),
            new Vector3(0.000000f,1.000000f,-0.000000f),
            new Vector3(-0.195090f,0.990393f,-0.000000f),
            new Vector3(-0.382684f,0.961940f,-0.000000f),
            new Vector3(-0.555570f,0.915735f,-0.000000f),
            new Vector3(-0.707107f,0.853553f,-0.000000f),
            new Vector3(-0.831470f,0.777785f,0.000000f),
            new Vector3(-0.923880f,0.691342f,0.000000f),
            new Vector3(-0.980785f,0.597545f,0.000000f),
            new Vector3(0.980785f,0.597545f,-0.000000f),
            new Vector3(0.923879f,0.691342f,-0.000000f),
            new Vector3(0.831470f,0.777785f,-0.000000f),
            new Vector3(0.707107f,0.853553f,-0.000000f),
            new Vector3(0.555570f,0.915735f,-0.000000f),
            new Vector3(0.382683f,0.961940f,-0.000000f),
            new Vector3(0.195090f,0.990393f,-0.000000f),
        };

        public static void DrawCapsule(string id, Vector3 origin, Quaternion orientation, float radius, float length, Vector4 col)
        {
            const uint vertexCount = 124;

            uint color = ColorConvertFloat4ToU32(col);
            if (queue.Draw(id, PrimitiveTopology.LineList, 132 * 2, vertexCount, out var cmd))
            {
                cmd.Indices = AllocCopyT(new ushort[]
                {
1,0,
3,2,
5,4,
7,6,
9,8,
11,10,
13,12,
15,14,
17,16,
19,18,
21,20,
23,22,
25,24,
27,26,
29,28,
31,30,
0,32,
33,32,
35,34,
37,36,
39,38,
8,40,
41,40,
43,42,
45,44,
47,46,
16,48,
49,48,
51,50,
53,52,
55,54,
24,56,
57,56,
59,58,
61,60,
63,62,
64,16,
65,64,
67,66,
69,68,
71,70,
73,72,
75,74,
77,76,
0,78,
81,80,
83,82,
85,84,
8,86,
87,24,
89,88,
91,90,
93,92,
94,32,
95,94,
97,96,
99,98,
101,100,
103,102,
105,104,
107,106,
48,108,
111,110,
113,112,
115,114,
40,116,
117,56,
119,118,
121,120,
123,122,
0,31,
32,63,
2,1,
4,3,
6,5,
8,7,
10,9,
12,11,
14,13,
16,15,
18,17,
20,19,
22,21,
24,23,
26,25,
28,27,
30,29,
34,33,
36,35,
38,37,
40,39,
42,41,
44,43,
46,45,
48,47,
50,49,
52,51,
54,53,
56,55,
58,57,
60,59,
62,61,
66,65,
68,67,
70,69,
72,71,
74,73,
76,75,
78,77,
80,79,
82,81,
84,83,
86,85,
88,87,
90,89,
92,91,
79,93,
96,95,
98,97,
100,99,
102,101,
104,103,
106,105,
108,107,
110,109,
112,111,
114,113,
116,115,
118,117,
120,119,
122,121,
109,123,
            });
                cmd.Vertices = AllocT<DebugDrawVert>(vertexCount);
            }

            TransformWithColor(cmd.Vertices, capsulePositions, vertexCount, Matrix4x4.CreateScale(radius, length, radius) * Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(origin), color);
        }

        private static readonly Vector3[] cylinderPositions =
        {
            new Vector3(0.000000f,1.000000f,1.000000f),
            new Vector3(0.195090f,1.000000f,0.980785f),
            new Vector3(0.382683f,1.000000f,0.923880f),
            new Vector3(0.555570f,1.000000f,0.831470f),
            new Vector3(0.707107f,1.000000f,0.707107f),
            new Vector3(0.831470f,1.000000f,0.555570f),
            new Vector3(0.923880f,1.000000f,0.382683f),
            new Vector3(0.980785f,1.000000f,0.195090f),
            new Vector3(1.000000f,1.000000f,0.000000f),
            new Vector3(0.980785f,1.000000f,-0.195090f),
            new Vector3(0.923880f,1.000000f,-0.382683f),
            new Vector3(0.831470f,1.000000f,-0.555570f),
            new Vector3(0.707107f,1.000000f,-0.707107f),
            new Vector3(0.555570f,1.000000f,-0.831470f),
            new Vector3(0.382683f,1.000000f,-0.923880f),
            new Vector3(0.195090f,1.000000f,-0.980785f),
            new Vector3(0.000000f,1.000000f,-1.000000f),
            new Vector3(-0.195090f,1.000000f,-0.980785f),
            new Vector3(-0.382683f,1.000000f,-0.923880f),
            new Vector3(-0.555570f,1.000000f,-0.831470f),
            new Vector3(-0.707107f,1.000000f,-0.707107f),
            new Vector3(-0.831470f,1.000000f,-0.555570f),
            new Vector3(-0.923880f,1.000000f,-0.382683f),
            new Vector3(-0.980785f,1.000000f,-0.195090f),
            new Vector3(-1.000000f,1.000000f,0.000000f),
            new Vector3(-0.980785f,1.000000f,0.195090f),
            new Vector3(-0.923880f,1.000000f,0.382683f),
            new Vector3(-0.831470f,1.000000f,0.555570f),
            new Vector3(-0.707107f,1.000000f,0.707107f),
            new Vector3(-0.555570f,1.000000f,0.831470f),
            new Vector3(-0.382683f,1.000000f,0.923880f),
            new Vector3(-0.195090f,1.000000f,0.980785f),
            new Vector3(0.000000f,-1.000000f,1.000000f),
            new Vector3(0.195090f,-1.000000f,0.980785f),
            new Vector3(0.382683f,-1.000000f,0.923880f),
            new Vector3(0.555570f,-1.000000f,0.831470f),
            new Vector3(0.707107f,-1.000000f,0.707107f),
            new Vector3(0.831470f,-1.000000f,0.555570f),
            new Vector3(0.923880f,-1.000000f,0.382683f),
            new Vector3(0.980785f,-1.000000f,0.195090f),
            new Vector3(1.000000f,-1.000000f,0.000000f),
            new Vector3(0.980785f,-1.000000f,-0.195090f),
            new Vector3(0.923880f,-1.000000f,-0.382683f),
            new Vector3(0.831470f,-1.000000f,-0.555570f),
            new Vector3(0.707107f,-1.000000f,-0.707107f),
            new Vector3(0.555570f,-1.000000f,-0.831470f),
            new Vector3(0.382683f,-1.000000f,-0.923880f),
            new Vector3(0.195090f,-1.000000f,-0.980785f),
            new Vector3(0.000000f,-1.000000f,-1.000000f),
            new Vector3(-0.195090f,-1.000000f,-0.980785f),
            new Vector3(-0.382683f,-1.000000f,-0.923880f),
            new Vector3(-0.555570f,-1.000000f,-0.831470f),
            new Vector3(-0.707107f,-1.000000f,-0.707107f),
            new Vector3(-0.831470f,-1.000000f,-0.555570f),
            new Vector3(-0.923880f,-1.000000f,-0.382683f),
            new Vector3(-0.980785f,-1.000000f,-0.195090f),
            new Vector3(-1.000000f,-1.000000f,0.000000f),
            new Vector3(-0.980785f,-1.000000f,0.195090f),
            new Vector3(-0.923880f,-1.000000f,0.382683f),
            new Vector3(-0.831470f,-1.000000f,0.555570f),
            new Vector3(-0.707107f,-1.000000f,0.707107f),
            new Vector3(-0.555570f,-1.000000f,0.831470f),
            new Vector3(-0.382683f,-1.000000f,0.923880f),
            new Vector3(-0.195090f,-1.000000f,0.980785f),
        };

        public static void DrawCylinder(string id, Vector3 origin, Quaternion orientation, float radius, float length, Vector4 col)
        {
            const uint vertexCount = 64;

            uint color = ColorConvertFloat4ToU32(col);
            if (queue.Draw(id, PrimitiveTopology.LineList, 68 * 2, vertexCount, out var cmd))
            {
                cmd.Indices = AllocCopyT(new ushort[]
                {
1,0,
2,1,
3,2,
4,3,
5,4,
6,5,
7,6,
8,7,
9,8,
10,9,
11,10,
12,11,
13,12,
14,13,
15,14,
16,15,
17,16,
18,17,
19,18,
20,19,
21,20,
22,21,
23,22,
24,23,
25,24,
26,25,
27,26,
28,27,
29,28,
30,29,
31,30,
0,32,
33,32,
34,33,
35,34,
36,35,
37,36,
38,37,
39,38,
8,40,
41,40,
42,41,
43,42,
44,43,
45,44,
46,45,
47,46,
48,47,
49,48,
50,49,
51,50,
52,51,
53,52,
54,53,
55,54,
56,55,
57,56,
58,57,
59,58,
60,59,
61,60,
62,61,
63,62,
40,39,
16,48,
0,31,
32,63,
24,56,
            });
                cmd.Vertices = AllocT<DebugDrawVert>(vertexCount);
            }

            TransformWithColor(cmd.Vertices, cylinderPositions, vertexCount, Matrix4x4.CreateScale(radius, length, radius) * Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(origin), color);
        }

        public static void DrawTriangle(string id, Vector3 origin, Quaternion orientation, Vector3 a, Vector3 b, Vector3 c, Vector4 col)
        {
            const uint vertexCount = 4;
            uint color = ColorConvertFloat4ToU32(col);
            if (queue.Draw(id, PrimitiveTopology.LineStrip, 4, vertexCount, out var cmd))
            {
                cmd.Vertices = AllocT<DebugDrawVert>(vertexCount);
                cmd.Indices = AllocCopyT(new ushort[] { 0, 1, 2, 3 });
            }

            cmd.Vertices[0] = new(Vector3.Transform(a, orientation) + origin, Vector2.Zero, color);
            cmd.Vertices[1] = new(Vector3.Transform(b, orientation) + origin, Vector2.Zero, color);
            cmd.Vertices[2] = new(Vector3.Transform(c, orientation) + origin, Vector2.Zero, color);
            cmd.Vertices[3] = cmd.Vertices[0];
        }

        public static void DrawGrid(string id, Matrix4x4 matrix, int size, Vector4 col)
        {
            uint vertexCount = 2u * (uint)size * 2u + 4;
            uint color = ColorConvertFloat4ToU32(col);
            if (queue.Draw(id, PrimitiveTopology.LineList, vertexCount, vertexCount, out var cmd))
            {
                cmd.EnableDepth = true;
                cmd.Vertices = AllocT<DebugDrawVert>(vertexCount);
                cmd.Indices = AllocT<ushort>(vertexCount);

                int half = size / 2;

                int i = 0;
                for (int x = -half; x <= half; x++)
                {
                    var pos0 = Vector3.Transform(new Vector3(x, 0, -half), matrix);
                    var pos1 = Vector3.Transform(new Vector3(x, 0, half), matrix);
                    cmd.Vertices[i] = new(pos0, Vector2.Zero, color);
                    cmd.Vertices[i + 1] = new(pos1, Vector2.Zero, color);
                    cmd.Indices[i] = (ushort)i;
                    cmd.Indices[i + 1] = (ushort)(i + 1);
                    i += 2;
                }

                for (int z = -half; z <= half; z++)
                {
                    var pos0 = Vector3.Transform(new Vector3(-half, 0, z), matrix);
                    var pos1 = Vector3.Transform(new Vector3(half, 0, z), matrix);
                    cmd.Vertices[i] = new(pos0, Vector2.Zero, color);
                    cmd.Vertices[i + 1] = new(pos1, Vector2.Zero, color);
                    cmd.Indices[i] = (ushort)i;
                    cmd.Indices[i + 1] = (ushort)(i + 1);
                    i += 2;
                }
            }

            {
                int half = size / 2;

                int i = 0;
                for (int x = -half; x <= half; x++)
                {
                    var pos0 = Vector3.Transform(new Vector3(x, 0, -half), matrix);
                    var pos1 = Vector3.Transform(new Vector3(x, 0, half), matrix);
                    cmd.Vertices[i] = new(pos0, Vector2.Zero, color);
                    cmd.Vertices[i + 1] = new(pos1, Vector2.Zero, color);
                    i += 2;
                }

                for (int z = -half; z <= half; z++)
                {
                    var pos0 = Vector3.Transform(new Vector3(-half, 0, z), matrix);
                    var pos1 = Vector3.Transform(new Vector3(half, 0, z), matrix);
                    cmd.Vertices[i] = new(pos0, Vector2.Zero, color);
                    cmd.Vertices[i + 1] = new(pos1, Vector2.Zero, color);
                    i += 2;
                }
            }
        }
    }
}