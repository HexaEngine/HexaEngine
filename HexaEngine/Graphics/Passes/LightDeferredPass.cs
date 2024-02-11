#nullable disable

using HexaEngine;

namespace HexaEngine.Graphics.Passes
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Types;
    using HexaEngine.Meshes;
    using HexaEngine.Scenes;

    public class LightDeferredPass : DrawPass
    {
        private ResourceRef<DepthStencil> depthStencil;
        private ResourceRef<GBuffer> gbuffer;
        private ResourceRef<Texture2D> AOBuffer;
        private ResourceRef<Texture2D> brdfLUT;
        private ResourceRef<StructuredUavBuffer<uint>> lightIndexList;
        private ResourceRef<StructuredUavBuffer<LightGrid>> lightGridBuffer;
        private ResourceRef<Texture2D> lightBuffer;
        private ResourceRef<ShadowAtlas> shadowAtlas;
        private ResourceRef<ConstantBuffer<ProbeBufferParams>> probeParamsBuffer;
        private ResourceRef<ConstantBuffer<DeferredLightParams>> lightParamsBuffer;

        private ResourceRef<ISamplerState> linearClampSampler;
        private ResourceRef<ISamplerState> linearWrapSampler;
        private ResourceRef<ISamplerState> pointClampSampler;
        private ResourceRef<ISamplerState> shadowSampler;

        private unsafe void** cbs;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;
        private ResourceRef<ConstantBuffer<CBWeather>> weather;
        private const uint nConstantBuffers = 3;
        private unsafe void** smps;
        private const uint nSamplers = 4;

        private ResourceRef<IGraphicsPipelineState> deferredIndirect;
        private unsafe void** indirectSrvs;
        private const uint nIndirectSrvsBase = 11;
        private const uint nIndirectSrvs = 13;

        private ResourceRef<IGraphicsPipelineState> deferred;
        private unsafe void** deferredSrvs;
        private const uint nDeferredSrvs = 10;

        private ResourceRef<IGraphicsPipelineState> deferredClusterd;
        private unsafe void** deferredClusterdSrvs;
        private const uint nDeferredClusterdSrvs = 12;

        private readonly bool forceForward = true;
        private readonly bool clustered = true;

        public LightDeferredPass(Windows.RendererFlags flags) : base("LightDeferred")
        {
            forceForward = (flags & Windows.RendererFlags.ForceForward) != 0;
            AddWriteDependency(new("LightBuffer"));
            AddReadDependency(new("GBuffer"));
            AddReadDependency(new("#AOBuffer"));
            AddReadDependency(new("ShadowAtlas"));
            AddReadDependency(new("BRDFLUT"));
        }

        public override unsafe void Init(GraphResourceBuilder creator, ICPUProfiler profiler)
        {
            depthStencil = creator.GetDepthStencilBuffer("#DepthStencil");
            gbuffer = creator.GetGBuffer("GBuffer");
            AOBuffer = creator.GetTexture2D("#AOBuffer");

            brdfLUT = creator.GetTexture2D("BRDFLUT");

            lightIndexList = creator.GetStructuredUavBuffer<uint>("LightIndexList");
            lightGridBuffer = creator.GetStructuredUavBuffer<LightGrid>("LightGridBuffer");

            lightBuffer = creator.GetTexture2D("LightBuffer");

            shadowAtlas = creator.GetShadowAtlas("ShadowAtlas");

            probeParamsBuffer = creator.CreateConstantBuffer<ProbeBufferParams>("ProbeBufferParams", CpuAccessFlags.Write);
            lightParamsBuffer = creator.CreateConstantBuffer<DeferredLightParams>("DeferredLightParams", CpuAccessFlags.Write);

            smps = AllocArrayAndZero(nSamplers);
            linearClampSampler = creator.CreateSamplerState("PointClamp", SamplerStateDescription.LinearClamp);
            linearWrapSampler = creator.CreateSamplerState("LinearWrap", SamplerStateDescription.LinearWrap);
            pointClampSampler = creator.CreateSamplerState("PointClamp", SamplerStateDescription.PointClamp);
            shadowSampler = creator.CreateSamplerState("LinearComparisonBorder", SamplerStateDescription.ComparisonLinearBorder);

            cbs = AllocArrayAndZero(nConstantBuffers);
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");
            weather = creator.GetConstantBuffer<CBWeather>("CBWeather");

            indirectSrvs = AllocArrayAndZero(nIndirectSrvs);
            deferredSrvs = AllocArrayAndZero(nDeferredSrvs);
            deferredClusterdSrvs = AllocArrayAndZero(nDeferredClusterdSrvs);

            deferredIndirect = creator.CreateGraphicsPipelineState(new()
            {
                Pipeline = new()
                {
                    VertexShader = "quad.hlsl",
                    PixelShader = "deferred/brdf/indirect.hlsl",
                },
                State = GraphicsPipelineStateDesc.DefaultAdditiveFullscreen
            });

            deferred = creator.CreateGraphicsPipelineState(new()
            {
                Pipeline = new()
                {
                    VertexShader = "quad.hlsl",
                    PixelShader = "deferred/brdf/light.hlsl",
                },
                State = GraphicsPipelineStateDesc.DefaultAdditiveFullscreen
            });

            deferredClusterd = creator.CreateGraphicsPipelineState(new()
            {
                Pipeline = new()
                {
                    VertexShader = "quad.hlsl",
                    PixelShader = "deferred/brdf/light.hlsl",
                    Macros = [new("CLUSTERED_DEFERRED", 1)]
                },
                State = GraphicsPipelineStateDesc.DefaultAdditiveFullscreen,
            });
        }

        public override unsafe void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler profiler)
        {
            if (forceForward)
                return;

            var current = SceneManager.Current;
            if (current == null)
            {
                return;
            }

            var renderers = current.RenderManager;
            var lights = current.LightManager;
            var globalProbes = lights.GlobalProbes;

            var gbuffer = this.gbuffer.Value;
            for (int i = 0; i < 4; i++)
            {
                if (i < gbuffer.Count)
                {
                    deferredSrvs[i] = indirectSrvs[i] = deferredClusterdSrvs[i] = (void*)gbuffer.SRVs[i]?.NativePointer;
                }
            }

            cbs[1] = (void*)camera.Value.NativePointer;
            cbs[2] = (void*)weather.Value.NativePointer;

            smps[0] = (void*)linearClampSampler.Value.NativePointer;
            smps[1] = (void*)linearWrapSampler.Value.NativePointer;
            smps[2] = (void*)pointClampSampler.Value.NativePointer;
            smps[3] = (void*)shadowSampler.Value.NativePointer;

            deferredSrvs[4] = indirectSrvs[4] = deferredClusterdSrvs[4] = (void*)depthStencil.Value.SRV.NativePointer;
            indirectSrvs[5] = deferredSrvs[5] = deferredClusterdSrvs[5] = (void*)AOBuffer.Value.SRV.NativePointer;
            indirectSrvs[9] = (void*)brdfLUT.Value.SRV.NativePointer;
            indirectSrvs[10] = (void*)globalProbes.SRV.NativePointer;

            deferredSrvs[6] = deferredClusterdSrvs[6] = (void*)lights.LightBuffer.SRV.NativePointer;
            deferredSrvs[7] = deferredClusterdSrvs[7] = (void*)lights.ShadowDataBuffer.SRV.NativePointer;

            deferredClusterdSrvs[8] = (void*)lightIndexList.Value.SRV.NativePointer;
            deferredClusterdSrvs[9] = (void*)lightGridBuffer.Value.SRV.NativePointer;

            deferredClusterdSrvs[10] = deferredSrvs[8] = (void*)shadowAtlas.Value.SRV.NativePointer;
            var dir = lights.Active.FirstOrDefault(x => x is DirectionalLight && x.ShadowMapEnable);
            if (dir != null)
            {
                deferredClusterdSrvs[11] = deferredSrvs[9] = (void*)((DirectionalLight)dir).GetShadowMap().NativePointer;
            }

            context.SetRenderTarget(lightBuffer.Value.RTV, depthStencil.Value);
            context.SetViewport(creator.Viewport);

            context.VSSetConstantBuffer(1, camera.Value);
            context.DSSetConstantBuffer(1, camera.Value);
            context.GSSetConstantBuffer(1, camera.Value);
            context.CSSetConstantBuffer(1, camera.Value);
            cbs[0] = null;
            context.PSSetConstantBuffers(0, nConstantBuffers, cbs);

            profiler?.Begin("LightForward.Background");
            RenderManager.ExecuteGroup(renderers.BackgroundQueue, context, profiler, "LightForward", RenderPath.Forward);
            profiler?.End("LightForward.Background");

            context.VSSetConstantBuffer(1, null);
            context.DSSetConstantBuffer(1, null);
            context.GSSetConstantBuffer(1, null);
            context.CSSetConstantBuffer(1, null);

            context.SetRenderTarget(lightBuffer.Value.RTV, null);

            // Indirect light pass
            var probeParamsBuffer = this.probeParamsBuffer.Value;
            var probeParams = probeParamsBuffer.Local;
            probeParams->GlobalProbes = globalProbes.Count;
            probeParamsBuffer.Update(context);
            cbs[0] = (void*)probeParamsBuffer.Buffer?.NativePointer;

            context.PSSetSamplers(0, nSamplers, smps);
            context.PSSetConstantBuffers(0, nConstantBuffers, cbs);
            context.PSSetShaderResources(0, nIndirectSrvs, indirectSrvs);

            context.SetPipelineState(deferredIndirect.Value);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetPipelineState(null);

            nint* null_samplers = stackalloc nint[4];
            context.PSSetSamplers(0, 4, (void**)null_samplers);

            nint* null_cbs = stackalloc nint[3];
            context.PSSetConstantBuffers(0, 3, (void**)null_cbs);

            nint* null_srvs = stackalloc nint[(int)nIndirectSrvs];
            context.PSSetShaderResources(0, nIndirectSrvs, (void**)null_srvs);

            if (clustered)
            {
                DeferredClustered(context, creator);
            }
            else
            {
                Deferred(context, creator, lights);
            }
        }

        private unsafe void Deferred(IGraphicsContext context, GraphResourceBuilder creator, LightManager lights)
        {
            // Direct light pass
            var lightParamsBuffer = this.lightParamsBuffer.Value;
            var lightParams = lightParamsBuffer.Local;
            lightParams->LightCount = lights.LightBuffer.Count;
            lightParamsBuffer.Update(context);
            cbs[0] = (void*)lightParamsBuffer.Buffer?.NativePointer;

            context.PSSetSamplers(0, nSamplers, smps);
            context.PSSetConstantBuffers(0, nConstantBuffers, cbs);
            context.PSSetShaderResources(0, nDeferredSrvs, deferredSrvs);

            context.SetPipelineState(deferred.Value);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetPipelineState(null);

            nint* null_samplers = stackalloc nint[(int)nSamplers];
            context.PSSetSamplers(0, nSamplers, (void**)null_samplers);

            nint* null_cbs = stackalloc nint[(int)nConstantBuffers];
            context.PSSetConstantBuffers(0, nConstantBuffers, (void**)null_cbs);

            nint* null_srvs = stackalloc nint[(int)nDeferredSrvs];
            context.PSSetShaderResources(0, nDeferredSrvs, (void**)null_srvs);
        }

        private unsafe void DeferredClustered(IGraphicsContext context, GraphResourceBuilder creator)
        {
            // Direct clustered light pass
            context.PSSetSamplers(0, nSamplers, smps);
            context.PSSetConstantBuffers(1, 1, &cbs[1]);
            context.PSSetShaderResources(0, nDeferredClusterdSrvs, deferredClusterdSrvs);

            context.SetPipelineState(deferredClusterd.Value);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetPipelineState(null);

            nint* null_samplers = stackalloc nint[(int)nSamplers];
            context.PSSetSamplers(0, nSamplers, (void**)null_samplers);

            nint* null_cbs = stackalloc nint[(int)nConstantBuffers];
            context.PSSetConstantBuffers(0, nConstantBuffers, (void**)null_cbs);

            nint* null_srvs = stackalloc nint[(int)nDeferredClusterdSrvs];
            context.PSSetShaderResources(0, nDeferredClusterdSrvs, (void**)null_srvs);
        }
    }
}