#nullable disable

namespace HexaEngine.Editor
{
    using HexaEngine.Cameras;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    public static unsafe class DebugDraw
    {
        private static IGraphicsDevice device;
        private static IGraphicsContext context;
        private static IRasterizerState rs;
        private static IDepthStencilState ds;
        private static IVertexShader vs;
        private static IPixelShader ps;
        private static IInputLayout il;
        private static IBuffer vb;
        private static IBuffer ib;
        private static IBuffer cb;
        private static int vbCapacity = 1000;
        private static int ibCapacity = 1000;

        private static readonly List<VertexPositionColor> lineVertices = new();
        private static readonly List<int> lineIndices = new();
        private static readonly List<ValueTuple<int, int, PrimitiveTopology>> drawcmds = new();

        private struct CBView
        {
            public Matrix4x4 View;
            public Matrix4x4 Proj;
        }

        public static void Init(IGraphicsDevice device)
        {
            DebugDraw.device = device;
            context = device.Context;
            var desc = RasterizerDescription.CullNone;
            desc.AntialiasedLineEnable = true;
            desc.MultisampleEnable = false;
            rs = device.CreateRasterizerState(desc);
            ds = device.CreateDepthStencilState(DepthStencilDescription.None);
            string vsCode = @"
struct VertexInputType
{
	float3 position : POSITION;
    float4 color : COLOR;
};
struct GeometryInput
{
	float4 position : SV_POSITION;
    float4 color : COLOR;
};
cbuffer MVPBuffer
{
    matrix view;
    matrix proj;
};
GeometryInput main(VertexInputType input)
{
	GeometryInput output;
    output.position = mul(float4(input.position, 1), view);
    output.position = mul(output.position, proj);
    output.color = input.color;
	return output;
}
";

            string psCode = @"
struct PixelInputType
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
};
float4 main(PixelInputType pixel) : SV_TARGET
{
    return pixel.color;
}
";
            if (!ShaderCache.GetShader("DEBUGDRAW-INTERNAL-VS", Array.Empty<ShaderMacro>(), out var vsBytes))
            {
                device.Compile(vsCode, "main", "VS", "vs_5_0", out var vsBlob);
                vsBytes = vsBlob.AsSpan();

                ShaderCache.CacheShader("DEBUGDRAW-INTERNAL-VS", Array.Empty<ShaderMacro>(), vsBlob);
            }

            vs = device.CreateVertexShader(vsBytes);
            il = device.CreateInputLayout(vsBytes);

            if (!ShaderCache.GetShader("DEBUGDRAW-INTERNAL-PS", Array.Empty<ShaderMacro>(), out var psBytes))
            {
                device.Compile(psCode, "main", "PS", "ps_5_0", out var psBlob);
                psBytes = psBlob.AsSpan();

                ShaderCache.CacheShader("DEBUGDRAW-INTERNAL-PS", Array.Empty<ShaderMacro>(), psBlob);
            }

            ps = device.CreatePixelShader(psBytes);

            vb = device.CreateBuffer(new BufferDescription(vbCapacity * sizeof(VertexPositionColor), BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            ib = device.CreateBuffer(new BufferDescription(ibCapacity * sizeof(int), BindFlags.IndexBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            cb = device.CreateBuffer(new BufferDescription(sizeof(CBView), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write));
        }

        public static void Dispose()
        {
            rs.Dispose();
            ds.Dispose();
            vs.Dispose();
            ps.Dispose();
            il.Dispose();
            vb.Dispose();
            ib.Dispose();
            cb.Dispose();
        }

        public static void Render(Camera camera, Viewport viewport)
        {
            if (camera == null) return;
            if (lineVertices.Count > vbCapacity)
            {
                vb.Dispose();
                vbCapacity = (int)(lineVertices.Count * 1.5f);
                vb = device.CreateBuffer(lineVertices.ToArray(), new BufferDescription(vbCapacity * sizeof(VertexPositionColor), BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            }
            else
            {
                context.Write(vb, lineVertices.ToArray());
            }
            if (lineIndices.Count > ibCapacity)
            {
                ib.Dispose();
                ibCapacity = (int)(lineIndices.Count * 1.5f);
                ib = device.CreateBuffer(lineIndices.ToArray(), new BufferDescription(ibCapacity * sizeof(int), BindFlags.IndexBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            }
            else
            {
                context.Write(ib, lineIndices.ToArray());
            }
            context.Write(cb, new CBView()
            {
                View = Matrix4x4.Transpose(camera.Transform.View),
                Proj = Matrix4x4.Transpose(camera.Transform.Projection)
            });

            int voffset = 0;
            int ioffset = 0;
            context.SetRasterizerState(rs);
            context.SetDepthStencilState(ds);
            context.VSSetShader(vs);
            context.PSSetShader(ps);
            context.SetInputLayout(il);
            context.SetConstantBuffer(cb, ShaderStage.Vertex, 0);
            context.SetVertexBuffer(vb, sizeof(VertexPositionColor));
            context.SetIndexBuffer(ib, Format.R32UInt, 0);
            context.SetViewport(viewport);
            for (int i = 0; i < drawcmds.Count; i++)
            {
                var cmd = drawcmds[i];
                context.SetPrimitiveTopology(cmd.Item3);
                context.DrawIndexed(cmd.Item2, ioffset, voffset);
                voffset += cmd.Item1;
                ioffset += cmd.Item2;
            }
            lineVertices.Clear();
            lineIndices.Clear();
            drawcmds.Clear();
        }

        private static void BatchDraw(VertexPositionColor[] vertices, PrimitiveTopology topology)
        {
            drawcmds.Add((vertices.Length, vertices.Length, topology));
            lineVertices.AddRange(vertices);
            int[] indices = new int[vertices.Length];
            for (int i = 0; i < indices.Length; i++)
                indices[i] = i;
            lineIndices.AddRange(indices);
        }

        private static void BatchDraw(VertexPositionColor[] vertices, int[] indices, PrimitiveTopology topology)
        {
            drawcmds.Add((vertices.Length, indices.Length, topology));
            lineVertices.AddRange(vertices);
            lineIndices.AddRange(indices);
        }

        public static void DrawFrustum(BoundingFrustum frustum, Vector4 color)
        {
            var corners = frustum.GetCorners();
            VertexPositionColor[] verts = new VertexPositionColor[corners.Length];

            int[] indices =
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
            };

            for (int i = 0; i < corners.Length; i++)
            {
                verts[i].Color = color;
                verts[i].Position = corners[i];
            }
            BatchDraw(verts, indices, PrimitiveTopology.LineList);
        }

        public static void DrawRay(Vector3 origin, Vector3 direction, bool normalize, Vector4 color)
        {
            VertexPositionColor[] verts = new VertexPositionColor[3];
            verts[0].Position = origin;

            Vector3 normDirection = Vector3.Normalize(direction);
            Vector3 rayDirection = normalize ? normDirection : direction;

            Vector3 perpVector = Vector3.Cross(normDirection, Vector3.UnitY);

            if (perpVector.LengthSquared() == 0f)
            {
                perpVector = Vector3.Cross(normDirection, Vector3.UnitZ);
            }
            perpVector = Vector3.Normalize(perpVector);

            verts[1].Position = rayDirection + origin;
            perpVector *= 0.0625f;
            normDirection *= -0.25f;
            rayDirection = perpVector + rayDirection;
            rayDirection = normDirection + rayDirection;
            verts[2].Position = rayDirection + origin;

            verts[0].Color = color;
            verts[1].Color = color;
            verts[2].Color = color;

            BatchDraw(verts, PrimitiveTopology.LineStrip);
        }

        public static void DrawLine(Vector3 origin, Vector3 direction, bool normalize, Vector4 color)
        {
            VertexPositionColor[] verts = new VertexPositionColor[2];
            verts[0].Position = origin;

            Vector3 normDirection = Vector3.Normalize(direction);
            Vector3 rayDirection = normalize ? normDirection : direction;

            verts[1].Position = rayDirection + origin;

            verts[0].Color = color;
            verts[1].Color = color;

            BatchDraw(verts, PrimitiveTopology.LineStrip);
        }

        public static void DrawRing(Vector3 origin, Quaternion orientation, Vector3 majorAxis, Vector3 minorAxis, Vector4 color)
        {
            const int c_ringSegments = 32;

            VertexPositionColor[] verts = new VertexPositionColor[c_ringSegments + 1];

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
                verts[i].Position = Vector3.Transform(pos, orientation) + origin;
                verts[i].Color = color;
                // Standard formula to rotate a vector.
                Vector3 newCos = incrementalCos * cosDelta - incrementalSin * sinDelta;
                Vector3 newSin = incrementalCos * sinDelta + incrementalSin * cosDelta;
                incrementalCos = newCos;
                incrementalSin = newSin;
            }
            verts[c_ringSegments] = verts[0];
            BatchDraw(verts, PrimitiveTopology.LineStrip);
        }

        public static void DrawRing(Vector3 origin, Vector3 majorAxis, Vector3 minorAxis, Vector4 color)
        {
            const int c_ringSegments = 32;

            VertexPositionColor[] verts = new VertexPositionColor[c_ringSegments + 1];

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
                verts[i].Position = pos;
                verts[i].Color = color;
                // Standard formula to rotate a vector.
                Vector3 newCos = incrementalCos * cosDelta - incrementalSin * sinDelta;
                Vector3 newSin = incrementalCos * sinDelta + incrementalSin * cosDelta;
                incrementalCos = newCos;
                incrementalSin = newSin;
            }
            verts[c_ringSegments] = verts[0];
            BatchDraw(verts, PrimitiveTopology.LineStrip);
        }

        public static void DrawRing(Vector3 origin, (Vector3, Vector3) ellipse, Vector4 color)
        {
            const int c_ringSegments = 32;

            VertexPositionColor[] verts = new VertexPositionColor[c_ringSegments + 1];
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
                verts[i].Position = pos;
                verts[i].Color = color;
                // Standard formula to rotate a vector.
                Vector3 newCos = incrementalCos * cosDelta - incrementalSin * sinDelta;
                Vector3 newSin = incrementalCos * sinDelta + incrementalSin * cosDelta;
                incrementalCos = newCos;
                incrementalSin = newSin;
            }
            verts[c_ringSegments] = verts[0];
            BatchDraw(verts, PrimitiveTopology.LineStrip);
        }

        public static void DrawBox(Vector3 origin, Quaternion orientation, float width, float height, float depth, Vector4 color)
        {
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

            int[] indices =
            {
                0,1,1,2,2,3,3,0,
                0,4,1,5,2,6,3,7,
                4,5,5,6,6,7,7,4
            };

            Matrix4x4 transform = Matrix4x4.CreateScale(width, height, depth) * Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(origin);
            VertexPositionColor[] verts = new VertexPositionColor[pos.Length];

            for (int i = 0; i < pos.Length; i++)
            {
                verts[i].Color = color;
                verts[i].Position = Vector3.Transform(pos[i], transform);
            }
            BatchDraw(verts, indices, PrimitiveTopology.LineList);
        }

        public static void DrawSphere(Vector3 origin, Quaternion orientation, float radius, Vector4 color)
        {
            Vector3[] pos =
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

            int[] indices =
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
            };
            Matrix4x4 transform = Matrix4x4.CreateScale(radius) * Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(origin);
            VertexPositionColor[] verts = new VertexPositionColor[pos.Length];

            for (int i = 0; i < pos.Length; i++)
            {
                verts[i].Color = color;
                verts[i].Position = Vector3.Transform(pos[i], transform);
            }
            BatchDraw(verts, indices, PrimitiveTopology.LineList);
        }

        public static void DrawCapsule(Vector3 origin, Quaternion orientation, float radius, float length, Vector4 color)
        {
            Vector3[] pos =
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

            int[] indices =
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
            };

            Matrix4x4 transform = Matrix4x4.CreateScale(radius, length, radius) * Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(origin);
            VertexPositionColor[] verts = new VertexPositionColor[pos.Length];

            for (int i = 0; i < pos.Length; i++)
            {
                verts[i].Color = color;
                verts[i].Position = Vector3.Transform(pos[i], transform);
            }
            BatchDraw(verts, indices, PrimitiveTopology.LineList);
        }

        public static void DrawCylinder(Vector3 origin, Quaternion orientation, float radius, float length, Vector4 color)
        {
            Vector3[] pos =
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

            int[] indices =
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
            };

            Matrix4x4 transform = Matrix4x4.CreateScale(radius, length, radius) * Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(origin);
            VertexPositionColor[] verts = new VertexPositionColor[pos.Length];

            for (int i = 0; i < pos.Length; i++)
            {
                verts[i].Color = color;
                verts[i].Position = Vector3.Transform(pos[i], transform);
            }
            BatchDraw(verts, indices, PrimitiveTopology.LineList);
        }

        public static void DrawTriangle(Vector3 origin, Quaternion orientation, Vector3 a, Vector3 b, Vector3 c, Vector4 color)
        {
            VertexPositionColor[] verts = new VertexPositionColor[4];
            verts[0] = new(Vector3.Transform(a, orientation) + origin, color);
            verts[1] = new(Vector3.Transform(b, orientation) + origin, color);
            verts[2] = new(Vector3.Transform(c, orientation) + origin, color);
            verts[3] = verts[0];
            BatchDraw(verts, PrimitiveTopology.LineStrip);
        }
    }
}