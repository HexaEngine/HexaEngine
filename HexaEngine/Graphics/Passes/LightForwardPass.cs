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
    using HexaEngine.Scenes.Managers;

    public class LightForwardPass : RenderPass
    {
        private ResourceRef<DepthStencil> depthStencil;
        private ResourceRef<GBuffer> gbuffer;
        private ResourceRef<Texture2D> AOBuffer;
        private ResourceRef<Texture2D> brdfLUT;
        private ResourceRef<StructuredUavBuffer<uint>> lightIndexList;
        private ResourceRef<StructuredUavBuffer<LightGrid>> lightGridBuffer;
        private ResourceRef<ShadowAtlas> shadowAtlas;
        private ResourceRef<Texture2D> lightBuffer;

        private ResourceRef<ISamplerState> linearClampSampler;
        private ResourceRef<ISamplerState> linearWrapSampler;
        private ResourceRef<ISamplerState> pointClampSampler;
        private ResourceRef<ISamplerState> anisotropicClampSampler;

        private ResourceRef<ConstantBuffer<ForwardLightParams>> lightParamsBuffer;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;
        private ResourceRef<ConstantBuffer<CBWeather>> weather;

        private unsafe void** forwardRTVs;
        private const uint nForwardRTVs = 3;

        public LightForwardPass(Windows.RendererFlags flags) : base("LightForward")
        {
            forceForward = (flags & Windows.RendererFlags.ForceForward) != 0;
            AddWriteDependency(new("LightBuffer"));
            AddWriteDependency(new("GBuffer"));
            AddReadDependency(new("#AOBuffer"));
            AddReadDependency(new("ShadowAtlas"));
        }

        private readonly bool forceForward = false;

        public override unsafe void Init(GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            depthStencil = creator.GetDepthStencilBuffer("#DepthStencil");
            gbuffer = creator.GetGBuffer("GBuffer");
            AOBuffer = creator.GetTexture2D("#AOBuffer");

            brdfLUT = creator.GetTexture2D("BRDFLUT");

            lightIndexList = creator.GetStructuredUavBuffer<uint>("LightIndexList");
            lightGridBuffer = creator.GetStructuredUavBuffer<LightGrid>("LightGridBuffer");

            shadowAtlas = creator.GetShadowAtlas("ShadowAtlas");

            var viewport = creator.Viewport;
            lightBuffer = creator.CreateTexture2D("LightBuffer", new(Format.R16G16B16A16Float, (int)viewport.Width, (int)viewport.Height, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget), ResourceCreationFlags.LazyInit);

            linearClampSampler = creator.CreateSamplerState("PointClamp", SamplerStateDescription.LinearClamp);
            linearWrapSampler = creator.CreateSamplerState("LinearWrap", SamplerStateDescription.LinearWrap);
            pointClampSampler = creator.CreateSamplerState("PointClamp", SamplerStateDescription.PointClamp);
            anisotropicClampSampler = creator.CreateSamplerState("AnisotropicClamp", SamplerStateDescription.AnisotropicClamp);

            lightParamsBuffer = creator.CreateConstantBuffer<ForwardLightParams>("ForwardLightParams", CpuAccessFlags.Write);
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");
            weather = creator.GetConstantBuffer<CBWeather>("CBWeather");

            forwardRTVs = AllocArrayAndZero(nForwardRTVs);
        }

        public override void Prepare(GraphResourceBuilder creator)
        {
            creator.Device.SetGlobalSampler("linearClampSampler", linearClampSampler.Value);
            creator.Device.SetGlobalSampler("linearWrapSampler", linearWrapSampler.Value);
            creator.Device.SetGlobalSampler("pointClampSampler", pointClampSampler.Value);
            creator.Device.SetGlobalSampler("anisotropicClampSampler", anisotropicClampSampler.Value);

            creator.Device.SetGlobalCBV("CameraBuffer", camera.Value);
            creator.Device.SetGlobalCBV("WeatherBuffer", weather.Value);
            creator.Device.SetGlobalCBV("ForwardLightParams", lightParamsBuffer.Value);
        }

        public override unsafe void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            var current = SceneManager.Current;
            if (current == null)
            {
                return;
            }

            profiler?.Begin("LightForward.Update");

            var renderers = current.RenderManager;
            var lights = current.LightManager;
            var globalProbes = lights.GlobalProbes;

            var gbuffer = this.gbuffer.Value;

            forwardRTVs[0] = (void*)lightBuffer.Value!.RTV!.NativePointer;
            forwardRTVs[1] = gbuffer.PRTVs[1];
            forwardRTVs[2] = gbuffer.PRTVs[2];

            context.SetRenderTargets(nForwardRTVs, forwardRTVs, depthStencil.Value!.DSV);

            profiler?.End("LightForward.Update");

            profiler?.Begin("LightForward.Begin");

            var cam = CameraManager.Current;
            var lightParamsBuffer = this.lightParamsBuffer.Value;
            var lightParams = lightParamsBuffer.Local;
            lightParams->LightCount = lights.LightBuffer.Count;
            lightParams->GlobalProbes = lights.GlobalProbes.Count;
            lightParamsBuffer.Update(context);

            context.SetViewport(creator.Viewport);

            profiler?.End("LightForward.Begin");

            if (forceForward)
            {
                profiler?.Begin("LightForward.Background");
                RenderManager.ExecuteGroup(renderers.BackgroundQueue, context, profiler, "LightForward", RenderPath.Forward);
                profiler?.End("LightForward.Background");

                profiler?.Begin("LightForward.Geometry");
                RenderManager.ExecuteGroup(renderers.BackgroundQueue, context, profiler, "LightForward", RenderPath.Forward);
                profiler?.End("LightForward.Geometry");

                profiler?.Begin("LightForward.AlphaTest");
                RenderManager.ExecuteGroup(renderers.AlphaTestQueue, context, profiler, "LightForward", RenderPath.Forward);
                profiler?.End("LightForward.AlphaTest");
            }

            profiler?.Begin("LightForward.GeometryLast");
            RenderManager.ExecuteGroup(renderers.GeometryLastQueue, context, profiler, "LightForward", RenderPath.Forward);
            profiler?.End("LightForward.GeometryLast");

            profiler?.Begin("LightForward.Transparency");
            RenderManager.ExecuteGroup(renderers.TransparencyQueue, context, profiler, "LightForward", RenderPath.Forward, forwardRTVs, nForwardRTVs, depthStencil.Value.DSV);
            profiler?.End("LightForward.Transparency");

            profiler?.Begin("LightForward.End");

            void* null_rtvs = stackalloc nint[(int)nForwardRTVs];
            context.SetRenderTargets(nForwardRTVs, (void**)null_rtvs, null);

            profiler?.End("LightForward.End");
        }
    }
}