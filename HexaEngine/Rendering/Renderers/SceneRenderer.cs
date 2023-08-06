#nullable disable

namespace HexaEngine.Rendering.Renderers
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.UI;
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Editor;
    using HexaEngine.Editor.Widgets;
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

    public class RendererSettings
    {
        public float RenderResolution { get; set; }

        public int RendererWidth { get; set; }

        public int RendererHeight { get; set; }

        public bool VSync { get; set; }

        public bool LimitFPS { get; set; }

        public int FPSTarget { get; set; }
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
        private float RenderResolution;
        private int width;
        private int height;
        private int rendererWidth;
        private int rendererHeight;
        private bool windowResized;
        private bool sceneChanged;
        private RendererSettings settings = new();
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
            Config.Global.Sort();
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
            var configKey1 = Config.Global.GenerateSubKeyAuto(settings, "RendererSettings");
            configKey.RemoveValue("renderResolution");
            configKey.Sort();
            RenderResolution = configKey.TryGet(nameof(RenderResolution), 1f);

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
            graphExecuter.Init(profiler1);

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
            window.Dispatcher.Invoke(() => WindowManager.Register(new RendererWidget(this)));
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

        private static readonly CPUFlameProfiler profiler1 = new();
        private int selected = -1;

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
            profiler1.BeginFrame();

            cameraBuffer[0] = new CBCamera(camera, new(width, height), cameraBuffer[0]);
            cameraBuffer.Update(context);

            scene.RenderManager.Update(context);
            scene.LightManager.Update(context, graphExecuter.ResourceCreator.GetShadowAtlas(0), camera);

            graphExecuter.ResourceCreator.Output = swapChain.BackbufferRTV;
            graphExecuter.ResourceCreator.OutputTex = swapChain.Backbuffer;
            graphExecuter.ResourceCreator.OutputViewport = viewport;
            graphExecuter.Execute(context, profiler1);

            profiler1.EndFrame();

            if (ImGui.Begin("Dbg"))
                ImGuiWidgetFlameGraph.PlotFlame("Test Flame", profiler1.Getter, profiler1.Current, profiler1.StageCount, ref selected);
            ImGui.End();
        }

        public void DrawSettings()
        {
            if (!initialized)
            {
                return;
            }

            var tex = graphExecuter.ResourceCreator.Textures;

            for (int i = 0; i < tex.Count; i++)
            {
                var size = ImGui.GetWindowContentRegionMax();

                var texture = tex[i];

                if (texture.SRV != null)
                {
                    float aspect = texture.Viewport.Height / texture.Viewport.Width;
                    size.X = MathF.Min(texture.Viewport.Width, size.X);
                    size.Y = texture.Viewport.Height;
                    var dx = texture.Viewport.Width - size.X;
                    if (dx > 0)
                    {
                        size.Y = size.X * aspect;
                    }

                    if (ImGui.CollapsingHeader(texture.DebugName))
                    {
                        ImGui.Image(texture.SRV.NativePointer, size);
                    }
                }
            }

            var shadowAtlas = graphExecuter.ResourceCreator.ShadowAtlas;
            for (int i = 0; i < shadowAtlas.Count; i++)
            {
                var size = ImGui.GetWindowContentRegionMax();

                var atlas = shadowAtlas[i];

                if (atlas.SRV != null)
                {
                    float aspect = atlas.Viewport.Height / atlas.Viewport.Width;
                    size.X = MathF.Min(atlas.Viewport.Width, size.X);
                    size.Y = atlas.Viewport.Height;
                    var dx = atlas.Viewport.Width - size.X;
                    if (dx > 0)
                    {
                        size.Y = size.X * aspect;
                    }

                    if (ImGui.CollapsingHeader(atlas.DebugName))
                    {
                        ImGui.Image(atlas.SRV.NativePointer, size);
                    }
                }
            }

            var gBuffers = graphExecuter.ResourceCreator.GBuffers;
            for (int i = 0; i < gBuffers.Count; i++)
            {
                var size = ImGui.GetWindowContentRegionMax();

                var gBuffer = gBuffers[i];

                for (int j = 0; j < gBuffer.Count; j++)
                {
                    float aspect = gBuffer.Viewport.Height / gBuffer.Viewport.Width;
                    size.X = MathF.Min(gBuffer.Viewport.Width, size.X);
                    size.Y = gBuffer.Viewport.Height;
                    var dx = gBuffer.Viewport.Width - size.X;
                    if (dx > 0)
                    {
                        size.Y = size.X * aspect;
                    }

                    if (ImGui.CollapsingHeader($"{gBuffer.DebugName}.{j}"))
                    {
                        ImGui.Image(gBuffer.SRVs[j].NativePointer, size);
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