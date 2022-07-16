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
            var desc = RasterizerDescription.CullBack;
            desc.AntialiasedLineEnable = true;
            desc.MultisampleEnable = false;
            rs = device.CreateRasterizerState(desc);
            ds = device.CreateDepthStencilState(DepthStencilDescription.Default);
            string vsCode = @"
struct VertexInputType
{
	float3 position : POSITION;
    float4 color : COLOR;
};
struct PixelInputType
{
	float4 position : SV_POSITION;
    float4 color : COLOR;
};
cbuffer MVPBuffer
{
    matrix view;
    matrix proj;
};
PixelInputType main(VertexInputType input)
{
	PixelInputType output;
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

        public static void Draw(BoundingFrustum frustum, Vector4 color)
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
    }
}