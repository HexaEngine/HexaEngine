#nullable disable

namespace HexaEngine.Rendering.Renderers
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Windows;
    using HexaEngine.Editor;
    using HexaEngine.Editor.Widgets;
    using HexaEngine.Lights;
    using HexaEngine.Meshes;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Scenes;
    using ImGuiNET;
    using System;
    using System.Numerics;

    public class SceneRenderer : ISceneRenderer
    {
        private IGraphicsDevice device;
        private ISwapChain swapChain;
        private IRenderWindow window;

        private readonly RendererSettings settings = new();
        private readonly CPUFlameProfiler profiler = new();
        private readonly HDRPipeline renderGraph = new();
        private RenderGraphExecuter graphExecuter;

        private ConstantBuffer<CBCamera> cameraBuffer;
        private ConstantBuffer<CBWeather> weatherBuffer;
        private ConstantBuffer<CBTessellation> tesselationBuffer;

        private bool initialized;
        private bool disposedValue;

        private DepthStencil depthStencil;

        private ConfigKey configKey;
        private float RenderResolution;
        private int width;
        private int height;
        private int rendererWidth;
        private int rendererHeight;

        public ICPUFlameProfiler Profiler => profiler;

        public ViewportShading Shading { get; set; }

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
            this.swapChain = swapChain ?? throw new NotSupportedException("Device needs a swapchain to operate properly");
            this.window = window;

            ResourceManager2.Shared.SetOrAddResource("SwapChain.RTV", swapChain.BackbufferRTV);
            ResourceManager2.Shared.SetOrAddResource("SwapChain", swapChain.Backbuffer);

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
            graphExecuter.Init(profiler);

            ResourceManager2.Shared.AddTexture("AOBuffer", resourceCreator.GetTexture2D(aoBuffer));

            initialized = true;
            Current = this;
            window.Dispatcher.Invoke(() => WindowManager.Register(new RendererWidget(this)));
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

            depthStencil = new(device, width, height, Format.D32Float);

            var scene = SceneManager.Current;
            if (scene == null)
            {
                return;
            }
        }

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

            cameraBuffer[0] = new CBCamera(camera, new(width, height), cameraBuffer[0]);
            cameraBuffer.Update(context);

            scene.RenderManager.Update(context);
            scene.WeatherManager.Update(context);
            scene.LightManager.Update(context, graphExecuter.ResourceCreator.GetShadowAtlas(0), camera);

            graphExecuter.ResourceCreator.Output = swapChain.BackbufferRTV;
            graphExecuter.ResourceCreator.OutputTex = swapChain.Backbuffer;
            graphExecuter.ResourceCreator.OutputViewport = viewport;
            graphExecuter.Execute(context, profiler);
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