namespace HexaEngine.Graphics.Filters
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using System;
    using System.Numerics;

    /// <summary>
    /// Pixel shader based approach of specular importance sampling for IBL
    /// </summary>
    public class IBLRoughnessPrefilter : IDisposable
    {
        private readonly Cube cube;
        private readonly IGraphicsPipelineState pipeline;
        private readonly ConstantBuffer<Matrix4x4> viewBuffer;
        private readonly ConstantBuffer<RoughnessParams> roughnessBuffer;
        private readonly ISamplerState sampler;

        public RenderTargetViewArray? Targets;
        public IShaderResourceView? Source;

        public struct CubeFaceCamera
        {
            public Matrix4x4 ViewProjection;
        }

        public struct RoughnessParams
        {
            public float Roughness;
            public Vector3 padd;
        }

        private readonly CubeFaceCamera[] Cameras;

        public float Roughness;
        private bool disposedValue;

        public IBLRoughnessPrefilter(IGraphicsDevice device)
        {
            Cameras = new CubeFaceCamera[6];
            cube = new();
            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "filter/prefilter/vs.hlsl",
                PixelShader = "filter/prefilter/ps.hlsl",
            }, new()
            {
                DepthStencil = DepthStencilDescription.None,
                Rasterizer = RasterizerDescription.CullNone,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            });

            SetViewPoint(Vector3.Zero);
            viewBuffer = new(CpuAccessFlags.Write);
            roughnessBuffer = new(CpuAccessFlags.Write);
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

            context.Write(roughnessBuffer, new RoughnessParams() { Roughness = Roughness });

            context.SetViewport(new(width, height));
            context.PSSetSampler(0, sampler);
            context.PSSetShaderResource(0, Source);
            context.PSSetConstantBuffer(0, roughnessBuffer);
            context.VSSetConstantBuffer(0, viewBuffer);

            for (int i = 0; i < 6; i++)
            {
                viewBuffer.Update(context, Cameras[i].ViewProjection);
                Targets.SetTarget(context, i);
                cube.DrawAuto(context, pipeline);
            }

            context.VSSetConstantBuffer(0, null);
            context.PSSetConstantBuffer(0, null);
            context.PSSetShaderResource(0, null);
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
                roughnessBuffer.Dispose();
                sampler.Dispose();
                disposedValue = true;
            }
        }

        ~IBLRoughnessPrefilter()
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