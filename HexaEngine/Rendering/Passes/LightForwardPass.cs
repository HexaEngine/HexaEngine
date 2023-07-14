#nullable disable

namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Lights;
    using HexaEngine.Meshes;
    using HexaEngine.Rendering;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Scenes;

    public class LightForwardPass : RenderPass
    {
        private ConstantBuffer<ForwardLightParams> lightParamsBuffer;

        private ISamplerState linearClampSampler;
        private ISamplerState linearWrapSampler;
        private ISamplerState pointClampSampler;
        private ISamplerState shadowSampler;

        private unsafe void** cbs;
        private const uint nConstantBuffers = 3;
        private unsafe void** smps;
        private const uint nSamplers = 4;

        private unsafe void** forwardRTVs;
        private const uint nForwardRTVs = 3;

        private unsafe void** forwardSRVs;
        private const uint nForwardSRVs = 8 + 9;
        private const uint nForwardIndirectSRVsBase = 8 + 7;

        private unsafe void** forwardClusteredSRVs;
        private const uint nForwardClusteredSRVs = 8 + 11;
        private const uint nForwardClusteredIndirectSRVsBase = 8 + 9;

        public LightForwardPass() : base("Lightning")
        {
            AddWriteDependency(new("LightBuffer"));
            AddWriteDependency(new("GBuffer"));
            AddReadDependency(new("AOBuffer"));
            AddReadDependency(new("#ShadowAtlas"));
        }

        private readonly bool forceForward = true;
        private readonly bool clustered = true;

        public override unsafe void Init(ResourceCreator creator, PipelineCreator pipelineCreator, IGraphicsDevice device)
        {
            lightParamsBuffer = creator.CreateConstantBuffer<ForwardLightParams>("ForwardLightParams", CpuAccessFlags.Write);

            linearClampSampler = creator.CreateSamplerState("PointClamp", SamplerStateDescription.LinearClamp);
            linearWrapSampler = creator.CreateSamplerState("LinearWrap", SamplerStateDescription.LinearWrap);
            pointClampSampler = creator.CreateSamplerState("PointClamp", SamplerStateDescription.PointClamp);
            shadowSampler = creator.CreateSamplerState("LinearComparisonBorder", SamplerStateDescription.ComparisonLinearBorder);

            smps = AllocArrayAndZero(nSamplers);
            smps[0] = (void*)linearClampSampler.NativePointer;
            smps[1] = (void*)linearWrapSampler.NativePointer;
            smps[2] = (void*)pointClampSampler.NativePointer;
            smps[3] = (void*)shadowSampler.NativePointer;

            cbs = AllocArrayAndZero(nConstantBuffers);
            cbs[1] = (void*)creator.GetConstantBuffer<CBCamera>("CBCamera").NativePointer;
            cbs[2] = (void*)creator.GetConstantBuffer<CBWeather>("CBWeather").NativePointer;

            forwardSRVs = AllocArrayAndZero(nForwardSRVs);
            forwardClusteredSRVs = AllocArrayAndZero(nForwardClusteredSRVs);

            forwardRTVs = AllocArrayAndZero(nForwardRTVs);
        }

        public override unsafe void Execute(IGraphicsContext context, ResourceCreator creator)
        {
            var current = SceneManager.Current;
            if (current == null)
            {
                return;
            }

            var renderers = current.RenderManager;
            var lights = current.LightManager;
            var globalProbes = lights.GlobalProbes;

            var gbuffer = creator.GetGBuffer("GBuffer");

            forwardSRVs[8] = forwardClusteredSRVs[8] = (void*)creator.GetTexture2D("AOBuffer").SRV.NativePointer;

            forwardSRVs[9] = forwardClusteredSRVs[9] = (void*)creator.GetTexture2D("BRDFLUT").SRV.NativePointer;
            forwardSRVs[10] = (void*)globalProbes.SRV.NativePointer;

            forwardSRVs[11] = forwardClusteredSRVs[11] = (void*)lights.LightBuffer.SRV.NativePointer;
            forwardSRVs[12] = forwardClusteredSRVs[12] = (void*)lights.ShadowDataBuffer.SRV.NativePointer;

            forwardClusteredSRVs[13] = (void*)creator.GetStructuredUavBuffer<uint>("LightIndexList").SRV.NativePointer;
            forwardClusteredSRVs[14] = (void*)creator.GetStructuredUavBuffer<LightGrid>("LightGridBuffer").SRV.NativePointer;

            forwardSRVs[13] = forwardClusteredSRVs[15] = (void*)creator.GetDepthStencilBuffer("#ShadowAtlas").SRV.NativePointer;

            forwardRTVs[0] = (void*)creator.GetTexture2D("LightBuffer").NativePointer;
            forwardRTVs[1] = gbuffer.PRTVs[1];
            forwardRTVs[2] = gbuffer.PRTVs[2];

            context.SetRenderTargets(nForwardRTVs, forwardRTVs, creator.GetDepthStencilBuffer("#DepthStencil").DSV);

            if (clustered)
            {
                ClusteredForwardBegin(context, creator, lights);
            }
            else
            {
                ForwardBegin(context, creator, lights);
            }

            if (forceForward)
            {
                var geometryQueue = renderers.GeometryQueue;
                for (int i = 0; i < geometryQueue.Count; i++)
                {
                    geometryQueue[i].Draw(context, RenderPath.Forward);
                }
            }
            var alphaTest = renderers.AlphaTestQueue;
            for (int i = 0; i < alphaTest.Count; i++)
            {
                alphaTest[i].Draw(context, RenderPath.Forward);
            }
            var transparency = renderers.TransparencyQueue;
            for (int i = 0; i < transparency.Count; i++)
            {
                transparency[i].Draw(context, RenderPath.Forward);
            }

            if (clustered)
            {
                ClusteredForwardEnd(context);
            }
            else
            {
                ForwardEnd(context);
            }
        }

        private unsafe void ForwardBegin(IGraphicsContext context, ResourceCreator creator, LightManager lights)
        {
            var lightParams = lightParamsBuffer.Local;
            lightParams->LightCount = lights.LightBuffer.Count;
            lightParams->GlobalProbes = lights.GlobalProbes.Count;
            lightParamsBuffer.Update(context);
            cbs[0] = (void*)lightParamsBuffer.Buffer?.NativePointer;

            context.SetViewport(creator.GetViewport());
            context.VSSetConstantBuffers(1, 1, &cbs[1]);
            context.DSSetConstantBuffers(1, 1, &cbs[1]);
            context.CSSetConstantBuffers(1, 1, &cbs[1]);
            context.PSSetConstantBuffers(0, nConstantBuffers, cbs);
            context.PSSetShaderResources(8, nForwardSRVs, forwardSRVs);
            context.PSSetSamplers(8, nSamplers, smps);
        }

        private unsafe void ForwardEnd(IGraphicsContext context)
        {
            nint* null_samplers = stackalloc nint[(int)nSamplers];
            context.PSSetSamplers(8, nSamplers, (void**)null_samplers);

            nint* null_srvs = stackalloc nint[(int)nForwardSRVs];
            context.PSSetShaderResources(8, nForwardSRVs, (void**)null_srvs);

            nint* null_cbs = stackalloc nint[(int)nConstantBuffers];
            context.PSSetConstantBuffers(0, nConstantBuffers, (void**)null_cbs);
        }

        private unsafe void ClusteredForwardBegin(IGraphicsContext context, ResourceCreator creator, LightManager lights)
        {
            var lightParams = lightParamsBuffer.Local;
            lightParams->LightCount = lights.LightBuffer.Count;
            lightParams->GlobalProbes = lights.GlobalProbes.Count;
            lightParamsBuffer.Update(context);
            cbs[0] = (void*)lightParamsBuffer.Buffer?.NativePointer;

            context.SetViewport(creator.GetViewport());
            context.VSSetConstantBuffers(1, 1, &cbs[1]);
            context.DSSetConstantBuffers(1, 1, &cbs[1]);
            context.CSSetConstantBuffers(1, 1, &cbs[1]);
            context.PSSetConstantBuffers(0, nConstantBuffers, cbs);
            context.PSSetShaderResources(0, nForwardClusteredSRVs, forwardClusteredSRVs);
            context.PSSetSamplers(8, nSamplers, smps);
        }

        private unsafe void ClusteredForwardEnd(IGraphicsContext context)
        {
            nint* null_samplers = stackalloc nint[(int)nSamplers];
            context.PSSetSamplers(8, nSamplers, (void**)null_samplers);

            nint* null_srvs = stackalloc nint[(int)nForwardClusteredSRVs];
            context.PSSetShaderResources(8, nForwardClusteredSRVs, (void**)null_srvs);

            nint* null_cbs = stackalloc nint[(int)nConstantBuffers];
            context.PSSetConstantBuffers(0, nConstantBuffers, (void**)null_cbs);
        }
    }
}