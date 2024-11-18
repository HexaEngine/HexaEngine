namespace HexaEngine.Graphics.Renderers
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Mathematics;
    using HexaEngine.Core;
    using HexaEngine.Core.Configuration;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Textures;
    using HexaEngine.Core.Windows;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Lights;
    using HexaEngine.Meshes;
    using HexaEngine.Profiling;
    using HexaEngine.Scenes;
    using System;
    using System.Numerics;

    public class RendererResizedEventArgs : EventArgs
    {
        public RendererResizedEventArgs(Vector2 oldSize, Vector2 newSize)
        {
            OldSize = oldSize;
            NewSize = newSize;
        }

        public Vector2 OldSize { get; internal set; }

        public Vector2 NewSize { get; internal set; }
    }

    public class OutputViewportChangedEventArgs : EventArgs
    {
        public Viewport OldViewport { get; internal set; }

        public Viewport NewViewport { get; internal set; }
    }

    public class SceneRenderer : ISceneRenderer
    {
        private IGraphicsDevice device = null!;
        private ISwapChain swapChain = null!;
        private ICoreWindow window = null!;

        private readonly RendererSettings settings = new();
        private readonly HDRPipeline renderGraph = null!;
        private RenderGraphExecuter graphExecuter = null!;

        private ResourceRef<ConstantBuffer<CBCamera>> cameraBuffer = null!;

        private bool initialized;
        private bool disposedValue;

        private ConfigKey configKey = null!;
        private float RenderResolution;
        private int width;
        private int height;
        private bool enableProfiling = true;

        private OutputViewportChangedEventArgs outputViewportChangedEventArgs = new();
        private Viewport outputViewport;

        public ViewportShading Shading { get; set; }

        public RenderGraph RenderGraph => renderGraph;

        public RenderGraphExecuter RenderGraphExecuter => graphExecuter;

        public IReadOnlyList<RenderPass> Passes => renderGraph.Passes;

        public GraphResourceBuilder ResourceBuilder => graphExecuter.ResourceBuilder;

        public bool EnableProfiling { get => enableProfiling; set => enableProfiling = value; }

        public int Width => width;

        public int Height => height;

        public Vector2 Size
        {
            get => new(width, height);
            set
            {
                OnRendererResizeBegin();
                var oldWidth = width;
                var oldHeight = height;
                width = (int)value.X;
                height = (int)value.Y;
                Resized?.Invoke(this, new(new(oldWidth, oldHeight), value));
                OnRendererResizeEnd(width, height);
            }
        }

        public Viewport OutputViewport
        {
            get => outputViewport;
            set
            {
                if (outputViewport == value)
                {
                    return;
                }

                outputViewportChangedEventArgs.OldViewport = outputViewport;
                outputViewportChangedEventArgs.NewViewport = value;
                OutputViewportChanged?.Invoke(this, outputViewportChangedEventArgs);
                outputViewport = value;
            }
        }

        public static event EventHandler<RendererResizedEventArgs>? Resized;

        public static event EventHandler<OutputViewportChangedEventArgs>? OutputViewportChanged;

        public SceneRenderer(Windows.RendererFlags flags)
        {
            renderGraph = new(flags);
        }

        public static SceneRenderer Current { get; private set; } = null!;

        public SceneDrawFlags DrawFlags { get; set; }

        /// <summary>
        /// Initializes the renderer.
        /// </summary>
        /// <param dbgName="device">The device.</param>
        /// <param dbgName="window">The window.</param>
        /// <returns></returns>
        public Task Initialize(IGraphicsDevice device, ISwapChain swapChain, ICoreWindow window)
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

            return Task.Factory.StartNew(Initialize);
        }

        private void Initialize()
        {
            InitializeSettings();

            var resourceCreator = graphExecuter.ResourceBuilder;
            resourceCreator.Viewport = new(width, height);

            cameraBuffer = resourceCreator.CreateConstantBuffer<CBCamera>("CBCamera", CpuAccessFlags.Write, ResourceCreationFlags.None);
            resourceCreator.CreateConstantBuffer<CBWeather>("CBWeather", CpuAccessFlags.Write, ResourceCreationFlags.None);
            resourceCreator.CreateTexture2D("#AOBuffer", new(Format.R16Float, width, height, 1, 1, GpuAccessFlags.All), ResourceCreationFlags.None);
            resourceCreator.CreateDepthStencilBuffer("#DepthStencil", new(width, height, 1, Format.D32Float), ResourceCreationFlags.None);
            graphExecuter.Init(CPUProfiler.Global);

            initialized = true;
            Current = this;
        }

        private void InitializeSettings()
        {
            {
                configKey.TryGetOrAddKeyValue("Width", "1920", DataType.Int32, false, out var val);
                width = val.GetInt32();
                val.ValueChanged += (ss, ee) =>
                {
                    width = ss.GetInt32();
                    OnRendererResizeBegin();
                    OnRendererResizeEnd(width, height);
                    Config.SaveGlobal();
                };
            }
            {
                configKey.TryGetOrAddKeyValue("Height", "1080", DataType.Int32, false, out var val);
                height = val.GetInt32();
                val.ValueChanged += (ss, ee) =>
                {
                    height = ss.GetInt32();
                    OnRendererResizeBegin();
                    OnRendererResizeEnd(width, height);
                    Config.SaveGlobal();
                };
            }
            {
                configKey.TryGetOrAddKeyValue("VSync", false.ToString(), DataType.Bool, false, out var val);
                swapChain.VSync = val.GetBool();
                val.ValueChanged += (ss, ee) =>
                {
                    swapChain.VSync = ss.GetBool();
                    Config.SaveGlobal();
                };
            }
            {
                configKey.TryGetOrAddKeyValue("LimitFPS", false.ToString(), DataType.Bool, false, out var val);
                swapChain.LimitFPS = val.GetBool();
                val.ValueChanged += (ss, ee) =>
                {
                    swapChain.LimitFPS = ss.GetBool();
                    Config.SaveGlobal();
                };
            }
            {
                configKey.TryGetOrAddKeyValue("TargetFPS", 120.ToString(), DataType.Int32, false, out var val);
                swapChain.TargetFPS = val.GetInt32();
                val.ValueChanged += (ss, ee) =>
                {
                    swapChain.TargetFPS = ss.GetInt32();
                    Config.SaveGlobal();
                };
            }

            Config.SaveGlobal();
        }

        private void OnRendererResizeBegin()
        {
            if (!initialized)
            {
                return;
            }

            graphExecuter.ResizeBegin();
        }

        private void OnRendererResizeEnd(int width, int height)
        {
            if (!initialized)
            {
                return;
            }
            var resourceCreator = graphExecuter.ResourceBuilder;
            resourceCreator.Viewport = new(width, height);

            cameraBuffer = resourceCreator.CreateConstantBuffer<CBCamera>("CBCamera", CpuAccessFlags.Write, ResourceCreationFlags.None);
            resourceCreator.CreateConstantBuffer<CBWeather>("CBWeather", CpuAccessFlags.Write, ResourceCreationFlags.None);
            resourceCreator.CreateTexture2D("#AOBuffer", new(Format.R16Float, width, height, 1, 1, GpuAccessFlags.RW), ResourceCreationFlags.None);
            resourceCreator.CreateDepthStencilBuffer("#DepthStencil", new(width, height, 1, Format.D32Float), ResourceCreationFlags.None);

            graphExecuter.ResizeEnd(CPUProfiler.Global);

            var scene = SceneManager.Current;
            if (scene == null)
            {
                return;
            }
        }

        [Profile]
        public unsafe void Render(IGraphicsContext context, IScene scene, Camera camera)
        {
            if (!initialized)
            {
                return;
            }

            if (camera == null)
            {
                return;
            }

            var cameraBuffer = this.cameraBuffer.Value!;
            cameraBuffer[0] = new CBCamera(camera, new(width, height), cameraBuffer[0]);
            cameraBuffer.Update(context);

            scene.RenderManager.Update(context);
            scene.WeatherManager.Update(context);
            scene.LightManager.Update(context, graphExecuter.ResourceBuilder.GetShadowAtlas(0), camera);

            graphExecuter.ResourceBuilder.Output = swapChain.BackbufferRTV;
            graphExecuter.ResourceBuilder.OutputViewport = outputViewport;
            graphExecuter.Execute(context, enableProfiling ? CPUProfiler.Global : null);
        }

        public unsafe void RenderTo(IGraphicsContext context, IRenderTargetView target, Hexa.NET.Mathematics.Viewport viewport, IScene scene, Camera camera)
        {
            if (!initialized)
            {
                return;
            }

            if (camera == null)
            {
                return;
            }

            var cameraBuffer = this.cameraBuffer.Value!;
            cameraBuffer[0] = new CBCamera(camera, new(width, height), cameraBuffer[0]);
            cameraBuffer.Update(context);

            scene.RenderManager.Update(context);
            scene.WeatherManager.Update(context);
            scene.LightManager.Update(context, graphExecuter.ResourceBuilder.GetShadowAtlas(0), camera);

            graphExecuter.ResourceBuilder.Output = target;
            graphExecuter.ResourceBuilder.OutputViewport = viewport;
            graphExecuter.Execute(context, enableProfiling ? CPUProfiler.Global : null);
        }

        public void TakeScreenshot(IGraphicsContext context, string path)
        {
            if (!initialized)
            {
                return;
            }

            Texture2D tempTexture = new(Format.R8G8B8A8UNorm, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);

            graphExecuter.ResourceBuilder.Output = tempTexture.RTV;
            graphExecuter.ResourceBuilder.OutputViewport = tempTexture.Viewport;
            graphExecuter.Execute(context, enableProfiling ? CPUProfiler.Global : null);

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

            var depth = graphExecuter.ResourceBuilder.DepthStencilBuffers;
            for (int i = 0; i < depth.Count; i++)
            {
                var size = ImGui.GetContentRegionAvail();

                var texture = depth[i];

                if (texture.SRV != null || !texture.SRV!.IsDisposed)
                {
                    Image(size, texture.Viewport, texture.DebugName ?? $"<unknown>##{i}", texture.SRV.NativePointer);
                }
            }

            var depthChain = graphExecuter.ResourceBuilder.DepthMipChains;
            for (int i = 0; i < depthChain.Count; i++)
            {
                var size = ImGui.GetContentRegionAvail();

                var chain = depthChain[i];

                for (int j = 0; j < chain.MipLevels; j++)
                {
                    var vp = chain.Viewports[j];
                    var srv = chain.SRVs[j];
                    Image(size, vp, $"{chain.DebugName}, Level: {j}", srv.NativePointer);
                }
            }

            var tex = graphExecuter.ResourceBuilder.Textures;
            for (int i = 0; i < tex.Count; i++)
            {
                var size = ImGui.GetContentRegionAvail();

                var texture = tex[i];

                if (texture.SRV != null || !texture.SRV!.IsDisposed)
                {
                    Image(size, texture.Viewport, texture.DebugName ?? $"<unknown>##{i}", texture.SRV.NativePointer);
                }
            }

            var shadowAtlas = graphExecuter.ResourceBuilder.ShadowAtlas;
            for (int i = 0; i < shadowAtlas.Count; i++)
            {
                var size = ImGui.GetContentRegionAvail();

                var atlas = shadowAtlas[i];

                if (atlas.SRV != null)
                {
                    Image(size, atlas.Viewport, atlas.DebugName, atlas.SRV.NativePointer);
                }
            }

            var gBuffers = graphExecuter.ResourceBuilder.GBuffers;
            for (int i = 0; i < gBuffers.Count; i++)
            {
                var size = ImGui.GetContentRegionAvail();

                var gBuffer = gBuffers[i];

                for (int j = 0; j < gBuffer.Count; j++)
                {
                    Image(size, gBuffer.Viewport, $"{gBuffer.DebugName}.{j}", gBuffer.SRVs[j].NativePointer);
                }
            }
        }

        private static void Image(Vector2 size, Viewport viewport, string debugName, nint srv)
        {
            float aspect = viewport.Height / viewport.Width;
            size.X = MathF.Min(viewport.Width, size.X);
            size.Y = viewport.Height;
            var dx = viewport.Width - size.X;
            if (dx > 0)
            {
                size.Y = size.X * aspect;
            }

            if (ImGui.CollapsingHeader(debugName))
            {
                ImGui.Image((ulong)srv, size);
            }
        }

        private static void Image(Vector2 size, Viewport viewport, string debugName, nint srv, Vector4 tint)
        {
            float aspect = viewport.Height / viewport.Width;
            size.X = MathF.Min(viewport.Width, size.X);
            size.Y = viewport.Height;
            var dx = viewport.Width - size.X;
            if (dx > 0)
            {
                size.Y = size.X * aspect;
            }

            if (ImGui.CollapsingHeader(debugName))
            {
                ImGui.Image((ulong)srv, size, tint);
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

                Current = null!;

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}