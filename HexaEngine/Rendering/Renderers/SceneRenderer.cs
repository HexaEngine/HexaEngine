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
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Rendering.Passes;
    using HexaEngine.Scenes;
    using ImGuiNET;
    using System;
    using System.Numerics;

    public class HBAOPass : DrawPass
    {
        public HBAOPass() : base("HABO")
        {
            AddReadDependency(new("GBuffer"));
            AddWriteDependency(new("AOBuffer"));
        }

        public override void Init(GraphResourceBuilder creator, GraphPipelineBuilder pipelineCreator, IGraphicsDevice device)
        {
            var viewport = creator.Viewport;
            creator.CreateTexture2D("AOBuffer", new(Format.R32Float, (int)viewport.Width, (int)viewport.Height, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget));
        }
    }

    public class BRDFLUTPass : RenderPass
    {
        public BRDFLUTPass() : base("BRDFLUT")
        {
        }

        public override void Init(GraphResourceBuilder creator, GraphPipelineBuilder pipelineCreator, IGraphicsDevice device)
        {
            creator.CreateTexture2D("BRDFLUT", new(Format.R16G16B16A16Float, 128, 128, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget));
        }
    }

    public class PostProcessPass : DrawPass
    {
        private PostProcessingManager postProcessingManager;

        public PostProcessPass() : base("PostProcess")
        {
            AddReadDependency(new("GBuffer"));
            AddReadDependency(new("LightBuffer"));
            AddReadDependency(new("#DepthStencil"));
        }

        public override void Init(GraphResourceBuilder creator, GraphPipelineBuilder pipelineCreator, IGraphicsDevice device)
        {
            var viewport = creator.Viewport;
            postProcessingManager = new(device, (int)viewport.Width, (int)viewport.Height);
            postProcessingManager.Add(new VelocityBuffer());
            postProcessingManager.Add(new HBAO());
            postProcessingManager.Add(new VolumetricClouds());
            postProcessingManager.Add(new SSR());
            postProcessingManager.Add(new SSGI());
            postProcessingManager.Add(new MotionBlur());
            postProcessingManager.Add(new DepthOfField());
            postProcessingManager.Add(new AutoExposure());
            postProcessingManager.Add(new Bloom());
            postProcessingManager.Add(new LensFlare());
            postProcessingManager.Add(new GodRays());
            postProcessingManager.Add(new Compose());
            postProcessingManager.Add(new TAA());
            postProcessingManager.Add(new FXAA());
            postProcessingManager.Initialize((int)viewport.Width, (int)viewport.Height);
            postProcessingManager.Enabled = true;
        }

        public override void Execute(IGraphicsContext context, GraphResourceBuilder creator)
        {
            postProcessingManager.Input = creator.GetTexture2D("LightBuffer");
            postProcessingManager.Output = creator.Output;
            postProcessingManager.OutputTex = creator.OutputTex;
            postProcessingManager.Viewport = creator.OutputViewport;
            postProcessingManager.Draw(context, creator);
        }
    }

    public class HDRPipeline : RenderGraph
    {
        public RenderPass[] Passes;

        public HDRPipeline() : base("HDRPipeline")
        {
            BRDFLUTPass brdfLutPass = new();
            DepthPrePass depthPrePass = new();
            PostProcessPrePass postProcessPrePass = new();
            HizDepthPass hizDepthPass = new();

            ObjectCullPass objectCullPass = new();
            LightCullPass lightCullPass = new();
            ShadowMapPass shadowMapPass = new();
            GBufferPass gBufferPass = new();
            LightForwardPass lightForwardPass = new();
            PostProcessPass postProcessPass = new();

            brdfLutPass.Build(this);
            depthPrePass.Build(this);
            postProcessPrePass.Build(this);
            hizDepthPass.Build(this);
            objectCullPass.Build(this);
            lightCullPass.Build(this);
            shadowMapPass.Build(this);
            gBufferPass.Build(this);
            lightForwardPass.Build(this);
            postProcessPass.Build(this);

            Passes = new RenderPass[]
            {
                brdfLutPass,
                depthPrePass,
                postProcessPrePass,
                hizDepthPass,
                objectCullPass,
                lightCullPass,
                shadowMapPass,
                gBufferPass,
                lightForwardPass,
                postProcessPass
            };
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

        private ConstantBuffer<CBCamera> cameraBuffer;
        private ConstantBuffer<CBWeather> weatherBuffer;
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
        private HDRPipeline renderGraph = new HDRPipeline();
        private RenderGraphExecuter graphExecuter;

        public CPUProfiler Profiler => profiler;

        public PostProcessingManager PostProcessing => postProcessing;

        public ViewportShading Shading { get => shading; set => shading = value; }

        public RenderGraph RenderGraph => renderGraph;
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

            RenderGraph.ResolveGlobalResources();
            RenderGraph.Build();

            graphExecuter = new(device, renderGraph, renderGraph.Passes);

            return
            Task.Factory.StartNew(Initialize);
        }

        private async void Initialize()
        {
            InitializeSettings();

            tesselationBuffer = new(device, CpuAccessFlags.Write);

            var resourceCreator = graphExecuter.ResourceCreator;
            resourceCreator.Viewport = new(width, height);

            cameraBuffer = resourceCreator.CreateConstantBuffer<CBCamera>("CBCamera", CpuAccessFlags.Write);
            ResourceManager2.Shared.AddConstantBuffer("CBCamera", cameraBuffer);

            weatherBuffer = resourceCreator.CreateConstantBuffer<CBWeather>("CBWeather", CpuAccessFlags.Write);
            ResourceManager2.Shared.AddConstantBuffer("CBWeather", weatherBuffer);

            var aoBuffer = resourceCreator.CreateTexture2D("#AOBuffer", new(Format.R16Float, width, height, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget));

            resourceCreator.CreateDepthStencilBuffer("#DepthStencil", new(width, height, 1, Format.D32Float));
            graphExecuter.Init();

            ResourceManager2.Shared.AddTexture("AOBuffer", resourceCreator.GetTexture2D(aoBuffer));

            //brdflut = ResourceManager2.Shared.AddTexture("BRDFLUT", new Texture2DDescription(Format.R16G16B16A16Float, 128, 128, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget)).Value;

            //window.Dispatcher.InvokeBlocking(() =>
            //{
            //    brdfLUT = new(device, false, true);
            //    brdfLUT.Target = brdflut.RTV;
            //    brdfLUT.Draw(context);
            //    context.ClearState();
            //    brdfLUT.Dispose();
            //    brdfLUT = null;
            //});

            initialized = true;
            Current = this;
            window.Dispatcher.Invoke(() => WidgetManager.Register(new RendererWidget(this)));
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

            hizBuffer.Resize(device, width, height);

            postProcessing.EndResize(width, height);

            var scene = SceneManager.Current;
            if (scene == null)
            {
                return;
            }
        }

        public unsafe void Render(IGraphicsContext context, IRenderWindow window, Mathematics.Viewport viewport, Scene scene, Camera camera)
        {
            if (!initialized)
            {
                return;
            }

            if (camera == null)
            {
                return;
            }

            cameraBuffer[0] = new CBCamera(camera, new(width, height), cameraBuffer[0]);
            cameraBuffer.Update(context);

            scene.RenderManager.Update(context);
            scene.LightManager.Update(context, graphExecuter.ResourceCreator.GetShadowAtlas(0), camera);

            graphExecuter.ResourceCreator.Output = swapChain.BackbufferRTV;
            graphExecuter.ResourceCreator.OutputTex = swapChain.Backbuffer;
            graphExecuter.ResourceCreator.OutputViewport = viewport;
            graphExecuter.Execute(context);

            /*

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
             CullingManager.DoCulling(context, hizBuffer.SRV);

 #if PROFILE
             device.Profiler.End(context, "ObjectCulling");
             profiler.End("ObjectCulling");
 #endif

 #if PROFILE
             profiler.Begin("LightCulling");
             device.Profiler.Begin(context, "LightCulling");
 #endif

             lights.CullLights(context);

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
            */
        }

        public void DrawSettings()
        {
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