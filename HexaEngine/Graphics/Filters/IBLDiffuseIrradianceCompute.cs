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
        private readonly IComputePipeline pipeline;
        private readonly ISamplerState sampler;

        public IShaderResourceView? Source;
        public IUnorderedAccessView? Target;

        private bool disposedValue;

        private const uint ThreadCount = 32;

        public IBLDiffuseIrradianceCompute(IGraphicsDevice device)
        {
            pipeline = device.CreateComputePipeline(new()
            {
                Path = "filter/irradiance/cs.hlsl"
            });

            sampler = device.CreateSamplerState(new(Filter.MinMagMipLinear, TextureAddressMode.Clamp));
        }

        public unsafe void Dispatch(IGraphicsContext context, uint width, uint height)
        {
            if (Target == null)
            {
                return;
            }

            context.CSSetSampler(0, sampler);
            context.CSSetShaderResource(0, Source);
            context.CSSetUnorderedAccessView((void*)Target.NativePointer);
            context.SetComputePipeline(pipeline);
            context.Dispatch(width / ThreadCount, height / ThreadCount, 6);
            context.SetComputePipeline(null);
            context.CSSetUnorderedAccessView(null);
            context.CSSetShaderResource(0, null);
            context.CSSetSampler(0, null);
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