namespace HexaEngine.Pipelines.Forward
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Resources;
    using System;

    public class ForwardPrincipledBSDF : IEffect
    {
        private IGraphicsPipeline brdf;
        private ISamplerState pointSampler;
        private ISamplerState anisoSampler;
        private unsafe void** cbs;
        private unsafe void** smps;
        private unsafe void** srvs;
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

        public async Task Initialize(IGraphicsDevice device, int width, int height)
        {
            brdf = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "deferred/bsdf/vs.hlsl",
                PixelShader = "deferred/bsdf/ps.hlsl",
            });
            brdf.State = new()
            {
                Blend = BlendDescription.AlphaBlend,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.PatchListWith3ControlPoints,
            };

            pointSampler = ResourceManager.GetOrAddSamplerState("PointClamp", SamplerDescription.PointClamp);
            anisoSampler = ResourceManager.GetOrAddSamplerState("AnisotropicClamp", SamplerDescription.AnisotropicClamp);

            unsafe
            {
                smps = AllocArray(2);
                smps[0] = (void*)pointSampler.NativePointer;
                smps[1] = (void*)anisoSampler.NativePointer;
                srvs = AllocArray(nsrvs);
                cbs = AllocArray(2);
            }

            Output = await ResourceManager.GetTextureRTVAsync("LightBuffer");
            DSV = await ResourceManager.GetDepthStencilViewAsync("SwapChain.DSV");
            Lights = await ResourceManager.GetConstantBufferAsync("CBLight");
            Camera = await ResourceManager.GetConstantBufferAsync("CBCamera");
            Irraidance = await ResourceManager.GetTextureSRVAsync("EnvironmentIrradiance");
            EnvPrefiltered = await ResourceManager.GetTextureSRVAsync("EnvironmentPrefilter");
            LUT = await ResourceManager.GetTextureSRVAsync("BRDFLUT");
            SSAO = await ResourceManager.GetTextureSRVAsync("SSAOBuffer");

            UpdateResources();
        }

        public void BeginResize()
        {
        }

        public async void EndResize(int width, int height)
        {
            Output = await ResourceManager.GetTextureRTVAsync("LightBuffer");
            Lights = await ResourceManager.GetConstantBufferAsync("CBLight");
            Camera = await ResourceManager.GetConstantBufferAsync("CBCamera");
            Irraidance = await ResourceManager.GetTextureSRVAsync("EnvironmentIrradiance");
            EnvPrefiltered = await ResourceManager.GetTextureSRVAsync("EnvironmentPrefilter");
            LUT = await ResourceManager.GetTextureSRVAsync("BRDFLUT");
            SSAO = await ResourceManager.GetTextureSRVAsync("SSAOBuffer");
        }

        public void UpdateTextures()
        {
            Irraidance = ResourceManager.GetTextureSRV("EnvironmentIrradiance");
            EnvPrefiltered = ResourceManager.GetTextureSRV("EnvironmentPrefilter");
            UpdateResources();
        }

        public unsafe void UpdateResources()
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

        public unsafe void Draw(IGraphicsContext context)
        {
            if (Output == null) return;
            context.SetRenderTarget(Output, DSV);
            context.PSSetConstantBuffers(cbs, 2, 0);
            context.DSSetConstantBuffer(Camera, 1);
            context.PSSetShaderResources(srvs, nsrvs, 8);
            context.PSSetSamplers(smps, 2, 1);
            brdf.BeginDraw(context, Output.Viewport);
        }

        protected virtual unsafe void Dispose(bool disposing)
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