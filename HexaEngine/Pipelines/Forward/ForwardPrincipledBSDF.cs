namespace HexaEngine.Pipelines.Forward
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Rendering.ConstantBuffers;
    using System;

    public unsafe class ForwardPrincipledBSDF : IEffect
    {
        private readonly GraphicsPipeline brdf;
        private readonly ISamplerState pointSampler;
        private readonly ISamplerState anisoSampler;
        private readonly void** cbs;
        private readonly void** smps;
        private readonly void** srvs;
        private const uint nsrvs = 4 + CBLight.MaxDirectionalLightSDs + CBLight.MaxPointLightSDs + CBLight.MaxSpotlightSDs;
        private bool disposedValue;

        public IRenderTargetView? Output;
        public IDepthStencilView? DSV;

        public IShaderResourceView? Irraidance;
        public IShaderResourceView? EnvPrefiltered;
        public IShaderResourceView? LUT;
        public IShaderResourceView? SSAO;
        public IShaderResourceView? CSM;
        public IShaderResourceView[]? OSMs;
        public IShaderResourceView[]? PSMs;
        public IBuffer? Camera;
        public IBuffer? Lights;

        public ForwardPrincipledBSDF(IGraphicsDevice device)
        {
            brdf = new(device, new()
            {
                VertexShader = "forward/bsdf/vs.hlsl",
                HullShader = "forward/bsdf/hs.hlsl",
                DomainShader = "forward/bsdf/ds.hlsl",
                PixelShader = "forward/bsdf/ps.hlsl",
            },
        new ShaderMacro[]
        {
            new("INSTANCED", 1)
        });
            brdf.State = new()
            {
                Blend = BlendDescription.AlphaBlend,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.PatchListWith3ControlPoints,
            };
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

            srvs[0] = (void*)Irraidance?.NativePointer;
            srvs[1] = (void*)EnvPrefiltered?.NativePointer;
            srvs[2] = (void*)LUT?.NativePointer;
            srvs[3] = (void*)SSAO?.NativePointer;
            srvs[4] = (void*)CSM?.NativePointer;
            if (OSMs != null)
                for (int i = 0; i < OSMs.Length; i++)
                {
                    srvs[i + 5] = (void*)OSMs[i]?.NativePointer;
                }
            if (PSMs != null)
                for (int i = 0; i < PSMs.Length; i++)
                {
                    srvs[i + 5 + CBLight.MaxPointLightSDs] = (void*)PSMs[i]?.NativePointer;
                }
#nullable enable
        }

        public void Draw(IGraphicsContext context)
        {
            if (Output == null) return;
            context.SetRenderTarget(Output, DSV);
            context.PSSetConstantBuffers(cbs, 2, 0);
            context.PSSetShaderResources(srvs, nsrvs, 8);
            context.PSSetSamplers(smps, 2, 1);
            brdf.BeginDraw(context, Output.Viewport);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                brdf.Dispose();
                pointSampler.Dispose();
                anisoSampler.Dispose();
                Free(cbs);
                Free(smps);
                Free(srvs);
                disposedValue = true;
            }
        }

        ~ForwardPrincipledBSDF()
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