namespace HexaEngine.Graphics.Filters
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using System;
    using System.Numerics;

    /// <summary>
    /// Pixel shader based approach of diffuse importance sampling for IBL
    /// </summary>
    public class IBLDiffuseIrradiance : IDisposable
    {
        private readonly Cube cube;
        private readonly IGraphicsPipeline pipeline;
        private readonly ConstantBuffer<Matrix4x4> viewBuffer;
        private readonly ISamplerState sampler;

        public RenderTargetViewArray? Targets;
        public IShaderResourceView? Source;

        public struct CubeFaceCamera
        {
            public Matrix4x4 ViewProjection;
        }

        private readonly CubeFaceCamera[] Cameras;
        private bool disposedValue;

        public IBLDiffuseIrradiance(IGraphicsDevice device)
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
                Rasterizer = RasterizerDescription.CullNone,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            });

            SetViewPoint(Vector3.Zero);
            viewBuffer = new(device, CpuAccessFlags.Write);
            sampler = device.CreateSamplerState(new(Filter.MinMagMipLinear, TextureAddressMode.Clamp));
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
                Cameras[i].ViewProjection = Matrix4x4.Transpose(Matrix4x4.CreateLookAt(camera, targets[i], upVectors[i]) * Matrix4x4.CreateScale(-1, 1, 1) * Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI * 0.5f, 1.0f, 0.1f, 100.0f));
            }
        }

        public void Draw(IGraphicsContext context, uint width, uint height)
        {
            if (Targets == null)
            {
                return;
            }

            context.SetViewport(new(width, height));
            context.PSSetSampler(0, sampler);
            context.PSSetShaderResource(0, Source);
            context.VSSetConstantBuffer(0, viewBuffer);
            context.SetGraphicsPipeline(pipeline);

            for (int i = 0; i < 6; i++)
            {
                Targets.SetTarget(context, i);
                viewBuffer.Update(context, Cameras[i].ViewProjection);
                cube.DrawAuto(context);
            }

            context.SetGraphicsPipeline(null);
            context.PSSetShaderResource(0, null);
            context.VSSetConstantBuffer(0, null);
            context.PSSetSampler(0, null);
            context.SetRenderTarget(null, null);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                cube.Dispose();
                pipeline.Dispose();
                viewBuffer.Dispose();
                sampler.Dispose();
                disposedValue = true;
            }
        }

        ~IBLDiffuseIrradiance()
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