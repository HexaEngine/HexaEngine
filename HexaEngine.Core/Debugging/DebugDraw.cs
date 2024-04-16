#nullable disable

namespace HexaEngine.Core.Debugging
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System;
    using System.Drawing;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides a utility for immediate mode debugging to draw 3D shapes.
    /// </summary>
    public static unsafe class DebugDraw
    {
        private static readonly DebugDrawCommandList immediateList = new(DebugDrawCommandListType.Immediate);
        private static readonly DebugDrawData drawData = new();

        private static Viewport viewport;
        private static Matrix4x4 camera;

        private const int COL32_R_SHIFT = 0;
        private const int COL32_G_SHIFT = 8;
        private const int COL32_B_SHIFT = 16;
        private const int COL32_A_SHIFT = 24;
        private const uint COL32_A_MASK = 0xFF000000;

        static DebugDraw()
        {
            drawData.CmdLists.Add(immediateList);
        }

        /// <summary>
        /// Saturates a float value to the range [0, 1].
        /// </summary>
        /// <param name="f">The input float value to saturate.</param>
        /// <returns>The saturated float value within the [0, 1] range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Saturate(float f)
        {
            return f < 0.0f ? 0.0f : f > 1.0f ? 1.0f : f;
        }

        /// <summary>
        /// Converts a floating-point value to an 8-bit integer value within the range [0, 255].
        /// </summary>
        /// <param name="val">The input floating-point value to convert.</param>
        /// <returns>The 8-bit integer value within the [0, 255] range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int F32ToInt8Sat(float val)
        {
            return (int)(Saturate(val) * 255.0f + 0.5f);
        }

        /// <summary>
        /// Converts a floating-point vector (Vector4) color to a packed 32-bit color representation.
        /// </summary>
        /// <param name="i">The input floating-point color as a Vector4.</param>
        /// <returns>The packed 32-bit color representation.</returns>
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
        private static void DrawPreComputed(PrimitiveTopology topology, Vector3[] positions, uint[] indices, Matrix4x4 matrix, Vector4 color)
        {
            immediateList.BeginDraw();
            immediateList.AddIndexRange(indices);
            immediateList.ReserveVerts((uint)positions.Length);
            var col = ColorConvertFloat4ToU32(color);
            DebugDrawVert* verts = immediateList.Vertices + immediateList.VertexCount;
            for (uint i = 0; i < positions.Length; i++)
            {
                verts[i].Position = Vector3.Transform(positions[i], matrix);
                verts[i].Color = col;
                verts[i].UV = WhiteUV;
            }
            immediateList.RecordCmd(topology);
        }

        /// <summary>
        /// Clears the draw commands and prepares for a new frame of drawing.
        /// </summary>
        public static void NewFrame()
        {
            drawData.CmdLists.Clear();
            drawData.CmdLists.Add(immediateList);
            immediateList.NewFrame();
        }

        /// <summary>
        /// Renders the accumulated draw commands to the viewport.
        /// </summary>
        public static DebugDrawData Render()
        {
            for (int i = 0; i < drawData.CmdLists.Count; i++)
            {
                var list = drawData.CmdLists[i];
                list.EndFrame();
                drawData.TotalVertices += list.VertexCount;
                drawData.TotalIndices += list.IndexCount;
            }

            drawData.Viewport = viewport;
            drawData.Camera = camera;

            return drawData;
        }

        /// <summary>
        /// Sets the viewport for rendering debug shapes.
        /// </summary>
        /// <param name="viewport">The viewport to set.</param>
        public static void SetViewport(Viewport viewport)
        {
            DebugDraw.viewport = viewport;
        }

        /// <summary>
        /// Gets the current viewport used for rendering.
        /// </summary>
        /// <returns>The current viewport.</returns>
        public static Viewport GetViewport()
        {
            return viewport;
        }

        /// <summary>
        /// Sets the camera matrix for rendering debug shapes.
        /// </summary>
        /// <param name="camera">The camera matrix to set.</param>
        public static void SetCamera(Matrix4x4 camera)
        {
            DebugDraw.camera = camera;
        }

        /// <summary>
        /// Gets the current camera matrix used for rendering.
        /// </summary>
        /// <returns>The current camera matrix.</returns>
        public static Matrix4x4 GetCamera()
        {
            return camera;
        }

        /// <summary>
        /// Gets the queue of debug draw commands for immediate mode rendering.
        /// </summary>
        /// <returns>The debug draw command queue.</returns>
        public static DebugDrawCommandList GetImmediateCommandList()
        {
            return immediateList;
        }

        /// <summary>
        /// Executes a deferred debug draw command list.
        /// </summary>
        /// <param name="commandList">The debug draw command list to execute.</param>
        /// <exception cref="InvalidOperationException">Thrown if the provided command list is not of type Deferred.</exception>
        public static void ExecuteCommandList(DebugDrawCommandList commandList)
        {
            if (commandList.Type != DebugDrawCommandListType.Deferred)
            {
                throw new InvalidOperationException($"CommandList must be type of Deferred, but was {commandList.Type}");
            }

            drawData.CmdLists.Add(commandList);
        }

        /// <summary>
        /// Draws a bounding frustum in the specified color.
        /// </summary>
        /// <param name="frustum">The bounding frustum to be drawn.</param>
        /// <param name="col">The color of the frustum.</param>
        ///
        public static void DrawFrustum(BoundingFrustum frustum, Vector4 col)
        {
            immediateList.BeginDraw();

            uint color = ColorConvertFloat4ToU32(col);

            immediateList.ReserveGeometry(BoundingFrustum.CornerCount, 24);
            var indices = immediateList.Indices + immediateList.IndexCount;
            var vertices = immediateList.Vertices + immediateList.VertexCount;

            indices[0] = 0; indices[1] = 1;
            indices[2] = 1; indices[3] = 2;
            indices[4] = 2; indices[5] = 3;
            indices[6] = 3; indices[7] = 0;
            indices[8] = 0; indices[9] = 4;
            indices[10] = 1; indices[11] = 5;
            indices[12] = 2; indices[13] = 6;
            indices[14] = 3; indices[15] = 7;
            indices[16] = 4; indices[17] = 5;
            indices[18] = 5; indices[19] = 6;
            indices[20] = 6; indices[21] = 7;
            indices[22] = 7; indices[23] = 4;

            var corners = frustum.Corners;
            for (int i = 0; i < BoundingFrustum.CornerCount; i++)
            {
                vertices[i].Color = color;
                vertices[i].Position = corners[i];
                vertices[i].UV = WhiteUV;
            }

            immediateList.RecordCmd(PrimitiveTopology.LineList);
        }

        /// <summary>
        /// Draws a bounding box in the specified color.
        /// </summary>
        /// <param name="box">The bounding box to be drawn.</param>
        /// <param name="col">The color of the box.</param>
        ///
        public static void DrawBoundingBox(BoundingBox box, Vector4 col)
        {
            immediateList.BeginDraw();

            const uint vertexCount = 8;
            uint color = ColorConvertFloat4ToU32(col);

            immediateList.ReserveGeometry(vertexCount, 24);
            var indices = immediateList.Indices + immediateList.IndexCount;
            var vertices = immediateList.Vertices + immediateList.VertexCount;

            indices[0] = 0; indices[1] = 1;
            indices[2] = 1; indices[3] = 2;
            indices[4] = 2; indices[5] = 3;
            indices[6] = 3; indices[7] = 0;
            indices[8] = 0; indices[9] = 4;
            indices[10] = 1; indices[11] = 5;
            indices[12] = 2; indices[13] = 6;
            indices[14] = 3; indices[15] = 7;
            indices[16] = 4; indices[17] = 5;
            indices[18] = 5; indices[19] = 6;
            indices[20] = 6; indices[21] = 7;
            indices[22] = 7; indices[23] = 4;

            vertices[0].Position = new Vector3(box.Min.X, box.Max.Y, box.Min.Z);
            vertices[1].Position = new Vector3(box.Min.X, box.Min.Y, box.Min.Z);
            vertices[2].Position = new Vector3(box.Max.X, box.Min.Y, box.Min.Z);
            vertices[3].Position = new Vector3(box.Max.X, box.Max.Y, box.Min.Z);
            vertices[4].Position = new Vector3(box.Min.X, box.Max.Y, box.Max.Z);
            vertices[5].Position = new Vector3(box.Min.X, box.Min.Y, box.Max.Z);
            vertices[6].Position = new Vector3(box.Max.X, box.Min.Y, box.Max.Z);
            vertices[7].Position = new Vector3(box.Max.X, box.Max.Y, box.Max.Z);

            for (int i = 0; i < vertexCount; i++)
            {
                vertices[i].Color = color;
                vertices[i].UV = WhiteUV;
            }

            immediateList.RecordCmd(PrimitiveTopology.LineList);
        }

        #region Sphere Data

        private static readonly Vector3[] spherePositions =
        [
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
        ];

        private static uint[] sphereIndices =
                [
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
            ];

        #endregion Sphere Data

        /// <summary>
        /// Draws a bounding sphere in the specified color.
        /// </summary>
        /// <param name="sphere">The bounding sphere to be drawn.</param>
        /// <param name="col">The color of the sphere.</param>
        ///
        public static void DrawBoundingSphere(BoundingSphere sphere, Vector4 col)
        {
            DrawPreComputed(PrimitiveTopology.LineList, spherePositions, sphereIndices, Matrix4x4.CreateScale(sphere.Radius) * Matrix4x4.CreateTranslation(sphere.Center), col);
        }

        /// <summary>
        /// Draws a ray starting from a specified origin and extending in a given direction.
        /// </summary>
        /// <param name="origin">The starting point of the ray.</param>
        /// <param name="direction">The direction in which the ray extends.</param>
        /// <param name="normalize">True if the direction vector should be normalized; otherwise, false.</param>
        /// <param name="col">The color of the ray.</param>
        ///
        public static void DrawRay(Vector3 origin, Vector3 direction, bool normalize, Vector4 col)
        {
            immediateList.BeginDraw();

            uint color = ColorConvertFloat4ToU32(col);

            immediateList.ReserveGeometry(3, 4);
            var indices = immediateList.Indices + immediateList.IndexCount;
            var vertices = immediateList.Vertices + immediateList.VertexCount;

            indices[0] = 0; indices[1] = 1;
            indices[2] = 1; indices[3] = 2;

            vertices[0].Position = origin;

            Vector3 normDirection = Vector3.Normalize(direction);
            Vector3 rayDirection = normalize ? normDirection : direction;

            Vector3 perpVector = Vector3.Cross(normDirection, Vector3.UnitY);

            if (perpVector.LengthSquared() == 0f)
            {
                perpVector = Vector3.Cross(normDirection, Vector3.UnitZ);
            }
            perpVector = Vector3.Normalize(perpVector);

            vertices[1].Position = rayDirection + origin;
            perpVector *= 0.0625f;
            normDirection *= -0.25f;
            rayDirection = perpVector + rayDirection;
            rayDirection = normDirection + rayDirection;
            vertices[2].Position = rayDirection + origin;

            vertices[0].Color = color;
            vertices[1].Color = color;
            vertices[2].Color = color;

            vertices[0].UV = WhiteUV;
            vertices[1].UV = WhiteUV;
            vertices[2].UV = WhiteUV;

            immediateList.RecordCmd(PrimitiveTopology.LineList);
        }

        /// <summary>
        /// Draws a line from a specified origin in the given direction.
        /// </summary>
        /// <param name="origin">The starting point of the line.</param>
        /// <param name="direction">The direction in which the line extends.</param>
        /// <param name="normalize">True if the direction vector should be normalized; otherwise, false.</param>
        /// <param name="col">The color of the line.</param>
        ///
        public static void DrawLine(Vector3 origin, Vector3 direction, bool normalize, Vector4 col)
        {
            immediateList.BeginDraw();

            uint color = ColorConvertFloat4ToU32(col);

            immediateList.ReserveGeometry(2, 2);

            var indices = immediateList.Indices + immediateList.IndexCount;
            var vertices = immediateList.Vertices + immediateList.VertexCount;

            indices[0] = 0;
            indices[1] = 1;

            vertices[0].Position = origin;

            Vector3 normDirection = Vector3.Normalize(direction);
            Vector3 rayDirection = normalize ? normDirection : direction;

            vertices[1].Position = rayDirection + origin;

            vertices[0].Color = color;
            vertices[1].Color = color;

            vertices[0].UV = WhiteUV;
            vertices[1].UV = WhiteUV;

            immediateList.RecordCmd(PrimitiveTopology.LineList);
        }

        /// <summary>
        /// Draws a line between a specified origin and destination points.
        /// </summary>
        /// <param name="origin">The starting point of the line.</param>
        /// <param name="destination">The ending point of the line.</param>
        /// <param name="col">The color of the line.</param>
        ///
        public static void DrawLine(Vector3 origin, Vector3 destination, Vector4 col)
        {
            immediateList.BeginDraw();

            uint color = ColorConvertFloat4ToU32(col);

            immediateList.ReserveGeometry(2, 2);
            var indices = immediateList.Indices + immediateList.IndexCount;
            var vertices = immediateList.Vertices + immediateList.VertexCount;

            indices[0] = 0;
            indices[1] = 1;

            vertices[0] = new(origin, default, color);
            vertices[1] = new(destination, default, color);

            immediateList.RecordCmd(PrimitiveTopology.LineList);
        }

        /// <summary>
        /// Draws a ring in 3D space at a specified position and orientation.
        /// </summary>
        /// <param name="origin">The center point of the ring.</param>
        /// <param name="orientation">The orientation (rotation) of the ring.</param>
        /// <param name="majorAxis">The major axis of the ring.</param>
        /// <param name="minorAxis">The minor axis of the ring.</param>
        /// <param name="col">The color of the ring.</param>
        ///
        public static void DrawRing(Vector3 origin, Quaternion orientation, Vector3 majorAxis, Vector3 minorAxis, Vector4 col)
        {
            immediateList.BeginDraw();

            uint color = ColorConvertFloat4ToU32(col);
            const int c_ringSegments = 32;

            immediateList.ReserveGeometry(c_ringSegments, c_ringSegments * 2);
            var indices = immediateList.Indices + immediateList.IndexCount;
            var vertices = immediateList.Vertices + immediateList.VertexCount;

            for (uint i = 0; i < c_ringSegments; i++)
            {
                indices[i * 2] = i;
                indices[i * 2 + 1] = i + 1;
            }

            indices[(c_ringSegments - 1) * 2 + 1] = 0;

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
                vertices[i].Position = Vector3.Transform(pos, orientation) + origin;
                vertices[i].UV = WhiteUV;
                vertices[i].Color = color;
                // Standard formula to rotate a vector.
                Vector3 newCos = incrementalCos * cosDelta - incrementalSin * sinDelta;
                Vector3 newSin = incrementalCos * sinDelta + incrementalSin * cosDelta;
                incrementalCos = newCos;
                incrementalSin = newSin;
            }

            immediateList.RecordCmd(PrimitiveTopology.LineList);
        }

        /// <summary>
        /// Draws a ring in 3D space at a specified position with given major and minor axes.
        /// </summary>
        /// <param name="origin">The center point of the ring.</param>
        /// <param name="majorAxis">The major axis of the ring.</param>
        /// <param name="minorAxis">The minor axis of the ring.</param>
        /// <param name="col">The color of the ring.</param>
        ///
        public static void DrawRing(Vector3 origin, Vector3 majorAxis, Vector3 minorAxis, Vector4 col)
        {
            immediateList.BeginDraw();

            uint color = ColorConvertFloat4ToU32(col);
            const int c_ringSegments = 32;

            immediateList.ReserveGeometry(c_ringSegments, c_ringSegments * 2);
            var indices = immediateList.Indices + immediateList.IndexCount;
            var vertices = immediateList.Vertices + immediateList.VertexCount;

            for (uint i = 0; i < c_ringSegments; i++)
            {
                indices[i * 2] = i;
                indices[i * 2 + 1] = i + 1;
            }

            indices[(c_ringSegments - 1) * 2 + 1] = 0;

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
                vertices[i].Position = pos;
                vertices[i].UV = WhiteUV;
                vertices[i].Color = color;
                // Standard formula to rotate a vector.
                Vector3 newCos = incrementalCos * cosDelta - incrementalSin * sinDelta;
                Vector3 newSin = incrementalCos * sinDelta + incrementalSin * cosDelta;
                incrementalCos = newCos;
                incrementalSin = newSin;
            }

            immediateList.RecordCmd(PrimitiveTopology.LineList);
        }

        /// <summary>
        /// Draws a ring in 3D space at a specified position with given major and minor axes.
        /// </summary>
        /// <param name="origin">The center point of the ring.</param>
        /// <param name="ellipse">The major axis and minor axis of the ring.</param>
        /// <param name="col">The color of the ring.</param>
        ///
        public static void DrawRing(Vector3 origin, (Vector3 majorAxis, Vector3 minorAxis) ellipse, Vector4 col)
        {
            immediateList.BeginDraw();

            uint color = ColorConvertFloat4ToU32(col);
            const int c_ringSegments = 32;

            immediateList.ReserveGeometry(c_ringSegments, c_ringSegments * 2);

            var indices = immediateList.Indices + immediateList.IndexCount;
            var vertices = immediateList.Vertices + immediateList.VertexCount;

            for (uint i = 0; i < c_ringSegments; i++)
            {
                indices[i * 2] = i;
                indices[i * 2 + 1] = i + 1;
            }

            indices[(c_ringSegments - 1) * 2 + 1] = 0;

            Vector3 majorAxis = ellipse.majorAxis;
            Vector3 minorAxis = ellipse.minorAxis;
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
                vertices[i].Position = pos;
                vertices[i].UV = WhiteUV;
                vertices[i].Color = color;
                // Standard formula to rotate a vector.
                Vector3 newCos = incrementalCos * cosDelta - incrementalSin * sinDelta;
                Vector3 newSin = incrementalCos * sinDelta + incrementalSin * cosDelta;
                incrementalCos = newCos;
                incrementalSin = newSin;
            }

            immediateList.RecordCmd(PrimitiveTopology.LineList);
        }

        /// <summary>
        /// Draws a ring-shaped billboard in 3D space.
        /// </summary>
        /// <param name="origin">The center of the ring.</param>
        /// <param name="camPos">Camera position in world space.</param>
        /// <param name="camUp">Camera up vector.</param>
        /// <param name="camForward">Camera forward vector.</param>
        /// <param name="ellipse">Tuple containing major and minor axes of the ellipse defining the ring.</param>
        /// <param name="col">Color of the ring.</param>
        ///
        public static void DrawRingBillboard(Vector3 origin, Vector3 camPos, Vector3 camUp, Vector3 camForward, (Vector3 majorAxis, Vector3 minorAxis) ellipse, Vector4 col)
        {
            immediateList.BeginDraw();

            uint color = ColorConvertFloat4ToU32(col);
            const int c_ringSegments = 32;

            immediateList.ReserveGeometry(c_ringSegments, c_ringSegments * 2);

            var indices = immediateList.Indices + immediateList.IndexCount;
            var vertices = immediateList.Vertices + immediateList.VertexCount;

            for (uint i = 0; i < c_ringSegments; i++)
            {
                indices[i * 2] = i;
                indices[i * 2 + 1] = i + 1;
            }

            indices[(c_ringSegments - 1) * 2 + 1] = 0;

            Vector3 majorAxis = ellipse.majorAxis;
            Vector3 minorAxis = ellipse.minorAxis;
            float fAngleDelta = MathUtil.PI2 / c_ringSegments;

            var mat = MathUtil.BillboardLH(origin, camPos, camUp, camForward);

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
                vertices[i].Position = Vector3.Transform(pos, mat);
                vertices[i].UV = WhiteUV;
                vertices[i].Color = color;
                // Standard formula to rotate a vector.
                Vector3 newCos = incrementalCos * cosDelta - incrementalSin * sinDelta;
                Vector3 newSin = incrementalCos * sinDelta + incrementalSin * cosDelta;
                incrementalCos = newCos;
                incrementalSin = newSin;
            }

            immediateList.RecordCmd(PrimitiveTopology.LineList);
        }

        private static readonly Vector3[] boxPositions =
