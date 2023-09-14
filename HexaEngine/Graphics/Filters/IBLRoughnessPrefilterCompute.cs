namespace HexaEngine.Graphics.Filters
{
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
        private readonly IComputePipeline pipeline;
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

        public IBLRoughnessPrefilterCompute(IGraphicsDevice device)
        {
            pipeline = device.CreateComputePipeline(new()
            {
                Path = "filter/prefilter/cs.hlsl",
            });

            roughnessBuffer = new(device, CpuAccessFlags.Write);
            sampler = device.CreateSamplerState(new(Filter.MinMagMipLinear, TextureAddressMode.Clamp));
        }

        public unsafe void Dispatch(IGraphicsContext context, uint width, uint height)
        {
            if (Target == null)
            {
                return;
            }

            context.Write(roughnessBuffer, new RoughnessParams() { Roughness = Roughness });

            context.CSSetConstantBuffer(0, roughnessBuffer);
            context.CSSetShaderResource(0, Source);
            context.CSSetSampler(0, sampler);
            context.CSSetUnorderedAccessView((void*)Target.NativePointer);
            context.SetComputePipeline(pipeline);
            context.Dispatch(width / ThreadCount, height / ThreadCount, 6);
            context.SetComputePipeline(null);
            context.CSSetUnorderedAccessView(null);
            context.CSSetSampler(0, null);
            context.CSSetShaderResource(0, null);
            context.CSSetConstantBuffer(0, null);
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

        ~IBLRoughnessPrefilterCompute()
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