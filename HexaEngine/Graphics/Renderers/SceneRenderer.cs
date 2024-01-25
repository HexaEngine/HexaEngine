#nullable disable

namespace HexaEngine.Graphics.Renderers
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core;
    using HexaEngine.Core.Configuration;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Textures;
    using HexaEngine.Core.Windows;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Lights;
    using HexaEngine.Meshes;
    using HexaEngine.Scenes;
    using System;
    using System.Numerics;

    public class SceneRenderer : ISceneRenderer
    {
        private IGraphicsDevice device;
        private ISwapChain swapChain;
        private IRenderWindow window;

        private readonly RendererSettings settings = new();
        private readonly CPUFlameProfiler profiler = new();
        private readonly HDRPipeline renderGraph;
        private RenderGraphExecuter graphExecuter;

        private ResourceRef<ConstantBuffer<CBCamera>> cameraBuffer;
        private ResourceRef<ConstantBuffer<CBWeather>> weatherBuffer;

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

        public RenderGraphExecuter RenderGraphExecuter => graphExecuter;

        public IReadOnlyList<RenderPass> Passes => renderGraph.Passes;

        public GraphResourceBuilder ResourceBuilder => graphExecuter.ResourceBuilder;

        public int Width => width;

        public int Height => height;

        public Vector2 Size => new(width, height);

        public SceneRenderer(Windows.RendererFlags flags)
        {
            renderGraph = new(flags);
        }

        public static SceneRenderer Current { get; private set; }

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
            this.swapChain = swapChain ?? throw new NotSupportedException("GraphicsDevice needs a swapchain to operate properly");
            this.window = window;

            configKey = Config.Global.GetOrCreateKey("Renderer");
            configKey.RemoveValue("renderResolution");
            configKey.Sort();
            RenderResolution = configKey.GetOrAddValue(nameof(RenderResolution), 1f);

            RenderGraph.ResolveGlobalResources();
            RenderGraph.Build();

            graphExecuter = new(device, renderGraph, renderGraph.Passes);

            return
            Task.Factory.StartNew(Initialize);
        }

        private async void Initialize()
        {
            InitializeSettings();

            var resourceCreator = graphExecuter.ResourceBuilder;
            resourceCreator.Viewport = new(width, height);

            cameraBuffer = resourceCreator.CreateConstantBuffer<CBCamera>("CBCamera", CpuAccessFlags.Write, ResourceCreationFlags.None);
            weatherBuffer = resourceCreator.CreateConstantBuffer<CBWeather>("CBWeather", CpuAccessFlags.Write, ResourceCreationFlags.None);
            aoBuffer = resourceCreator.CreateTexture2D("#AOBuffer", new(Format.R16Float, width, height, 1, 1, GpuAccessFlags.RW), ResourceCreationFlags.None);

            resourceCreator.CreateDepthStencilBuffer("#DepthStencil", new(width, height, 1, Format.D32Float), ResourceCreationFlags.None);
            graphExecuter.Init(profiler);

            initialized = true;
            Current = this;
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

            depthStencil = new(device, Format.D32Float, width, height);

            var scene = SceneManager.Current;
            if (scene == null)
            {
                return;
            }
        }

        private int selected = -1;
        private ResourceRef<Texture2D> aoBuffer;

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

            var cameraBuffer = this.cameraBuffer.Value;
            cameraBuffer[0] = new CBCamera(camera, new(width, height), cameraBuffer[0]);
            cameraBuffer.Update(context);

            scene.RenderManager.Update(context);
            scene.WeatherManager.Update(context);
            scene.LightManager.Update(context, graphExecuter.ResourceBuilder.GetShadowAtlas(0), camera);

            graphExecuter.ResourceBuilder.Output = swapChain.BackbufferRTV;
            graphExecuter.ResourceBuilder.OutputTex = swapChain.Backbuffer;
            graphExecuter.ResourceBuilder.OutputViewport = viewport;
            graphExecuter.Execute(context, profiler);
        }

        public void TakeScreenshot(IGraphicsContext context, string path)
        {
            if (!initialized)
            {
                return;
            }

            Texture2D tempTexture = new(context.Device, Format.R8G8B8A8UNorm, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);

            graphExecuter.ResourceBuilder.Output = tempTexture.RTV;
            graphExecuter.ResourceBuilder.OutputTex = tempTexture;
            graphExecuter.ResourceBuilder.OutputViewport = tempTexture.Viewport;
            graphExecuter.Execute(context, profiler);

            tempTexture.Dispose();

            var scratchImage = context.Device.TextureLoader.CaptureTexture(context, tempTexture);
            scratchImage.SaveToFile(path, TexFileFormat.Auto, 0);
            scratchImage.Dispose();
        }

        public void DrawSettings()
        {
            if (!initialized)
            {
                return;
            }

            var tex = graphExecuter.ResourceBuilder.Textures;

            for (int i = 0; i < tex.Count; i++)
            {
                var size = ImGui.GetWindowContentRegionMax();

                var texture = tex[i];

                if (texture.SRV != null || !texture.SRV.IsDisposed)
                {
                    float aspect = texture.Viewport.Height / texture.Viewport.Width;
                    size.X = MathF.Min(texture.Viewport.Width, size.X);
                    size.Y = texture.Viewport.Height;
                    var dx = texture.Viewport.Width - size.X;
                    if (dx > 0)
                    {
                        size.Y = size.X * aspect;
                    }

                    if (ImGui.CollapsingHeader(texture.DebugName ?? $"<unknown>##{i}"))
                    {
                        ImGui.Image(texture.SRV.NativePointer, size);
                    }
                }
            }

            var shadowAtlas = graphExecuter.ResourceBuilder.ShadowAtlas;
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

            var gBuffers = graphExecuter.ResourceBuilder.GBuffers;
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

                graphExecuter.Release();

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