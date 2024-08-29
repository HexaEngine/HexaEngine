namespace HexaEngine.Graphics.Filters
{
    using HexaEngine.Core.Graphics;
    using System;

    /// <summary>
    /// Compute shader based approach of diffuse importance sampling for IBL
    /// Should be more memory efficient than the Pixel shader approach.
    /// </summary>
    public class IBLDiffuseIrradianceCompute : IDisposable
    {
        private readonly IComputePipelineState pipeline;
        private readonly ISamplerState sampler;

        public IShaderResourceView? Source;
        public IUnorderedAccessView? Target;

        private bool disposedValue;

        private const uint ThreadCount = 32;

        public IBLDiffuseIrradianceCompute(IGraphicsDevice device)
        {
            pipeline = device.CreateComputePipelineState(new ComputePipelineDesc()
            {
                Path = "filter/irradiance/cs.hlsl"
            });

            sampler = device.CreateSamplerState(new(Filter.MinMagMipLinear, TextureAddressMode.Clamp));
            pipeline.Bindings.SetSampler("defaultSampler", sampler);
        }

        public unsafe void Dispatch(IGraphicsContext context, uint width, uint height)
        {
            if (Target == null)
            {
                return;
            }

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