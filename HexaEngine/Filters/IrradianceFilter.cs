namespace HexaEngine.Filters
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Graphics.Structs;
    using Silk.NET.Core.Native;
    using System;
    using System.Numerics;

    public class IrradianceFilter : IFilter
    {
        private readonly Cube cube;
        private readonly IGraphicsPipeline pipeline;
        private readonly IBuffer mvpBuffer;
        private readonly ISamplerState sampler;

        public RenderTargetViewArray? Targets;
        public IShaderResourceView? Source;

        public struct CubeFaceCamera
        {
            public Matrix4x4 View;
            public Matrix4x4 Projection;
        }

        private CubeFaceCamera[] Cameras;
        private bool disposedValue;

        public IrradianceFilter(IGraphicsDevice device)
        {
            Cameras = new CubeFaceCamera[6];
            cube = new(device);
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "filter/irradiance/vs.hlsl",
                PixelShader = "filter/irradiance/ps.hlsl"
            },
            new GraphicsPipelineState()
            {
                DepthStencil = DepthStencilDescription.None,
                Rasterizer = new RasterizerDescription(CullMode.None, FillMode.Solid) { ScissorEnable = true },
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            });

            SetViewPoint(Vector3.Zero);
            mvpBuffer = device.CreateBuffer(new ModelViewProj(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write); //CreateConstantBuffer<ModelViewProj>(ShaderStage.Vertex, 0);
            sampler = device.CreateSamplerState(SamplerDescription.AnisotropicWrap);
        }

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

            for (int i = 0; i < 6; i++)
            {
                Cameras[i].View = Matrix4x4.CreateLookAt(camera, targets[i], upVectors[i]) * Matrix4x4.CreateScale(-1, 1, 1);
                Cameras[i].Projection = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI * 0.5f, 1.0f, 0.1f, 100.0f);
            }
        }

        public void Draw(IGraphicsContext context)
        {
            if (Targets == null)
            {
                return;
            }

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
                        context.VSSetConstantBuffer(0, mvpBuffer);
                        context.PSSetSampler(0, sampler);
                        context.PSSetShaderResource(0, Source);
                        Targets.SetTarget(context, i);
                        context.SetViewport(Targets.Viewport);
                        cube.DrawAuto(context, pipeline);
                    }
                    context.Flush();
                }
            }
        }

        public void DrawSlice(IGraphicsContext context, int i, int x, int y, int xSize, int ySize)
        {
            if (Targets == null)
            {
                return;
            }

            context.Write(mvpBuffer, new ModelViewProj(Matrix4x4.Identity, Cameras[i].View, Cameras[i].Projection));
            context.SetScissorRect(x, y, x + xSize, y + ySize);
            context.VSSetConstantBuffer(0, mvpBuffer);
            context.PSSetSampler(0, sampler);
            context.PSSetShaderResource(0, Source);
            Targets.SetTarget(context, i);
            context.SetViewport(Targets.Viewport);
            cube.DrawAuto(context, pipeline);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                cube.Dispose();
                pipeline.Dispose();
                mvpBuffer.Dispose();
                sampler.Dispose();
                disposedValue = true;
            }
        }

        ~IrradianceFilter()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}