namespace HexaEngine.Pipelines.Deferred.Lighting
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Objects.Primitives;
    using HexaEngine.Rendering.ConstantBuffers;
    using System;

    public unsafe class PrincipledBSDF : IEffect
    {
        private readonly Quad quad;
        private readonly Pipeline brdf;
        private readonly ISamplerState pointSampler;
        private readonly ISamplerState anisoSampler;
        private readonly void** cbs;
        private readonly void** smps;
        private readonly void** srvs;
        private const uint nsrvs = 8 + 4 + CBLight.MaxDirectionalLightSDs + CBLight.MaxPointLightSDs + CBLight.MaxSpotlightSDs;
        private bool disposedValue;

        public IRenderTargetView? Output;

        public IShaderResourceView?[]? GBuffers;
        public IShaderResourceView? Irraidance;
        public IShaderResourceView? EnvPrefiltered;
        public IShaderResourceView? LUT;
        public IShaderResourceView? SSAO;
        public IShaderResourceView? CSM;
        public IShaderResourceView[]? OSMs;
        public IShaderResourceView[]? PSMs;
        public IBuffer? Camera;
        public IBuffer? Lights;

        public PrincipledBSDF(IGraphicsDevice device)
        {
            quad = new(device);
            brdf = new(device, new()
            {
                VertexShader = "deferred/pbrbrdf/vs.hlsl",
                PixelShader = "deferred/pbrbrdf/ps.hlsl",
            });
            pointSampler = device.CreateSamplerState(SamplerDescription.PointClamp);
            anisoSampler = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);
            smps = AllocArray(2);
            smps[0] = (void*)pointSampler.NativePointer;
            smps[1] = (void*)anisoSampler.NativePointer;
            srvs = AllocArray(nsrvs);
            cbs = AllocArray(2);
        }

        public void Resize()
        {
#nullable disable
            cbs[0] = (void*)Lights?.NativePointer;
            cbs[1] = (void*)Camera?.NativePointer;

            if (GBuffers != null)
                for (int i = 0; i < 8; i++)
                {
                    if (i < GBuffers.Length)
                    {
                        srvs[i] = (void*)GBuffers[i]?.NativePointer;
                    }
                }
            srvs[8] = (void*)Irraidance?.NativePointer;
            srvs[9] = (void*)EnvPrefiltered?.NativePointer;
            srvs[10] = (void*)LUT?.NativePointer;
            srvs[11] = (void*)SSAO?.NativePointer;
            srvs[12] = (void*)CSM?.NativePointer;
            if (OSMs != null)
                for (int i = 0; i < OSMs.Length; i++)
                {
                    srvs[i + 13] = (void*)OSMs[i]?.NativePointer;
                }
            if (PSMs != null)
                for (int i = 0; i < PSMs.Length; i++)
                {
                    srvs[i + 13 + CBLight.MaxPointLightSDs] = (void*)PSMs[i]?.NativePointer;
                }
#nullable enable
        }

        public void Draw(IGraphicsContext context)
        {
            if (Output == null) return;
            context.ClearRenderTargetView(Output, default);
            context.SetRenderTarget(Output, default);
            context.PSSetConstantBuffers(cbs, 2, 0);
            context.PSSetShaderResources(srvs, nsrvs, 0);
            context.PSSetSamplers(smps, 2, 0);
            quad.DrawAuto(context, brdf, Output.Viewport);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                quad.Dispose();
                brdf.Dispose();
                pointSampler.Dispose();
                anisoSampler.Dispose();
                Free(cbs);
                Free(smps);
                Free(srvs);
                disposedValue = true;
            }
        }

        ~PrincipledBSDF()
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