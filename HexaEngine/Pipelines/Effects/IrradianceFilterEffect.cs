namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Objects.Primitives;
    using HexaEngine.Resources.Buffers;
    using System;
    using System.Numerics;

    public class IrradianceFilterEffect : Effect
    {
        private readonly IBuffer mvpBuffer;
        private readonly ISamplerState sampler;
        public IShaderResourceView Source;

        public struct CubeFaceCamera
        {
            public Matrix4x4 View;
            public Matrix4x4 Projection;
        }

#nullable disable
        private CubeFaceCamera[] Cameras;
#nullable enable

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

        public IrradianceFilterEffect(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "effects/irradiance/vs.hlsl",
            PixelShader = "effects/irradiance/ps.hlsl"
        })
        {
            AutoSetTarget = false;
            SetViewPoint(Vector3.Zero);
            mvpBuffer = device.CreateBuffer(new ModelViewProj(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write); //CreateConstantBuffer<ModelViewProj>(ShaderStage.Vertex, 0);
            sampler = device.CreateSamplerState(SamplerDescription.AnisotropicWrap);
            Mesh = new Cube(device);
            State = new()
            {
                DepthStencil = DepthStencilDescription.None,
                Rasterizer = new RasterizerDescription(CullMode.None, FillMode.Solid) { ScissorEnable = true },
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };
        }

        public override void Draw(IGraphicsContext context)
        {
            if (Targets == null) return;
            int width = (int)Targets.Viewport.Width;
            int height = (int)Targets.Viewport.Height;
            int xTileSize = width / 8;
            int yTileSize = height / 8;

            for (int i = 0; i < 6; i++)
            {
                Targets.ClearTarget(context, i);

                for (int x = 0; x < width; x += xTileSize)
                {
                    for (int y = 0; y < height; y += yTileSize)
                    {
                        context.Write(mvpBuffer, new ModelViewProj(Matrix4x4.Identity, Cameras[i].View, Cameras[i].Projection));
                        context.SetScissorRect(x, y, x + xTileSize, y + yTileSize);
                        context.VSSetConstantBuffer(mvpBuffer, 0);
                        context.PSSetSampler(sampler, 0);
                        context.PSSetShaderResource(Source, 0);
                        Targets.SetTarget(context, i);
                        base.DrawAuto(context, Targets.Viewport);
                    }
                    context.Flush();
                }
            }
        }

        public void DrawSlice(IGraphicsContext context, int i, int x, int y, int xSize, int ySize)
        {
            if (Targets == null) return;

            context.Write(mvpBuffer, new ModelViewProj(Matrix4x4.Identity, Cameras[i].View, Cameras[i].Projection));
            context.SetScissorRect(x, y, x + xSize, y + ySize);
            context.VSSetConstantBuffer(mvpBuffer, 0);
            context.PSSetSampler(sampler, 0);
            context.PSSetShaderResource(Source, 0);
            Targets.SetTarget(context, i);
            base.DrawAuto(context, Targets.Viewport);
        }

        public override void DrawSettings()
        {
            throw new NotImplementedException();
        }
    }
}