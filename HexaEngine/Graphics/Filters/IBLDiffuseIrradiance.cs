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
        private readonly IGraphicsPipelineState pipeline;
        private readonly ConstantBuffer<Matrix4x4> viewBuffer;
        private readonly ISamplerState sampler;

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
            cube = new(new());
            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "filter/irradiance/vs.hlsl",
                PixelShader = "filter/irradiance/ps.hlsl",
            }, new()
            {
                DepthStencil = DepthStencilDescription.None,
                Rasterizer = RasterizerDescription.CullNone,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            });

            SetViewPoint(Vector3.Zero);
            viewBuffer = new(CpuAccessFlags.Write);
            sampler = device.CreateSamplerState(new(Filter.MinMagMipLinear, TextureAddressMode.Clamp));
            pipeline.Bindings.SetCBV("mvp", viewBuffer);
            pipeline.Bindings.SetSampler("defaultSampler", sampler);
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

        public void Draw(IGraphicsContext context, IRenderTargetView[] rtvs, uint width, uint height)
        {
            context.SetViewport(new(width, height));
            pipeline.Bindings.SetSRV("inputTexture", Source);
            context.SetGraphicsPipelineState(pipeline);

            for (int i = 0; i < 6; i++)
            {
                context.SetRenderTarget(rtvs[i], null);
                viewBuffer.Update(context, Cameras[i].ViewProjection);
                cube.DrawAuto(context);
            }

            context.SetGraphicsPipelineState(null);
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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}