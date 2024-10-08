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
        private ResourceRef<ISamplerState> linearMirrorSampler;
        private ResourceRef<ISamplerState> linearClampSampler;
        private ResourceRef<ISamplerState> linearWrapSampler;
        private ResourceRef<ISamplerState> linearBorderSampler;
        private ResourceRef<ISamplerState> pointClampSampler;
        private ResourceRef<ISamplerState> pointWrapSampler;
        private ResourceRef<ISamplerState> anisotropicClampSampler;
        private ResourceRef<ISamplerState> anisotropicWrapSampler;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;
        private ResourceRef<ConstantBuffer<CBWeather>> weather;

        private ResourceRef<IGraphicsPipelineState> deferred;
        private IGraphicsDevice dev;

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

            linearMirrorSampler = creator.CreateSamplerState("LinearMirror", SamplerStateDescription.LinearMirror);
            linearClampSampler = creator.CreateSamplerState("LinearClamp", SamplerStateDescription.LinearClamp);
            linearWrapSampler = creator.CreateSamplerState("LinearWrap", SamplerStateDescription.LinearWrap);
            linearBorderSampler = creator.CreateSamplerState("LinearBorder", SamplerStateDescription.LinearBorder);
            pointClampSampler = creator.CreateSamplerState("PointClamp", SamplerStateDescription.PointClamp);
            pointWrapSampler = creator.CreateSamplerState("PointWrap", SamplerStateDescription.PointWrap);
            anisotropicClampSampler = creator.CreateSamplerState("AnisotropicClamp", SamplerStateDescription.AnisotropicClamp);
            anisotropicWrapSampler = creator.CreateSamplerState("AnisotropicWrap", SamplerStateDescription.AnisotropicWrap);

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
            LightManager.LightShadowMapChanged += LightShadowMapChanged;
            if (LightManager.Current != null)
            {
                ActiveLightsChanged(LightManager.Current, new(LightManager.Current));
            }
        }

        private void LightShadowMapChanged(object sender, LightShadowMapChangedEventArgs e)
        {
            if (e.Light is DirectionalLight)
            {
                dev.SetGlobalSRV("depthCSM", e.View);
            }
        }

        private unsafe void ActiveLightsChanged(object sender, ActiveLightsChangedEventArgs e)
        {
            var manager = e.LightManager;
            dev.SetGlobalSRV("globalProbes", manager.GlobalProbes.SRV);
            dev.SetGlobalSRV("lights", manager.LightBuffer.SRV);
            dev.SetGlobalSRV("shadowData", manager.ShadowDataBuffer.SRV);

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
            dev.SetGlobalSRV("depthCSM", csmBuffer);

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
            dev.SetGlobalSRV("globalDiffuse", globalDiffuseIbl);
            dev.SetGlobalSRV("globalSpecular", globalSpecularIbl);
        }

        public override void Prepare(GraphResourceBuilder creator)
        {
            dev = creator.Device;

            dev.SetGlobalCBV("CameraBuffer", camera.Value);
            dev.SetGlobalCBV("WeatherBuffer", weather.Value);
            dev.SetGlobalCBV("ForwardLightParams", lightParamsBuffer.Value);

            dev.SetGlobalSampler("pointClampSampler", pointClampSampler.Value);
            dev.SetGlobalSampler("pointWrapSampler", pointWrapSampler.Value);
            dev.SetGlobalSampler("linearClampSampler", linearClampSampler.Value);
            dev.SetGlobalSampler("linearWrapSampler", linearWrapSampler.Value);
            dev.SetGlobalSampler("linearBorderSampler", linearBorderSampler.Value);
            dev.SetGlobalSampler("linearMirrorSampler", linearMirrorSampler.Value);
            dev.SetGlobalSampler("ansiotropicClampSampler", anisotropicClampSampler.Value);
            dev.SetGlobalSampler("anisotropicWrapSampler", anisotropicWrapSampler.Value);

            dev.SetGlobalSRV("ssao", AOBuffer.Value.SRV);
            dev.SetGlobalSRV("iblDFG", brdfLUT.Value.SRV);

            dev.SetGlobalSRV("lightIndexList", lightIndexList.Value.SRV);
            dev.SetGlobalSRV("lightGrid", lightGridBuffer.Value.SRV);
            dev.SetGlobalSRV("depthAtlas", shadowAtlas.Value.SRV);
            dev.SetGlobalSRV("shadowAtlas", shadowAtlas.Value.SRV);

            GBuffer gbuffer = this.gbuffer.Value;
            dev.SetGlobalSRV("GBufferA", gbuffer.SRVs[0]);
            dev.SetGlobalSRV("GBufferB", gbuffer.SRVs[1]);
            dev.SetGlobalSRV("normalTex", gbuffer.SRVs[1]);
            dev.SetGlobalSRV("GBufferC", gbuffer.SRVs[2]);
            dev.SetGlobalSRV("GBufferD", gbuffer.SRVs[3]);

            dev.SetGlobalSRV("Depth", depthStencil.Value.SRV);
            dev.SetGlobalSRV("depthTex", depthStencil.Value.SRV);
        }

        public override void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler profiler)
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

            profiler?.Begin("LightForward.Background");
            RenderManager.ExecuteGroup(renderers.BackgroundQueue, context, profiler, "LightForward", RenderPath.Forward);
            profiler?.End("LightForward.Background");

            context.Device.Profiler.Begin(context, "Light.Deferred");
            context.SetRenderTarget(lightBuffer.Value.RTV, null);

            context.SetGraphicsPipelineState(deferred.Value);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);

            context.Device.Profiler.End(context, "Light.Deferred");
        }
    }
}