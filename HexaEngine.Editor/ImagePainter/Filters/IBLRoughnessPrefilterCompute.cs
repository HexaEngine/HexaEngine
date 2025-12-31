namespace HexaEngine.Editor.ImagePainter.Filters
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using System;
    using System.Numerics;

    /// <summary>
    /// Compute shader based approach of specular importance sampling for IBL
    /// Should be more memory efficient than the Pixel shader approach.
    /// </summary>
    public class IBLRoughnessPrefilterCompute : IDisposable
    {
        private readonly IComputePipelineState pipeline;
        private readonly ConstantBuffer<RoughnessParams> roughnessBuffer;
        private readonly ISamplerState sampler;

        public IUnorderedAccessView? Target;
        public IShaderResourceView? Source;

        public struct RoughnessParams
        {
            public float Roughness;
            public Vector3 padd;
        }

        public float Roughness;
        private bool disposedValue;

        private const int ThreadCount = 32;

        public IBLRoughnessPrefilterCompute()
        {
            var device = Application.GraphicsDevice;
            pipeline = device.CreateComputePipelineState(new ComputePipelineDesc()
            {
                Path = "HexaEngine.ImagePainter:shaders/filter/prefilter/cs.hlsl",
            });

            roughnessBuffer = new(CpuAccessFlags.Write);
            sampler = device.CreateSamplerState(new(Filter.MinMagMipLinear, TextureAddressMode.Clamp));
            pipeline.Bindings.SetSampler("defaultSampler", sampler);
            pipeline.Bindings.SetCBV("ParamsBuffer", roughnessBuffer);
        }

        public unsafe void Dispatch(IGraphicsContext context, uint width, uint height)
        {
            if (Target == null)
            {
                return;
            }

            context.Write(roughnessBuffer, new RoughnessParams() { Roughness = Roughness });

            pipeline.Bindings.SetSRV("inputTexture", Source!);
            pipeline.Bindings.SetUAV("outputTexture", Target!);

            context.SetComputePipelineState(pipeline);
            context.Dispatch(width / ThreadCount, height / ThreadCount, 6);
            context.SetComputePipelineState(null);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                pipeline.Dispose();
                roughnessBuffer.Dispose();
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