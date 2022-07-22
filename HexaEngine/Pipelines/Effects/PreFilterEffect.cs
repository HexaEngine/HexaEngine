namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Objects.Primitives;
    using HexaEngine.Resources.Buffers;
    using System;
    using System.Numerics;

    /// <summary>
    /// Pre filter for the env map used in the brfd pipeline.
    /// </summary>
    public class PreFilterEffect : Effect
    {
        private readonly IBuffer mvpBuffer;
        private readonly IBuffer rghbuffer;

        public struct CubeFaceCamera
        {
            public Matrix4x4 View;
            public Matrix4x4 Projection;
        }

        public struct RoughnessBuffer
        {
            public float Roughness;
            public Vector3 Padd;
        }

#nullable disable
        private CubeFaceCamera[] Cameras;
#nullable enable

        public float Roughness;

        public void SetViewPoint(Vector3 camera)
        {
            // The TextureCube Texture2D assumes the
            // following order of faces.

            // The LookAt targets for view matrices
            var targets = new[] {
                camera + Vector3.UnitX, // +X
                camera - Vector3.UnitX, // -X
                camera + Vector3.UnitY, // +Y
                camera - Vector3.UnitY, // -Y
                camera + Vector3.UnitZ, // +Z
                camera - Vector3.UnitZ  // -Z
            };

            var upVectors = new[] {
                Vector3.UnitY, // +X
                Vector3.UnitY, // -X
                -Vector3.UnitZ,// +Y
                Vector3.UnitZ,// -Y
                Vector3.UnitY, // +Z
                Vector3.UnitY, // -Z
            };

            Cameras = new CubeFaceCamera[6];

            for (int i = 0; i < 6; i++)
            {
                Cameras[i].View = Matrix4x4.CreateLookAt(camera, targets[i], upVectors[i]) * Matrix4x4.CreateScale(-1, 1, 1);
                Cameras[i].Projection = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI * 0.5f, 1.0f, 0.1f, 100.0f);
            }
        }

        public PreFilterEffect(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "effects/prefilter/vs.hlsl",
            PixelShader = "effects/prefilter/ps.hlsl"
        })
        {
            AutoSetTarget = false;
            SetViewPoint(Vector3.Zero);
            mvpBuffer = CreateConstantBuffer<ViewProj>(ShaderStage.Vertex, 0);
            rghbuffer = CreateConstantBuffer<RoughnessBuffer>(ShaderStage.Pixel, 0);
            Mesh = new Cube(device);
            State = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullNone,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };
        }

        public override void Draw(IGraphicsContext context)
        {
            if (Targets == null) return;
            context.Write(rghbuffer, new RoughnessBuffer() { Roughness = Roughness });
            for (int i = 0; i < 6; i++)
            {
                IQuery query = context.Device.CreateQuery();
                context.Write(mvpBuffer, new ViewProj(Cameras[i].View, Cameras[i].Projection));
                Targets.ClearAndSetTarget(context, i);
                base.DrawAuto(context, Targets.Viewport);
                context.QueryEnd(query);
                context.QueryGetData(query);
                query.Dispose();
            }
        }

        public override void DrawSettings()
        {
            throw new NotImplementedException();
        }
    }
}