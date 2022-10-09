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
        private static Blob vsBlob;
        private static IVertexShader vs;
        private static Blob psBlob;
        private static IPixelShader ps;
        private static IInputLayout il;
        private static IBuffer vb;
        private static IBuffer cb;
        private static int vbCapacity = 1000;

        private static readonly List<VertexPositionColor> lineVertices = new();
        private static readonly List<ValueTuple<int, PrimitiveTopology>> drawcmds = new();

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
            device.Compile(vsCode, "main", "VS", "vs_4_0", out vsBlob);
            vs = device.CreateVertexShader(vsBlob);
            il = device.CreateInputLayout(vsBlob);
            device.Compile(psCode, "main", "PS", "ps_4_0", out psBlob);
            ps = device.CreatePixelShader(psBlob);

            vb = device.CreateBuffer(new BufferDescription(vbCapacity * sizeof(VertexPositionColor), BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            cb = device.CreateBuffer(new BufferDescription(sizeof(CBView), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write));
        }

        public static void Dispose()
        {
            rs.Dispose();
            ds.Dispose();
            vsBlob.Dispose();
            vs.Dispose();
            psBlob.Dispose();
            ps.Dispose();
            il.Dispose();
            vb.Dispose();
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
            context.Write(cb, new CBView()
            {
                View = Matrix4x4.Transpose(camera.Transform.View),
                Proj = Matrix4x4.Transpose(camera.Transform.Projection)
            });

            int offset = 0;
            context.SetRasterizerState(rs);
            context.SetDepthStencilState(ds);
            context.VSSetShader(vs);
            context.PSSetShader(ps);
            context.SetInputLayout(il);
            context.SetConstantBuffer(cb, ShaderStage.Vertex, 0);
            context.SetVertexBuffer(vb, sizeof(VertexPositionColor));
            context.SetViewport(viewport);
            for (int i = 0; i < drawcmds.Count; i++)
            {
                var cmd = drawcmds[i];
                context.SetPrimitiveTopology(cmd.Item2);
                context.Draw(cmd.Item1, offset);
                offset += cmd.Item1;
            }
            lineVertices.Clear();
            drawcmds.Clear();
        }

        private static void BatchDraw(VertexPositionColor[] vertices, PrimitiveTopology topology)
        {
            drawcmds.Add((vertices.Length, topology));
            lineVertices.AddRange(vertices);
        }

        public static void DrawFrustum(BoundingFrustum frustum, Vector4 color)
        {
            var corners = frustum.GetCorners();
            VertexPositionColor[] verts = new VertexPositionColor[24];
            verts[0].Position = corners[0];
            verts[1].Position = corners[1];
            verts[2].Position = corners[1];
            verts[3].Position = corners[2];
            verts[4].Position = corners[2];
            verts[5].Position = corners[3];
            verts[6].Position = corners[3];
            verts[7].Position = corners[0];

            verts[8].Position = corners[0];
            verts[9].Position = corners[4];
            verts[10].Position = corners[1];
            verts[11].Position = corners[5];
            verts[12].Position = corners[2];
            verts[13].Position = corners[6];
            verts[14].Position = corners[3];
            verts[15].Position = corners[7];

            verts[16].Position = corners[4];
            verts[17].Position = corners[5];
            verts[18].Position = corners[5];
            verts[19].Position = corners[6];
            verts[20].Position = corners[6];
            verts[21].Position = corners[7];
            verts[22].Position = corners[7];
            verts[23].Position = corners[4];

            for (int i = 0; i < 24; i++)
            {
                verts[i].Color = color;
            }

            BatchDraw(verts, PrimitiveTopology.LineList);
        }

        public static void DrawGrid(Vector3 xAxis, Vector3 yAxis, Vector3 origin, int xdivs, int ydivs, Vector4 color)
        {
            xdivs = Math.Max(1, xdivs);
            ydivs = Math.Max(1, ydivs);

            VertexPositionColor[] vertices = new VertexPositionColor[(xdivs + 1) * (ydivs + 1) * 2];
            int ig = 0;
            for (int i = 0; i <= xdivs; ++i)
            {
                float percent = i / (float)xdivs;
                percent = percent * 2.0f - 1.0f;
                Vector3 scale = xAxis * percent;
                scale += origin;

                VertexPositionColor v1 = new(scale - yAxis, color);
                VertexPositionColor v2 = new(scale + yAxis, color);
                vertices[ig++] = v1;
                vertices[ig++] = v2;
            }

            for (int i = 0; i <= ydivs; i++)
            {
                float percent = i / (float)ydivs;
                percent = percent * 2.0f - 1.0f;
                Vector3 scale = yAxis * percent;
                scale += origin;

                VertexPositionColor v1 = new(scale - xAxis, color);
                VertexPositionColor v2 = new(scale + xAxis, color);
                vertices[ig++] = v1;
                vertices[ig++] = v2;
            }

            BatchDraw(vertices, PrimitiveTopology.LineList);
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

        public static int GenerateRing(Span<VertexPositionColor> verts, Vector3 majorAxis, Vector3 minorAxis, Vector4 color)
        {
            const int c_ringSegments = 32;

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
                verts[i].Position = pos;
                verts[i].Color = color;
                // Standard formula to rotate a vector.
                Vector3 newCos = incrementalCos * cosDelta - incrementalSin * sinDelta;
                Vector3 newSin = incrementalCos * sinDelta + incrementalSin * cosDelta;
                incrementalCos = newCos;
                incrementalSin = newSin;
            }
            verts[c_ringSegments] = verts[0];
            return c_ringSegments + 1;
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
            float cx = origin.X;
            float cy = origin.Y;
            float cz = origin.Z;
            float w = width * 1f;
            float h = height * 1f;
            float d = depth * 1f;

            Vector3[] pos = new Vector3[8];
            pos[0] = new(cx - w, cy + h, cz - d);
            pos[1] = new(cx - w, cy - h, cz - d);
            pos[2] = new(cx + w, cy - h, cz - d);
            pos[3] = new(cx + w, cy + h, cz - d);

            pos[4] = new(cx - w, cy + h, cz + d);
            pos[5] = new(cx - w, cy - h, cz + d);
            pos[6] = new(cx + w, cy - h, cz + d);
            pos[7] = new(cx + w, cy + h, cz + d);

            int[] indices = new int[]
            {
                0,1,1,2,2,3,3,0,
                0,4,1,5,2,6,3,7,
                4,5,5,6,6,7,7,4
            };

            VertexPositionColor[] verts = new VertexPositionColor[24];

            for (int i = 0; i < 24; i++)
            {
                verts[i].Color = color;
                verts[i].Position = Vector3.Transform(pos[indices[i]] - origin, orientation) + origin;
            }
            BatchDraw(verts, PrimitiveTopology.LineList);
        }

        public static void DrawSphere(Vector3 origin, Quaternion orientation, float radius, Vector4 color)
        {
            DrawRing(origin, orientation, Vector3.UnitX * radius, Vector3.UnitY * radius, color);
            DrawRing(origin, orientation, Vector3.UnitY * radius, Vector3.UnitZ * radius, color);
            DrawRing(origin, orientation, Vector3.UnitZ * radius, Vector3.UnitX * radius, color);
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
32,31,
1,33,
34,33,
36,35,
38,37,
40,39,
9,41,
42,41,
44,43,
46,45,
48,47,
17,49,
50,49,
52,51,
54,53,
56,55,
25,57,
58,57,
60,59,
62,61,
64,63,
65,17,
66,65,
68,67,
70,69,
72,71,
74,73,
76,75,
78,77,
1,79,
82,81,
84,83,
86,85,
9,87,
88,25,
90,89,
92,91,
94,93,
95,33,
96,95,
98,97,
100,99,
102,101,
104,103,
106,105,
108,107,
49,109,
112,111,
114,113,
116,115,
41,117,
118,57,
120,119,
122,121,
124,123,
1,32,
33,64,
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
35,34,
37,36,
39,38,
41,40,
43,42,
45,44,
47,46,
49,48,
51,50,
53,52,
55,54,
57,56,
59,58,
61,60,
63,62,
67,66,
69,68,
71,70,
73,72,
75,74,
77,76,
79,78,
81,80,
83,82,
85,84,
87,86,
89,88,
91,90,
93,92,
80,94,
97,96,
99,98,
101,100,
103,102,
105,104,
107,106,
109,108,
111,110,
113,112,
115,114,
117,116,
119,118,
121,120,
123,122,
110,124
            };

            Matrix4x4 transform = Matrix4x4.CreateScale(radius, length, radius) * Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(origin);
            VertexPositionColor[] verts = new VertexPositionColor[indices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                verts[i].Color = color;
                verts[i].Position = Vector3.Transform(pos[indices[i] - 1], transform);
            }
            BatchDraw(verts, PrimitiveTopology.LineList);
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
32,31,
1,33,
34,33,
35,34,
36,35,
37,36,
38,37,
39,38,
40,39,
9,41,
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
64,63,
41,40,
17,49,
1,32,
33,64,
25,57,
            };

            Matrix4x4 transform = Matrix4x4.CreateScale(radius, length, radius) * Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(origin);
            VertexPositionColor[] verts = new VertexPositionColor[indices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                verts[i].Color = color;
                verts[i].Position = Vector3.Transform(pos[indices[i] - 1], transform);
            }
            BatchDraw(verts, PrimitiveTopology.LineList);
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