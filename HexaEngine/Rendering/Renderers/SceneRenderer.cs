#nullable disable

namespace HexaEngine.Rendering.Renderers
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Culling;
    using HexaEngine.Editor;
    using HexaEngine.Editor.Widgets;
    using HexaEngine.Effects.BuildIn;
    using HexaEngine.Filters;
    using HexaEngine.Lights;
    using HexaEngine.Meshes;
    using HexaEngine.PostFx;
    using HexaEngine.Rendering;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Rendering.Passes;
    using HexaEngine.Scenes;
    using ImGuiNET;
    using System;
    using System.Numerics;

    public class ClearMainDepthStencilPass : ClearDepthStencilPass
    {
        public ClearMainDepthStencilPass() : base("ClearMainDepthStencil")
        {
            AddWriteDependency(new("$DepthStencil"));
        }
    }

    public class DepthPrePass : DepthStencilPass
    {
        public DepthPrePass() : base("PrePass")
        {
            AddWriteDependency(new("$DepthStencil"));
        }
    }

    public class PostProcessPrePass : DrawPass
    {
        public PostProcessPrePass() : base("PostProcessPrePass")
        {
            AddReadDependency(new("$DepthStencil"));
        }
    }

    public class HizDepthPass : Graph.ComputePass
    {
        public HizDepthPass() : base("HizDepth")
        {
            AddReadDependency(new("$DepthStencil"));
        }
    }

    public class ObjectCullPass : Graph.ComputePass
    {
        public ObjectCullPass() : base("ObjectCull")
        {
            AddReadDependency(new("$DepthStencil"));
        }
    }

    public class LightCullPass : Graph.ComputePass
    {
        public LightCullPass() : base("LightCull")
        {
            AddReadDependency(new("$DepthStencil"));
        }
    }

    public class ShadowMapPass : DrawPass
    {
        public ShadowMapPass() : base("ShadowMap")
        {
            AddWriteDependency(new("$ShadowAtlas"));
        }
    }

    public class ClearGBufferPass : ClearMultiRenderTargetPass
    {
        public ClearGBufferPass() : base("ClearGBuffer")
        {
            AddWriteDependency(new("$GBuffer"));
        }
    }

    public class GBufferPass : MultiTargetDrawPass
    {
        public GBufferPass() : base("GBuffer")
        {
            AddWriteDependency(new("$DepthStencil"));
            AddWriteDependency(new("$GBuffer"));
        }
    }

    public class ClearLightBufferPass : ClearRenderTargetPass
    {
        public ClearLightBufferPass() : base("ClearLightBuffer")
        {
            AddWriteDependency(new("$LightBuffer"));
        }
    }

    public class LightDeferredPass : DrawPass
    {
        public LightDeferredPass() : base("LightDeferred")
        {
            AddWriteDependency(new("$LightBuffer"));
            AddReadDependency(new("$GBuffer"));
            AddReadDependency(new("$AOBuffer"));
            AddReadDependency(new("$ShadowAtlas"));
        }
    }

    public class LightForwardPass : MultiTargetDrawPass
    {
        public LightForwardPass() : base("Lightning")
        {
            AddWriteDependency(new("$LightBuffer"));
            AddWriteDependency(new("$GBuffer"));
            AddReadDependency(new("$AOBuffer"));
            AddReadDependency(new("$ShadowAtlas"));
        }
    }

    public class HBAOPass : DrawPass
    {
        public HBAOPass() : base("HABO")
        {
            AddReadDependency(new("$GBuffer"));
            AddWriteDependency(new("$AOBuffer"));
        }
    }

    public class PostProcessPass : DrawPass
    {
        public PostProcessPass() : base("PostProcess")
        {
            AddReadDependency(new("$GBuffer"));
            AddReadDependency(new("$LightBuffer"));
            AddReadDependency(new("$DepthStencil"));
        }
    }

    public class HDRPipeline : Graph.RenderGraph
    {
        public HDRPipeline() : base("HDRPipeline")
        {
            ClearMainDepthStencilPass clearMainDepthStencilPass = new();
            DepthPrePass depthPrePass = new();
            PostProcessPrePass postProcessPrePass = new();
            HizDepthPass hizDepthPass = new();
            ObjectCullPass objectCullPass = new();
            LightCullPass lightCullPass = new();
            ClearGBufferPass clearGBufferPass = new();
            GBufferPass gBufferPass = new();
            ClearLightBufferPass clearLightBufferPass = new();
            LightForwardPass lightForwardPass = new();
            HBAOPass aoPass = new();
            VelocityBufferPass velocityBufferPass = new();
            VolumetricLightsPass volumetricLightsPass = new();
            BloomPass bloomPass = new();
            AutoExposurePass autoExposurePass = new();
            DepthOfFieldPass depthOfFieldPass = new();
            MotionBlurPass motionBlurPass = new();
            GodRaysPass godRaysPass = new();
            LensFlarePass lensFlarePass = new();
            VolumetricCloudsPass volumetricCloudsPass = new();
            TAAPass taaPass = new();
            ComposePass composePass = new();

            /*
            AddPass(clearMainDepthStencilPass);
            AddPass(depthPrePass);
            AddPass(postProcessPrePass);
            AddPass(hizDepthPass);
            AddPass(objectCullPass);
            AddPass(lightCullPass);
            AddPass(clearGBufferPass);
            AddPass(gBufferPass);
            AddPass(clearLightBufferPass);
            AddPass(lightDeferredPass);
            AddPass(lightForwardPass);
            AddPass(aoPass);
            AddPass(velocityBufferPass);
            AddPass(volumetricLightsPass);
            AddPass(bloomPass);
            AddPass(autoExposurePass);
            AddPass(depthOfFieldPass);
            AddPass(motionBlurPass);
            AddPass(godRaysPass);
            AddPass(lensFlarePass);
            AddPass(volumetricCloudsPass);
            AddPass(taaPass);
            AddPass(composePass);
            */

            depthPrePass.Dependencies.Add(clearMainDepthStencilPass);
            postProcessPrePass.Dependencies.Add(depthPrePass);
            hizDepthPass.Dependencies.Add(depthPrePass);
            objectCullPass.Dependencies.Add(hizDepthPass);
            lightCullPass.Dependencies.Add(depthPrePass);
            gBufferPass.Dependencies.Add(clearGBufferPass);
            gBufferPass.Dependencies.Add(objectCullPass);
            aoPass.Dependencies.Add(gBufferPass);
            aoPass.Dependencies.Add(depthPrePass);
            lightForwardPass.Dependencies.Add(postProcessPrePass);
            lightForwardPass.Dependencies.Add(clearLightBufferPass);
            lightForwardPass.Dependencies.Add(lightCullPass);
            lightForwardPass.Dependencies.Add(objectCullPass);
            lightForwardPass.Dependencies.Add(aoPass);
            velocityBufferPass.Dependencies.Add(lightForwardPass);
            volumetricLightsPass.Dependencies.Add(lightForwardPass);
            depthOfFieldPass.Dependencies.Add(lightForwardPass);
            depthOfFieldPass.Dependencies.Add(volumetricLightsPass);

            taaPass.Dependencies.Add(velocityBufferPass);
            taaPass.Dependencies.Add(depthOfFieldPass);
            motionBlurPass.Dependencies.Add(velocityBufferPass);
            motionBlurPass.Dependencies.Add(taaPass);
            bloomPass.Dependencies.Add(motionBlurPass);
            autoExposurePass.Dependencies.Add(bloomPass);

            godRaysPass.Dependencies.Add(autoExposurePass);
            godRaysPass.Dependencies.Add(lightForwardPass);
            lensFlarePass.Dependencies.Add(godRaysPass);
            lensFlarePass.Dependencies.Add(lightForwardPass);

            volumetricCloudsPass.Dependencies.Add(lensFlarePass);
            volumetricCloudsPass.Dependencies.Add(lightForwardPass);

            composePass.Dependencies.Add(volumetricCloudsPass);
        }
    }

    public class SceneRenderer : ISceneRenderer
    {
        private ViewportShading shading = Application.InDesignMode ? ViewportShading.Solid : ViewportShading.Rendered;

        private bool initialized;
        private bool disposedValue;
        private IGraphicsDevice device;
        private IGraphicsContext context;
        private IGraphicsContext deferredContext;
        private PostProcessingManager postProcessing;
        private ISwapChain swapChain;
        private IRenderWindow window;

        private Quad quad;

        private ConstantBuffer<CBCamera> cameraBuffer;
        private ConstantBuffer<CBTessellation> tesselationBuffer;

        private DepthStencil depthStencil;
        private DepthStencil depthStencil2;
        private ResourceRef<Texture2D> lightBuffer;
        private IDepthStencilView dsv;
        private GBuffer gbuffer;

        private DepthMipChain hizBuffer;

        private IAmbientOcclusion ssao;

        private BRDFLUT brdfLUT;

        private Texture2D brdflut;

        private ConfigKey configKey;
        private float renderResolution;
        private int width;
        private int height;
        private int rendererWidth;
        private int rendererHeight;
        private bool windowResized;
        private bool sceneChanged;
        private readonly CPUProfiler profiler = new(10);

        private ICommandList commandList;

        public CPUProfiler Profiler => profiler;

        public PostProcessingManager PostProcessing => postProcessing;

        public ViewportShading Shading { get => shading; set => shading = value; }

        public Graph.RenderGraph RenderGraph { get; } = new HDRPipeline();

        public int Width => width;

        public int Height => height;

        public Vector2 Size => new(width, height);

        public SceneRenderer()
        {
        }

        public static SceneRenderer? Current { get; private set; }

        /// <summary>
        /// Initializes the renderer.
        /// </summary>
        /// <param dbgName="device">The device.</param>
        /// <param dbgName="window">The window.</param>
        /// <returns></returns>
        public Task Initialize(IGraphicsDevice device, ISwapChain swapChain, IRenderWindow window)
        {
            this.device = device;
            context = device.Context;
            this.swapChain = swapChain ?? throw new NotSupportedException("Device needs a swapchain to operate properly");
            this.window = window;
            swapChain.Resizing += OnWindowResizeBegin;
            swapChain.Resized += OnWindowResizeEnd;
            ResourceManager2.Shared.SetOrAddResource("SwapChain.RTV", swapChain.BackbufferRTV);
            ResourceManager2.Shared.SetOrAddResource("SwapChain", swapChain.Backbuffer);
            SceneManager.SceneChanged += SceneChanged;

            configKey = Config.Global.GetOrCreateKey("Renderer");
            renderResolution = configKey.TryGet(nameof(renderResolution), 1f);

            RenderGraph.Build();

            return
            Task.Factory.StartNew(Initialize);
        }

        private async void Initialize()
        {
            InitializeSettings();

            quad = new(device);

            cameraBuffer = ResourceManager2.Shared.SetOrAddConstantBuffer<CBCamera>("CBCamera", CpuAccessFlags.Write).Value;
            tesselationBuffer = new(device, CpuAccessFlags.Write);

            gbuffer = new GBuffer(device, width, height,
                Format.R16G16B16A16Float,   // BaseColor(RGB)   Material ID(A)
                Format.R8G8B8A8UNorm,       // Normal(XYZ)      Roughness(W)
                Format.R8G8B8A8UNorm,       // Metallic         Reflectance             AO      Material Data
                Format.R8G8B8A8UNorm        // Emission(XYZ)    Emission Strength(W)
                );

            depthStencil = new(device, width, height, Format.D32Float);
            depthStencil2 = new(device, width, height, Format.D32Float);
            dsv = depthStencil.DSV;
            hizBuffer = new(device, width, height);

            ResourceManager2.Shared.AddGBuffer("GBuffer", gbuffer);
            ResourceManager2.Shared.AddShaderResourceView("GBuffer.Color", gbuffer.SRVs[0]);
            ResourceManager2.Shared.AddShaderResourceView("GBuffer.Normal", gbuffer.SRVs[1]);
            ResourceManager2.Shared.AddShaderResourceView("GBuffer.Misc", gbuffer.SRVs[2]);
            ResourceManager2.Shared.AddShaderResourceView("GBuffer.Emission", gbuffer.SRVs[3]);
            ResourceManager2.Shared.AddShaderResourceView("GBuffer.Depth", depthStencil.SRV);
            ResourceManager2.Shared.AddShaderResourceView("GBuffer.DepthCopy", depthStencil2.SRV);
            ResourceManager2.Shared.AddShaderResourceView("GBuffer.DepthChain", hizBuffer.SRV);
            ResourceManager2.Shared.AddDepthStencilView("GBuffer.DepthStencil", depthStencil.DSV);
            lightBuffer = ResourceManager2.Shared.AddTexture("LightBuffer", new Texture2DDescription(Format.R16G16B16A16Float, width, height, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget));

            postProcessing = new(device, width, height);

            postProcessing.Add(new VelocityBuffer());
            postProcessing.Add(new VolumetricClouds());
            postProcessing.Add(new SSR());
            postProcessing.Add(new MotionBlur());
            postProcessing.Add(new DepthOfField());
            postProcessing.Add(new AutoExposure());
            postProcessing.Add(new Bloom());
            postProcessing.Add(new LensFlare());
            postProcessing.Add(new GodRays());
            postProcessing.Add(new Compose());
            postProcessing.Add(new TAA());
            postProcessing.Add(new FXAA());

            ssao = new HBAO();

            brdflut = ResourceManager2.Shared.AddTexture("BRDFLUT", new Texture2DDescription(Format.R16G16B16A16Float, 128, 128, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget)).Value;

            window.Dispatcher.InvokeBlocking(() =>
            {
                brdfLUT = new(device, false, true);
                brdfLUT.Target = brdflut.RTV;
                brdfLUT.Draw(context);
                context.ClearState();
                brdfLUT.Dispose();
                brdfLUT = null;
            });

            await postProcessing.InitializeAsync(width, height);
            postProcessing.Enabled = true;

            deferredContext = device.CreateDeferredContext();

            initialized = true;
            Current = this;
            window.Dispatcher.Invoke(() => WidgetManager.Register(new RendererWidget(this)));

            configKey.GenerateSubKeyAuto(ssao, "HBAO");

            await ssao.Initialize(device, width, height);
#if PROFILE
            device.Profiler.CreateBlock("Update");
            device.Profiler.CreateBlock("PrePass");
            device.Profiler.CreateBlock("ObjectCulling");
            device.Profiler.CreateBlock("LightCulling");
            device.Profiler.CreateBlock("ShadowMaps");
            device.Profiler.CreateBlock("Geometry");
            device.Profiler.CreateBlock("LightsDeferred");
            device.Profiler.CreateBlock("LightsForward");
            device.Profiler.CreateBlock("AO");
            device.Profiler.CreateBlock("PostProcessing");
#endif
        }

        private void OnWindowResizeBegin(object sender, EventArgs e)
        {
            postProcessing?.BeginResize();
        }

        private void SceneChanged(object sender, SceneChangedEventArgs e)
        {
            sceneChanged = true;
        }

        private void InitializeSettings()
        {
            {
                configKey.TryGetOrAddKeyValue("Width", "1920", DataType.Int32, false, out var val);
                rendererWidth = val.GetInt32();
                val.ValueChanged += (ss, ee) =>
                {
                    rendererWidth = ss.GetInt32();
                    OnRendererResizeBegin();
                    OnRendererResizeEnd(rendererWidth, rendererHeight);
                    Config.Global.Save();
                };
            }
            {
                configKey.TryGetOrAddKeyValue("Height", "1080", DataType.Int32, false, out var val);
                rendererHeight = val.GetInt32();
                val.ValueChanged += (ss, ee) =>
                {
                    rendererHeight = ss.GetInt32();
                    OnRendererResizeBegin();
                    OnRendererResizeEnd(rendererWidth, rendererHeight);
                    Config.Global.Save();
                };
            }
            {
                configKey.TryGetOrAddKeyValue("VSync", false.ToString(), DataType.Bool, false, out var val);
                swapChain.VSync = val.GetBool();
                val.ValueChanged += (ss, ee) =>
                {
                    swapChain.VSync = ss.GetBool();
                    Config.Global.Save();
                };
            }
            {
                configKey.TryGetOrAddKeyValue("LimitFPS", false.ToString(), DataType.Bool, false, out var val);
                swapChain.LimitFPS = val.GetBool();
                val.ValueChanged += (ss, ee) =>
                {
                    swapChain.LimitFPS = ss.GetBool();
                    Config.Global.Save();
                };
            }
            {
                configKey.TryGetOrAddKeyValue("TargetFPS", 120.ToString(), DataType.Int32, false, out var val);
                swapChain.TargetFPS = val.GetInt32();
                val.ValueChanged += (ss, ee) =>
                {
                    swapChain.TargetFPS = ss.GetInt32();
                    Config.Global.Save();
                };
            }
            {
                configKey.TryGetOrAddKeyValue("Frustum culling", true.ToString(), DataType.Bool, false, out var val);
                if (val.GetBool())
                {
                    CullingManager.CullingFlags |= CullingFlags.Frustum;
                }

                val.ValueChanged += (ss, ee) =>
                {
                    if (val.GetBool())
                    {
                        CullingManager.CullingFlags |= CullingFlags.Frustum;
                    }
                    else
                    {
                        CullingManager.CullingFlags &= ~CullingFlags.Frustum;
                    }

                    Config.Global.Save();
                };
            }
            {
                configKey.TryGetOrAddKeyValue("Occlusion culling", true.ToString(), DataType.Bool, false, out var val);
                if (val.GetBool())
                {
                    CullingManager.CullingFlags |= CullingFlags.Occlusion;
                }

                val.ValueChanged += (ss, ee) =>
                {
                    if (val.GetBool())
                    {
                        CullingManager.CullingFlags |= CullingFlags.Occlusion;
                    }
                    else
                    {
                        CullingManager.CullingFlags &= ~CullingFlags.Occlusion;
                    }

                    Config.Global.Save();
                };
            }

            width = rendererWidth;
            height = rendererHeight;

            Config.Global.Save();
        }

        private void OnWindowResizeEnd(object sender, ResizedEventArgs args)
        {
            windowResized = true;
        }

        private void OnRendererResizeBegin()
        {
            if (!initialized)
            {
                return;
            }

            depthStencil.Dispose();
        }

        private void OnRendererResizeEnd(int width, int height)
        {
            if (!initialized)
            {
                return;
            }

            gbuffer.Resize(width, height);
            depthStencil = new(device, width, height, Format.D32Float);
            depthStencil2 = new(device, width, height, Format.D32Float);
            dsv = depthStencil.DSV;

            lightBuffer = ResourceManager2.Shared.UpdateTexture("LightBuffer", new Texture2DDescription(Format.R16G16B16A16Float, width, height, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget));
            ResourceManager2.Shared.AddShaderResourceView("GBuffer.Color", gbuffer.SRVs[0]);
            ResourceManager2.Shared.AddShaderResourceView("GBuffer.Normal", gbuffer.SRVs[1]);
            ResourceManager2.Shared.AddShaderResourceView("GBuffer.Misc", gbuffer.SRVs[2]);
            ResourceManager2.Shared.AddShaderResourceView("GBuffer.Emission", gbuffer.SRVs[3]);
            ResourceManager2.Shared.SetOrAddResource("GBuffer.Depth", depthStencil.SRV);
            ResourceManager2.Shared.SetOrAddResource("GBuffer.DepthCopy", depthStencil2.SRV);
            ResourceManager2.Shared.SetOrAddResource("GBuffer.DepthStencil", depthStencil.DSV);

            hizBuffer.Resize(device, width, height);

            postProcessing.EndResize(width, height);

            var scene = SceneManager.Current;
            if (scene == null)
            {
                return;
            }

            scene.LightManager.BeginResize();
            scene.LightManager.EndResize(width, height);
        }

        public unsafe void LoadScene(Scene scene)
        {
            scene.LightManager.BeginResize();
            scene.LightManager.EndResize(width, height).Wait();
        }

        private const bool forceForward = true;

        public unsafe void Render(IGraphicsContext context, IRenderWindow window, Mathematics.Viewport viewport, Scene scene, Camera camera)
        {
            if (sceneChanged)
            {
                LoadScene(scene);
                sceneChanged = false;
            }

            if (windowResized)
            {
                windowResized = false;
                ResourceManager2.Shared.SetOrAddResource("SwapChain.RTV", swapChain.BackbufferRTV);
                ResourceManager2.Shared.SetOrAddResource("SwapChain", swapChain.Backbuffer);
                postProcessing.ResizeOutput();
            }

            postProcessing.SetViewport(viewport);

            if (!initialized)
            {
                return;
            }

            if (camera == null)
            {
                return;
            }

            var lights = scene.LightManager;
            var renderers = scene.RenderManager;

#if PROFILE
            profiler.Begin("Update");
            device.Profiler.Begin(context, "Update");
#endif

            cameraBuffer[0] = new CBCamera(camera, new(width, height), cameraBuffer[0]);
            cameraBuffer.Update(context);

            renderers.Update(context);
            lights.Update(context, camera);
            lights.UpdateBuffers(context);
            CullingManager.UpdateCamera(context);

#if PROFILE
            device.Profiler.End(context, "Update");
            profiler.End("Update");
#endif
            if (prepassEnabled)
            {
#if PROFILE
                profiler.Begin("PrePass");
                device.Profiler.Begin(context, "PrePass");
#endif
                context.ClearDepthStencilView(dsv, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
                context.SetRenderTargets(0, null, dsv);
                context.SetViewport(gbuffer.Viewport);
                renderers.DrawDepth(context, RenderQueueIndex.Geometry | RenderQueueIndex.Transparency);
                context.ClearState();

                postProcessing.PrePassDraw(context);

#if PROFILE
                device.Profiler.End(context, "PrePass");
                profiler.End("PrePass");
#endif
            }
#if PROFILE
            profiler.Begin("ObjectCulling");
            device.Profiler.Begin(context, "ObjectCulling");
#endif

            hizBuffer.Generate(context, depthStencil.SRV);
            CullingManager.DoCulling(context, hizBuffer);

#if PROFILE
            device.Profiler.End(context, "ObjectCulling");
            profiler.End("ObjectCulling");
#endif

#if PROFILE
            profiler.Begin("LightCulling");
            device.Profiler.Begin(context, "LightCulling");
#endif
            if (commandList == null)
            {
                lights.CullLights(context);
                commandList = deferredContext.FinishCommandList(false);
            }
            context.ExecuteCommandList(commandList, false);
#if PROFILE
            device.Profiler.End(context, "LightCulling");
            profiler.End("LightCulling");
#endif

            if (!prepassEnabled)
            {
                context.ClearDepthStencilView(dsv, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
            }

            context.ClearRenderTargetView(lightBuffer.Value.RTV, default);
#if PROFILE
            profiler.Begin("ShadowMaps");
            device.Profiler.Begin(context, "ShadowMaps");
#endif
            renderers.UpdateShadowMaps(context, camera);
#if PROFILE
            device.Profiler.End(context, "ShadowMaps");
            profiler.End("ShadowMaps");
#endif

#if PROFILE
            profiler.Begin("Geometry");
            device.Profiler.Begin(context, "Geometry");
#endif
            context.ClearRenderTargetViews(gbuffer.Count, gbuffer.PRTVs, Vector4.Zero);
            if (!forceForward)
            {
                // Fill Geometry Buffer
                context.SetRenderTargets(gbuffer.Count, gbuffer.PRTVs, depthStencil.DSV);
                context.SetViewport(gbuffer.Viewport);
                renderers.Draw(context, RenderQueueIndex.Geometry, RenderPath.Deferred);
                context.ClearState();
            }

#if PROFILE
            device.Profiler.End(context, "Geometry");
            profiler.End("Geometry");
#endif

#if PROFILE
            profiler.Begin("LightsDeferred");
            device.Profiler.Begin(context, "LightsDeferred");
#endif

            depthStencil.CopyTo(context, depthStencil2);

            context.SetRenderTarget(lightBuffer.Value.RTV, dsv);
            context.SetViewport(lightBuffer.Value.RTV.Viewport);
            renderers.Draw(context, RenderQueueIndex.Background, RenderPath.Default);
            context.ClearState();

            if (!forceForward)
            {
                context.SetRenderTarget(lightBuffer.Value.RTV, default);
                context.SetViewport(gbuffer.Viewport);

                lights.DeferredPass(context);
            }

#if PROFILE
            device.Profiler.End(context, "LightsDeferred");
            profiler.End("LightsDeferred");
#endif
#if PROFILE
            profiler.Begin("LightsForward");
            device.Profiler.Begin(context, "LightsForward");
#endif
            if (forceForward)
            {
                var geometryQueue = renderers.GeometryQueue;
                for (int i = 0; i < geometryQueue.Count; i++)
                {
                    lights.ForwardPass(context, geometryQueue[i], camera);
                }
            }
            var alphaTest = renderers.AlphaTestQueue;
            for (int i = 0; i < alphaTest.Count; i++)
            {
                lights.ForwardPass(context, alphaTest[i], camera);
            }
            var transparency = renderers.TransparencyQueue;
            for (int i = 0; i < transparency.Count; i++)
            {
                lights.ForwardPass(context, transparency[i], camera);
            }

#if PROFILE
            device.Profiler.End(context, "LightsForward");
            profiler.End("LightsForward");
#endif

#if PROFILE
            profiler.Begin("AO");
            device.Profiler.Begin(context, "AO");
#endif
            // SSAO Pass
            ssao.Draw(context);
#if PROFILE
            device.Profiler.End(context, "AO");
            profiler.End("AO");
#endif

#if PROFILE
            profiler.Begin("PostProcessing");
            device.Profiler.Begin(context, "PostProcessing");
#endif
            postProcessing.Draw(context);
#if PROFILE
            device.Profiler.End(context, "PostProcessing");
            profiler.End("PostProcessing");
#endif
        }

        private bool prepassEnabled = true;

        public void DrawSettings()
        {
            hizBuffer.Debug();
            if (!initialized)
            {
                return;
            }

            var resources = ResourceManager2.Shared.Resources;

            for (int i = 0; i < resources.Count; i++)
            {
                var size = ImGui.GetWindowContentRegionMax();

                var resource = resources[i];

                if (resource.Value is IShaderResourceView srv)
                {
                    size.Y = size.X / 16 * 9;

                    if (ImGui.CollapsingHeader(resource.Name))
                    {
                        ImGui.Image(srv.NativePointer, size);
                    }
                }
                if (resource.Value is Texture2D texture && texture.SRV != null)
                {
                    float aspect = texture.Viewport.Height / texture.Viewport.Width;
                    size.X = MathF.Min(texture.Viewport.Width, size.X);
                    size.Y = texture.Viewport.Height;
                    var dx = texture.Viewport.Width - size.X;
                    if (dx > 0)
                    {
                        size.Y = size.X * aspect;
                    }

                    if (ImGui.CollapsingHeader(resource.Name))
                    {
                        ImGui.Image(texture.SRV.NativePointer, size);
                    }
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (!initialized)
                {
                    return;
                }

                Current = null;

                dsv.Dispose();
                depthStencil.Dispose();
                depthStencil2.Dispose();

                hizBuffer.Dispose();
                deferredContext.Dispose();
                cameraBuffer.Dispose();
                tesselationBuffer.Dispose();
                quad.Dispose();

                gbuffer.Dispose();

                ssao.Dispose();

                postProcessing.Dispose();

                brdflut.Dispose();
                disposedValue = true;
            }
        }

        ~SceneRenderer()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}