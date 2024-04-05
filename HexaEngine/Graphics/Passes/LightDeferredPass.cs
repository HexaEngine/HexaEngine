#nullable disable

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

        private unsafe void** cbs;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;
        private ResourceRef<ConstantBuffer<CBWeather>> weather;
        private const uint nConstantBuffers = 3;
        private unsafe void** smps;
        private const uint nSamplers = 3;

        private ResourceRef<IGraphicsPipelineState> deferred;
        private unsafe void** deferredSrvs;
        private const uint nDeferredSrvs = 16;

        private readonly bool forceForward = true;

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

            cbs = AllocArrayAndZero(nConstantBuffers);
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");
            weather = creator.GetConstantBuffer<CBWeather>("CBWeather");

            deferredSrvs = AllocArrayAndZero(nDeferredSrvs);

            deferred = creator.CreateGraphicsPipelineState(new()
            {
                Pipeline = new()
                {
                    VertexShader = "quad.hlsl",
                    PixelShader = "deferred/brdf/light.hlsl",
                },
                State = GraphicsPipelineStateDesc.DefaultAdditiveFullscreen
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

            deferredSrvs[0] = (void*)AOBuffer.Value.SRV.NativePointer;
            deferredSrvs[1] = (void*)brdfLUT.Value.SRV.NativePointer;
            deferredSrvs[2] = (void*)globalProbes.SRV.NativePointer;
            deferredSrvs[3] = (void*)lights.LightBuffer.SRV.NativePointer;
            deferredSrvs[4] = (void*)lights.ShadowDataBuffer.SRV.NativePointer;
            deferredSrvs[5] = (void*)lightIndexList.Value.SRV.NativePointer;
            deferredSrvs[6] = (void*)lightGridBuffer.Value.SRV.NativePointer;
            deferredSrvs[7] = (void*)shadowAtlas.Value.SRV.NativePointer;
            for (int i = 0; i < lights.ActiveCount; i++)
            {
                var light = lights.Active[i];
                if (light is DirectionalLight directional && directional.ShadowMapEnable)
                {
                    deferredSrvs[8] = (void*)(directional.GetShadowMap()?.NativePointer ?? 0);
                }
            }

            deferredSrvs[9] = null; // IBL global diffuse
            deferredSrvs[10] = null; // IBL global specular
            const int deferredBaseIndex = 11;
            GBuffer gbuffer = this.gbuffer.Value;
            for (int i = 0; i < 4; i++)
            {
                if (i < gbuffer.Count)
                {
                    deferredSrvs[i + deferredBaseIndex] = (void*)gbuffer.SRVs[i]?.NativePointer;
                }
            }
            deferredSrvs[deferredBaseIndex + 4] = (void*)depthStencil.Value.SRV.NativePointer;

            cbs[1] = (void*)camera.Value.NativePointer;
            cbs[2] = (void*)weather.Value.NativePointer;

            smps[0] = (void*)linearClampSampler.Value.NativePointer;
            smps[1] = (void*)linearWrapSampler.Value.NativePointer;
            smps[2] = (void*)pointClampSampler.Value.NativePointer;

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

            var probeParamsBuffer = this.probeParamsBuffer.Value;
            var probeParams = probeParamsBuffer.Local;
            probeParams->GlobalProbes = globalProbes.Count;
            probeParamsBuffer.Update(context);
            cbs[0] = (void*)probeParamsBuffer.Buffer?.NativePointer;

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
    }
}