[
new Vector3(-1, +1, -1),
new Vector3(-1, -1, -1),
new Vector3(+1, -1, -1),
new Vector3(+1, +1, -1),
new Vector3(-1, +1, +1),
new Vector3(-1, -1, +1),
new Vector3(+1, -1, +1),
new Vector3(+1, +1, +1),
            ];

        private static readonly uint[] boxIndices = [0, 1, 1, 2, 2, 3, 3, 0, 0, 4, 1, 5, 2, 6, 3, 7, 4, 5, 5, 6, 6, 7, 7, 4];

        /// <summary>
        /// Draws a 3D box at a specified position with the given orientation and dimensions.
        /// </summary>
        /// <param name="origin">The center point of the box.</param>
        /// <param name="orientation">The orientation (rotation) of the box.</param>
        /// <param name="width">The width of the box.</param>
        /// <param name="height">The height of the box.</param>
        /// <param name="depth">The depth of the box.</param>
        /// <param name="col">The color of the box.</param>
        ///
        public static void DrawBox(Vector3 origin, Quaternion orientation, float width, float height, float depth, Vector4 col)
        {
            DrawPreComputed(PrimitiveTopology.LineList, boxPositions, boxIndices, Matrix4x4.CreateScale(width, height, depth) * Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(origin), col);
        }

        /// <summary>
        /// Draws a 3D sphere at a specified position with the given orientation and radius.
        /// </summary>
        /// <param name="origin">The center point of the sphere.</param>
        /// <param name="orientation">The orientation (rotation) of the sphere.</param>
        /// <param name="radius">The radius of the sphere.</param>
        /// <param name="col">The color of the sphere.</param>
        ///
        public static void DrawSphere(Vector3 origin, Quaternion orientation, float radius, Vector4 col)
        {
            DrawPreComputed(PrimitiveTopology.LineList, spherePositions, sphereIndices, Matrix4x4.CreateScale(radius) * Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(origin), col);
        }

        #region Capsule Data

        private static readonly Vector3[] capsulePositions =
        [
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
        ];

        private static readonly uint[] capsuleIndices =
                [
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
            ];

        #endregion Capsule Data

        /// <summary>
        /// Draws a 3D capsule at a specified position with the given orientation, radius, and length.
        /// </summary>
        /// <param name="origin">The center point of the capsule.</param>
        /// <param name="orientation">The orientation (rotation) of the capsule.</param>
        /// <param name="radius">The radius of the capsule.</param>
        /// <param name="length">The length of the capsule (excluding the two hemispheres).</param>
        /// <param name="col">The color of the capsule.</param>
        ///
        public static void DrawCapsule(Vector3 origin, Quaternion orientation, float radius, float length, Vector4 col)
        {
            DrawPreComputed(PrimitiveTopology.LineList, capsulePositions, capsuleIndices, Matrix4x4.CreateScale(radius, length, radius) * Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(origin), col);
        }

        #region Cylinder Data

        private static readonly Vector3[] cylinderPositions =
        [
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
        ];

        private static readonly uint[] cylinderIndices =
        [
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
            ];

        public static Vector2 WhiteUV { get; private set; } = Vector2.One / 2;

        #endregion Cylinder Data

        /// <summary>
        /// Draws a 3D cylinder at a specified position with the given orientation, radius, and length.
        /// </summary>
        /// <param name="origin">The center point of the cylinder.</param>
        /// <param name="orientation">The orientation (rotation) of the cylinder.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="length">The length of the cylinder.</param>
        /// <param name="col">The color of the cylinder.</param>
        ///
        public static void DrawCylinder(Vector3 origin, Quaternion orientation, float radius, float length, Vector4 col)
        {
            DrawPreComputed(PrimitiveTopology.LineList, cylinderPositions, cylinderIndices, Matrix4x4.CreateScale(radius, length, radius) * Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(origin), col);
        }

        /// <summary>
        /// Draws a 3D triangle with the specified vertices, position, orientation, and color.
        /// </summary>
        /// <param name="origin">The position of the triangle's local origin.</param>
        /// <param name="orientation">The orientation (rotation) of the triangle.</param>
        /// <param name="a">The first vertex of the triangle.</param>
        /// <param name="b">The second vertex of the triangle.</param>
        /// <param name="c">The third vertex of the triangle.</param>
        /// <param name="col">The color of the triangle.</param>
        ///
        public static void DrawTriangle(Vector3 origin, Quaternion orientation, Vector3 a, Vector3 b, Vector3 c, Vector4 col)
        {
            immediateList.BeginDraw();

            uint color = ColorConvertFloat4ToU32(col);

            immediateList.ReserveGeometry(3, 6);
            var indices = immediateList.Indices + immediateList.IndexCount;
            var vertices = immediateList.Vertices + immediateList.VertexCount;

            indices[0] = 0; indices[1] = 1;
            indices[2] = 1; indices[3] = 2;
            indices[4] = 2; indices[5] = 0;

            vertices[0] = new(Vector3.Transform(a, orientation) + origin, default, color);
            vertices[1] = new(Vector3.Transform(b, orientation) + origin, default, color);
            vertices[2] = new(Vector3.Transform(c, orientation) + origin, default, color);

            immediateList.RecordCmd(PrimitiveTopology.TriangleList);
        }

        /// <summary>
        /// Draws a textured quad.
        /// </summary>
        /// <param name="origin">The origin of the quad.</param>
        /// <param name="orientation">The orientation of the quad.</param>
        /// <param name="scale">The scale of the quad.</param>
        /// <param name="uv0">The UV coordinates of the bottom-left corner of the quad.</param>
        /// <param name="uv1">The UV coordinates of the top-right corner of the quad.</param>
        /// <param name="col">The color of the quad.</param>
        /// <param name="texId">The texture ID of the quad.</param>
        ///
        public static void DrawQuad(Vector3 origin, Quaternion orientation, Vector2 scale, Vector2 uv0, Vector2 uv1, Vector4 col, nint texId)
        {
            immediateList.BeginDraw();

            uint color = ColorConvertFloat4ToU32(col);

            immediateList.ReserveGeometry(4, 6);
            var indices = immediateList.Indices + immediateList.IndexCount;
            var vertices = immediateList.Vertices + immediateList.VertexCount;

            indices[0] = 0; indices[1] = 1; indices[2] = 2;
            indices[3] = 1; indices[4] = 2; indices[5] = 3;

            Vector3 p0 = new(-scale.X, -scale.Y, 0); // bottom left corner
            Vector3 p1 = new(-scale.X, scale.Y, 0); // top left corner
            Vector3 p2 = new(scale.X, scale.Y, 0); // top right corner
            Vector3 p3 = new(scale.X, -scale.Y, 0); // bottom right corner

            vertices[0] = new(Vector3.Transform(p0, orientation) + origin, new(uv0.X, uv1.Y), color);
            vertices[1] = new(Vector3.Transform(p1, orientation) + origin, uv0, color);
            vertices[2] = new(Vector3.Transform(p2, orientation) + origin, new(uv1.X, uv0.Y), color);
            vertices[3] = new(Vector3.Transform(p3, orientation) + origin, uv1, color);

            immediateList.RecordCmd(PrimitiveTopology.TriangleList, texId);
        }

        /// <summary>
        /// Draws a textured quad billboard in 3D space.
        /// </summary>
        /// <param name="origin">The center point of the billboard in world space.</param>
        /// <param name="camOrigin">The origin of the camera in world space.</param>
        /// <param name="camUp">The up vector of the camera in world space.</param>
        /// <param name="camForward">The forward vector of the camera in world space.</param>
        /// <param name="scale">The scale of the billboard along the X and Y axes.</param>
        /// <param name="uv0">The UV coordinate for the bottom-left corner of the texture.</param>
        /// <param name="uv1">The UV coordinate for the top-right corner of the texture.</param>
        /// <param name="col">The color of the billboard.</param>
        /// <param name="texId">The identifier of the texture to be applied to the billboard.</param>
        ///
        public static void DrawQuadBillboard(Vector3 origin, Vector3 camOrigin, Vector3 camUp, Vector3 camForward, Vector2 scale, Vector2 uv0, Vector2 uv1, Vector4 col, nint texId)
        {
            immediateList.BeginDraw();

            uint color = ColorConvertFloat4ToU32(col);

            immediateList.ReserveGeometry(4, 6);
            var indices = immediateList.Indices + immediateList.IndexCount;
            var vertices = immediateList.Vertices + immediateList.VertexCount;

            indices[0] = 0; indices[1] = 1; indices[2] = 2;
            indices[3] = 0; indices[4] = 2; indices[5] = 3;

            Matrix4x4 mat = MathUtil.BillboardLH(origin, camOrigin, camUp, camForward);

            Vector3 p0 = new(-scale.X, -scale.Y, 0); // bottom left corner
            Vector3 p1 = new(-scale.X, scale.Y, 0); // top left corner
            Vector3 p2 = new(scale.X, scale.Y, 0); // top right corner
            Vector3 p3 = new(scale.X, -scale.Y, 0); // bottom right corner

            vertices[0] = new(Vector3.Transform(p0, mat), new(uv0.X, uv1.Y), color);
            vertices[1] = new(Vector3.Transform(p1, mat), uv0, color);
            vertices[2] = new(Vector3.Transform(p2, mat), new(uv1.X, uv0.Y), color);
            vertices[3] = new(Vector3.Transform(p3, mat), uv1, color);

            immediateList.RecordCmd(PrimitiveTopology.TriangleList, texId);
        }

        /// <summary>
        /// Draws a 2D grid in 3D space defined by the provided transformation matrix.
        /// </summary>
        /// <param name="matrix">The transformation matrix defining the orientation and position of the grid.</param>
        /// <param name="size">The size of the grid (half-extent in each dimension).</param>
        /// <param name="col">The color of the grid lines.</param>
        ///
        public static void DrawGrid(Matrix4x4 matrix, int size, Vector4 col)
        {
            immediateList.BeginDraw();

            uint vertexCount = 2u * (uint)size * 2u + 4;
            uint color = ColorConvertFloat4ToU32(col);

            immediateList.ReserveGeometry(vertexCount, vertexCount);
            var indices = immediateList.Indices + immediateList.IndexCount;
            var vertices = immediateList.Vertices + immediateList.VertexCount;

            int half = size / 2;

            uint i = 0;
            for (int x = -half; x <= half; x++)
            {
                var pos0 = Vector3.Transform(new Vector3(x, 0, -half), matrix);
                var pos1 = Vector3.Transform(new Vector3(x, 0, half), matrix);
                vertices[i] = new(pos0, default, color);
                vertices[i + 1] = new(pos1, default, color);
                indices[i] = i;
                indices[i + 1] = i + 1;
                i += 2;
            }

            for (int z = -half; z <= half; z++)
            {
                var pos0 = Vector3.Transform(new Vector3(-half, 0, z), matrix);
                var pos1 = Vector3.Transform(new Vector3(half, 0, z), matrix);
                vertices[i] = new(pos0, default, color);
                vertices[i + 1] = new(pos1, default, color);
                indices[i] = i;
                indices[i + 1] = i + 1;
                i += 2;
            }

            immediateList.RecordCmd(PrimitiveTopology.LineList);
        }
    }
}