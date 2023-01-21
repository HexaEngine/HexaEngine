#nullable disable

namespace HexaEngine.Pipelines.Deferred
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Resources;
    using System;

    public class DeferredPrincipledBSDF : IEffect
    {
        private Quad quad;
        private IGraphicsPipeline brdf;
        private ISamplerState pointSampler;
        private ISamplerState anisoSampler;
        private unsafe void** cbs;
        private unsafe void** smps;
        private unsafe void** srvs;
        private const uint nsrvs = 8 + 4 + CBLight.MaxDirectionalLightSDs + CBLight.MaxPointLightSDs + CBLight.MaxSpotlightSDs;
        private bool disposedValue;

        public IRenderTargetView Output;

        public IShaderResourceView[] GBuffers;
        public IShaderResourceView Irraidance;
        public IShaderResourceView EnvPrefiltered;
        public IShaderResourceView LUT;
        public IShaderResourceView SSAO;
        public IShaderResourceView CSM;
        public IShaderResourceView[] OSMs;
        public IShaderResourceView[] PSMs;
        public IBuffer Camera;
        public IBuffer Lights;

        public async Task Initialize(IGraphicsDevice device, int width, int height)
        {
            Output = ResourceManager.AddTextureRTV("LightBuffer", TextureDescription.CreateTexture2DWithRTV(width, height, 1));
            quad = new(device);
            brdf = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "deferred/bsdf/vs.hlsl",
                PixelShader = "deferred/bsdf/ps.hlsl",
            });

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

            Lights = await ResourceManager.GetConstantBufferAsync("CBLight");
            Camera = await ResourceManager.GetConstantBufferAsync("CBCamera");
            GBuffers = await ResourceManager.GetTextureArraySRVAsync("GBuffer");
            Irraidance = await ResourceManager.GetTextureSRVAsync("EnvironmentIrradiance");
            EnvPrefiltered = await ResourceManager.GetTextureSRVAsync("EnvironmentPrefilter");
            LUT = await ResourceManager.GetTextureSRVAsync("BRDFLUT");
            SSAO = await ResourceManager.GetTextureSRVAsync("SSAOBuffer");

            UpdateResources();
        }

        public void BeginResize()
        {
            ResourceManager.RequireUpdate("LightBuffer");
        }

        public async void EndResize(int width, int height)
        {
            Output = ResourceManager.UpdateTextureRTV("LightBuffer", TextureDescription.CreateTexture2DWithRTV(width, height, 1));

            Lights = await ResourceManager.GetConstantBufferAsync("CBLight");
            Camera = await ResourceManager.GetConstantBufferAsync("CBCamera");
            GBuffers = await ResourceManager.GetTextureArraySRVAsync("GBuffer");
            Irraidance = await ResourceManager.GetTextureSRVAsync("EnvironmentIrradiance");
            EnvPrefiltered = await ResourceManager.GetTextureSRVAsync("EnvironmentPrefilter");
            LUT = await ResourceManager.GetTextureSRVAsync("BRDFLUT");
            SSAO = await ResourceManager.GetTextureSRVAsync("SSAOBuffer");

            UpdateResources();
        }

        public void UpdateTextures()
        {
            Irraidance = ResourceManager.GetTextureSRV("EnvironmentIrradiance");
            EnvPrefiltered = ResourceManager.GetTextureSRV("EnvironmentPrefilter");
            UpdateResources();
        }

        private unsafe void UpdateResources()
        {
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
        }

        public unsafe void Draw(IGraphicsContext context)
        {
            if (Output == null) return;
            context.ClearRenderTargetView(Output, default);
            context.SetRenderTarget(Output, default);
            context.PSSetConstantBuffers(cbs, 2, 0);
            context.PSSetShaderResources(srvs, nsrvs, 0);
            context.PSSetSamplers(smps, 2, 0);
            quad.DrawAuto(context, brdf, Output.Viewport);
        }

        protected virtual unsafe void Dispose(bool disposing)
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

        ~DeferredPrincipledBSDF()
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