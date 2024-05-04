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
    using HexaEngine.Scenes.Managers;
    using System.Numerics;

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

        private unsafe void** cbs;

        private const uint nConstantBuffers = 3;
        private unsafe void** smps;
        private const uint nSamplers = 4;

        private unsafe void** forwardRTVs;
        private const uint nForwardRTVs = 3;

        private unsafe void** forwardSRVs;
        private const uint nForwardSRVs = 8 + 11;

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

            smps = AllocArrayAndZero(nSamplers);
            linearClampSampler = creator.CreateSamplerState("PointClamp", SamplerStateDescription.LinearClamp);
            linearWrapSampler = creator.CreateSamplerState("LinearWrap", SamplerStateDescription.LinearWrap);
            pointClampSampler = creator.CreateSamplerState("PointClamp", SamplerStateDescription.PointClamp);
            anisotropicClampSampler = creator.CreateSamplerState("AnisotropicClamp", SamplerStateDescription.AnisotropicClamp);

            cbs = AllocArrayAndZero(nConstantBuffers);
            lightParamsBuffer = creator.CreateConstantBuffer<ForwardLightParams>("ForwardLightParams", CpuAccessFlags.Write);
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");
            weather = creator.GetConstantBuffer<CBWeather>("CBWeather");

            forwardSRVs = AllocArrayAndZero(nForwardSRVs);

            forwardRTVs = AllocArrayAndZero(nForwardRTVs);
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

            smps[0] = (void*)linearClampSampler.Value.NativePointer;
            smps[1] = (void*)linearWrapSampler.Value.NativePointer;
            smps[2] = (void*)pointClampSampler.Value.NativePointer;
            smps[3] = (void*)anisotropicClampSampler.Value.NativePointer;

            forwardSRVs[0] = (void*)AOBuffer.Value.SRV.NativePointer;
            forwardSRVs[1] = (void*)brdfLUT.Value.SRV.NativePointer;

            forwardSRVs[3] = (void*)lights.LightBuffer.SRV.NativePointer;
            forwardSRVs[4] = (void*)lights.ShadowDataBuffer.SRV.NativePointer;
            forwardSRVs[5] = (void*)lightIndexList.Value.SRV.NativePointer;
            forwardSRVs[6] = (void*)lightGridBuffer.Value.SRV.NativePointer;
            forwardSRVs[7] = (void*)shadowAtlas.Value.SRV.NativePointer;
            forwardSRVs[8] = null;
            for (int i = 0; i < lights.ActiveCount; i++)
            {
                var light = lights.Active[i];
                if (light is DirectionalLight directional && directional.ShadowMapEnable)
                {
                    forwardSRVs[8] = (void*)(directional.GetShadowMap()?.NativePointer ?? 0);
                    break;
                }
            }

            forwardSRVs[9] = null; // IBL global diffuse
            forwardSRVs[10] = null; // IBL global specular
            for (int i = 0; i < lights.ActiveCount; i++)
            {
                var light = lights.Active[i];
                if (light is IBLLight iBLLight)
                {
                    forwardSRVs[9] = (void*)(iBLLight.DiffuseMap?.SRV.NativePointer ?? 0); // IBL global diffuse
                    forwardSRVs[10] = (void*)(iBLLight.SpecularMap?.SRV.NativePointer ?? 0); // IBL global specular
                    break;
                }
            }

            forwardRTVs[0] = (void*)lightBuffer.Value.RTV.NativePointer;
            forwardRTVs[1] = gbuffer.PRTVs[1];
            forwardRTVs[2] = gbuffer.PRTVs[2];

            context.SetRenderTargets(nForwardRTVs, forwardRTVs, depthStencil.Value.DSV);

            profiler?.End("LightForward.Update");

            profiler?.Begin("LightForward.Begin");

            var cam = CameraManager.Current;
            var lightParamsBuffer = this.lightParamsBuffer.Value;
            var lightParams = lightParamsBuffer.Local;
            lightParams->LightCount = lights.LightBuffer.Count;
            lightParams->GlobalProbes = lights.GlobalProbes.Count;
            lightParamsBuffer.Update(context);
            cbs[0] = (void*)lightParamsBuffer.Buffer?.NativePointer;
            cbs[1] = (void*)camera.Value.NativePointer;
            cbs[2] = (void*)weather.Value.NativePointer;

            context.SetViewport(creator.Viewport);
            context.VSSetConstantBuffers(1, 1, &cbs[1]);
            context.DSSetConstantBuffers(1, 1, &cbs[1]);
            context.CSSetConstantBuffers(1, 1, &cbs[1]);
            context.PSSetConstantBuffers(0, nConstantBuffers, cbs);
            context.PSSetShaderResources(0, nForwardSRVs, forwardSRVs);
            context.PSSetSamplers(0, nSamplers, smps);

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

            nint* null_samplers = stackalloc nint[(int)nSamplers];
            context.PSSetSamplers(0, nSamplers, (void**)null_samplers);

            nint* null_srvs = stackalloc nint[(int)nForwardSRVs];
            context.PSSetShaderResources(0, nForwardSRVs, (void**)null_srvs);

            nint* null_cbs = stackalloc nint[(int)nConstantBuffers];
            context.PSSetConstantBuffers(0, nConstantBuffers, (void**)null_cbs);

            void* null_rtvs = stackalloc nint[(int)nForwardRTVs];
            context.SetRenderTargets(nForwardRTVs, (void**)null_rtvs, null);

            profiler?.End("LightForward.End");
        }
    }
}