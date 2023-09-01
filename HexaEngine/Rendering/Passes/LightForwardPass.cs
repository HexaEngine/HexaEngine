namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graph;
    using HexaEngine.Lights;
    using HexaEngine.Meshes;
    using HexaEngine.Rendering;
    using HexaEngine.Rendering.Graph;
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
        private ResourceRef<ISamplerState> shadowSampler;

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
        private const uint nForwardSRVs = 8 + 9;
        private const uint nForwardIndirectSRVsBase = 8 + 7;

        private unsafe void** forwardClusteredSRVs;
        private const uint nForwardClusteredSRVs = 8 + 11;
        private const uint nForwardClusteredIndirectSRVsBase = 8 + 9;

        public LightForwardPass() : base("LightForward")
        {
            AddWriteDependency(new("LightBuffer"));
            AddWriteDependency(new("GBuffer"));
            AddReadDependency(new("#AOBuffer"));
            AddReadDependency(new("ShadowAtlas"));
        }

        private readonly bool forceForward = true;
        private readonly bool clustered = true;

        public override unsafe void Init(GraphResourceBuilder creator, GraphPipelineBuilder pipelineCreator, IGraphicsDevice device, ICPUProfiler? profiler)
        {
            depthStencil = creator.GetDepthStencilBuffer("#DepthStencil");
            gbuffer = creator.GetGBuffer("GBuffer");
            AOBuffer = creator.GetTexture2D("#AOBuffer");

            brdfLUT = creator.GetTexture2D("BRDFLUT");

            lightIndexList = creator.GetStructuredUavBuffer<uint>("LightIndexList");
            lightGridBuffer = creator.GetStructuredUavBuffer<LightGrid>("LightGridBuffer");

            shadowAtlas = creator.GetShadowAtlas("ShadowAtlas");

            var viewport = creator.Viewport;
            lightBuffer = creator.CreateTexture2D("LightBuffer", new(Format.R16G16B16A16Float, (int)viewport.Width, (int)viewport.Height, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget));

            smps = AllocArrayAndZero(nSamplers);
            linearClampSampler = creator.CreateSamplerState("PointClamp", SamplerStateDescription.LinearClamp);
            linearWrapSampler = creator.CreateSamplerState("LinearWrap", SamplerStateDescription.LinearWrap);
            pointClampSampler = creator.CreateSamplerState("PointClamp", SamplerStateDescription.PointClamp);
            shadowSampler = creator.CreateSamplerState("LinearComparisonBorder", SamplerStateDescription.ComparisonLinearBorder);

            cbs = AllocArrayAndZero(nConstantBuffers);
            lightParamsBuffer = creator.CreateConstantBuffer<ForwardLightParams>("ForwardLightParams", CpuAccessFlags.Write);
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");
            weather = creator.GetConstantBuffer<CBWeather>("CBWeather");

            forwardSRVs = AllocArrayAndZero(nForwardSRVs);
            forwardClusteredSRVs = AllocArrayAndZero(nForwardClusteredSRVs);

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
            smps[3] = (void*)shadowSampler.Value.NativePointer;

            forwardSRVs[8] = forwardClusteredSRVs[8] = (void*)AOBuffer.Value.SRV.NativePointer;

            forwardSRVs[9] = forwardClusteredSRVs[9] = (void*)brdfLUT.Value.SRV.NativePointer;
            forwardSRVs[10] = (void*)globalProbes.SRV.NativePointer;

            forwardSRVs[11] = forwardClusteredSRVs[11] = (void*)lights.LightBuffer.SRV.NativePointer;
            forwardSRVs[12] = forwardClusteredSRVs[12] = (void*)lights.ShadowDataBuffer.SRV.NativePointer;

            forwardClusteredSRVs[13] = (void*)lightIndexList.Value.SRV.NativePointer;
            forwardClusteredSRVs[14] = (void*)lightGridBuffer.Value.SRV.NativePointer;

            forwardSRVs[13] = forwardClusteredSRVs[15] = (void*)shadowAtlas.Value.SRV.NativePointer;

            forwardRTVs[0] = (void*)lightBuffer.Value.RTV.NativePointer;
            forwardRTVs[1] = gbuffer.PRTVs[1];
            forwardRTVs[2] = gbuffer.PRTVs[2];

            context.ClearRenderTargetViews(1, &forwardRTVs[0], default);

            context.SetRenderTargets(nForwardRTVs, forwardRTVs, depthStencil.Value.DSV);

            profiler?.End("LightForward.Update");

            profiler?.Begin("LightForward.Begin");

            if (clustered)
            {
                ClusteredForwardBegin(context, creator, lights);
            }
            else
            {
                ForwardBegin(context, creator, lights);
            }

            profiler?.End("LightForward.Begin");

            if (forceForward)
            {
                profiler?.Begin("LightForward.Background");
                var background = renderers.BackgroundQueue;
                for (int i = 0; i < background.Count; i++)
                {
                    var renderer = background[i];
                    profiler?.Begin($"LightForward.{renderer.DebugName}");
                    renderer.Draw(context, RenderPath.Forward);
                    profiler?.End($"LightForward.{renderer.DebugName}");
                }
                profiler?.End("LightForward.Background");

                profiler?.Begin("LightForward.Geometry");
                var geometry = renderers.GeometryQueue;
                for (int i = 0; i < geometry.Count; i++)
                {
                    var renderer = geometry[i];
                    profiler?.Begin($"LightForward.{renderer.DebugName}");
                    renderer.Draw(context, RenderPath.Forward);
                    profiler?.End($"LightForward.{renderer.DebugName}");
                }
                profiler?.End("LightForward.Geometry");
            }

            profiler?.Begin("LightForward.AlphaTest");
            var alphaTest = renderers.AlphaTestQueue;
            for (int i = 0; i < alphaTest.Count; i++)
            {
                var renderer = alphaTest[i];
                profiler?.Begin($"LightForward.{renderer.DebugName}");
                renderer.Draw(context, RenderPath.Forward);
                profiler?.End($"LightForward.{renderer.DebugName}");
            }
            profiler?.End("LightForward.AlphaTest");

            profiler?.Begin("LightForward.GeometryLast");
            var geometryLast = renderers.TransparencyQueue;
            for (int i = 0; i < geometryLast.Count; i++)
            {
                var renderer = geometryLast[i];
                profiler?.Begin($"LightForward.{renderer.DebugName}");
                renderer.Draw(context, RenderPath.Forward);
                profiler?.End($"LightForward.{renderer.DebugName}");
            }
            profiler?.End("LightForward.GeometryLast");

            profiler?.Begin("LightForward.Transparency");
            var transparency = renderers.TransparencyQueue;
            for (int i = 0; i < transparency.Count; i++)
            {
                var renderer = transparency[i];
                profiler?.Begin($"LightForward.{renderer.DebugName}");
                renderer.Draw(context, RenderPath.Forward);
                profiler?.End($"LightForward.{renderer.DebugName}");
            }
            profiler?.End("LightForward.Transparency");

            profiler?.Begin("LightForward.End");

            if (clustered)
            {
                ClusteredForwardEnd(context);
            }
            else
            {
                ForwardEnd(context);
            }

            void* null_rtvs = stackalloc nint[(int)nForwardRTVs];
            context.SetRenderTargets(nForwardRTVs, (void**)null_rtvs, null);

            profiler?.End("LightForward.End");
        }

        private unsafe void ForwardBegin(IGraphicsContext context, GraphResourceBuilder creator, LightManager lights)
        {
            var lightParamsBuffer = this.lightParamsBuffer.Value;
            var lightParams = lightParamsBuffer.Local;
            lightParams->LightCount = lights.LightBuffer.Count;
            lightParams->GlobalProbes = lights.GlobalProbes.Count;
            lightParamsBuffer.Update(context);
            cbs[0] = (void*)lightParamsBuffer.NativePointer;
            cbs[1] = (void*)camera.Value.NativePointer;
            cbs[2] = (void*)weather.Value.NativePointer;

            context.SetViewport(creator.Viewport);
            context.VSSetConstantBuffers(1, 1, &cbs[1]);
            context.DSSetConstantBuffers(1, 1, &cbs[1]);
            context.GSSetConstantBuffers(1, 1, &cbs[1]);
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

        private unsafe void ClusteredForwardBegin(IGraphicsContext context, GraphResourceBuilder creator, LightManager lights)
        {
            var cam = CameraManager.Current;
            var lightParamsBuffer = this.lightParamsBuffer.Value;
            var lightParams = lightParamsBuffer.Local;
            lightParams->LightCount = lights.LightBuffer.Count;
            lightParams->GlobalProbes = lights.GlobalProbes.Count;
            lightParamsBuffer.Update(context);
            cbs[0] = (void*)lightParamsBuffer.Buffer?.NativePointer;
            cbs[1] = (void*)camera.Value.NativePointer;
            cbs[2] = null;

            context.SetViewport(creator.Viewport);
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