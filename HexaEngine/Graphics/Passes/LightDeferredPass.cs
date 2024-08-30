#nullable disable

namespace HexaEngine.Graphics.Passes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Types;
    using HexaEngine.Meshes;
    using HexaEngine.Profiling;
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
        private ResourceRef<ISamplerState> anisotropicClampSampler;
        private unsafe void** cbs;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;
        private ResourceRef<ConstantBuffer<CBWeather>> weather;
        private const uint nConstantBuffers = 3;

        private ResourceRef<IGraphicsPipelineState> deferred;

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

            linearClampSampler = creator.CreateSamplerState("LinearClamp", SamplerStateDescription.LinearClamp);
            linearWrapSampler = creator.CreateSamplerState("LinearWrap", SamplerStateDescription.LinearWrap);
            pointClampSampler = creator.CreateSamplerState("PointClamp", SamplerStateDescription.PointClamp);
            anisotropicClampSampler = creator.CreateSamplerState("AnisotropicClamp", SamplerStateDescription.AnisotropicClamp);

            cbs = AllocArrayAndZero(nConstantBuffers);
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");
            weather = creator.GetConstantBuffer<CBWeather>("CBWeather");

            var stateDesc = GraphicsPipelineStateDesc.DefaultAdditiveFullscreen;
            stateDesc.Flags = GraphicsPipelineStateFlags.CreateResourceBindingList;
            deferred = creator.CreateGraphicsPipelineState(new()
            {
                Pipeline = new()
                {
                    VertexShader = "quad.hlsl",
                    PixelShader = "deferred/brdf/light.hlsl",
                },
                State = stateDesc
            });

            LightManager.ActiveLightsChanged += ActiveLightsChanged;
        }

        private unsafe void ActiveLightsChanged(object sender, LightManager manager)
        {
            var bindings = deferred.Value.Bindings;

            bindings.SetSRV("globalProbes", manager.GlobalProbes.SRV);
            bindings.SetSRV("lights", manager.LightBuffer.SRV);
            bindings.SetSRV("shadowData", manager.ShadowDataBuffer.SRV);

            IShaderResourceView csmBuffer = null;
            for (int i = 0; i < manager.ActiveCount; i++)
            {
                var light = manager.Active[i];
                if (light is DirectionalLight directional && directional.ShadowMapEnable)
                {
                    csmBuffer = directional.GetShadowMap();
                    break;
                }
            }
            bindings.SetSRV("depthCSM", csmBuffer);

            IShaderResourceView globalDiffuseIbl = null;
            IShaderResourceView globalSpecularIbl = null;
            for (int i = 0; i < manager.ActiveCount; i++)
            {
                var light = manager.Active[i];
                if (light is IBLLight iBLLight)
                {
                    globalDiffuseIbl = iBLLight.DiffuseMap?.SRV; // IBL global diffuse
                    globalSpecularIbl = iBLLight.SpecularMap?.SRV; // IBL global specular
                    break;
                }
            }
            bindings.SetSRV("globalDiffuse", globalDiffuseIbl);
            bindings.SetSRV("globalSpecular", globalSpecularIbl);
        }

        public override unsafe void Prepare(GraphResourceBuilder creator)
        {
            var bindings = deferred.Value.Bindings;
            bindings.SetSampler("linearClampSampler", linearClampSampler.Value);
            bindings.SetSampler("linearWrapSampler", linearWrapSampler.Value);
            bindings.SetSampler("pointClampSampler", pointClampSampler.Value);
            bindings.SetSampler("ansiotropicClampSampler", anisotropicClampSampler.Value);

            bindings.SetCBV("CameraBuffer", camera.Value);
            bindings.SetCBV("WeatherBuffer", weather.Value);

            bindings.SetSRV("ssao", AOBuffer.Value.SRV);
            bindings.SetSRV("iblDFG", brdfLUT.Value.SRV);

            bindings.SetSRV("lightIndexList", lightIndexList.Value.SRV);
            bindings.SetSRV("lightGrid", lightGridBuffer.Value.SRV);
            bindings.SetSRV("depthAtlas", shadowAtlas.Value.SRV);

            GBuffer gbuffer = this.gbuffer.Value;
            bindings.SetSRV("GBufferA", gbuffer.SRVs[0]);
            bindings.SetSRV("GBufferB", gbuffer.SRVs[1]);
            bindings.SetSRV("GBufferC", gbuffer.SRVs[2]);
            bindings.SetSRV("GBufferD", gbuffer.SRVs[3]);

            bindings.SetSRV("Depth", depthStencil.Value.SRV);

            cbs[1] = (void*)camera.Value.NativePointer;
            cbs[2] = (void*)weather.Value.NativePointer;
        }

        public override unsafe void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler profiler)
        {
            if (forceForward)
            {
                return;
            }

            var current = SceneManager.Current;
            if (current == null)
            {
                return;
            }

            var renderers = current.RenderManager;

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

            context.Device.Profiler.Begin(context, "Light.Deferred");
            context.SetRenderTarget(lightBuffer.Value.RTV, null);

            context.SetGraphicsPipelineState(deferred.Value);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);

            context.Device.Profiler.End(context, "Light.Deferred");
        }
    }
}