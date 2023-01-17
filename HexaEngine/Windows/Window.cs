namespace HexaEngine.Windows
{
    using HexaEngine.Audio;
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Events;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.Core.Windows;
    using HexaEngine.Editor;
    using HexaEngine.Mathematics;
    using HexaEngine.Rendering;
    using HexaEngine.Scenes.Managers;
    using System;
    using System.Numerics;

    public enum RendererFlags
    {
        None = 0,
        SceneGraph = 1,
        ImGui = 2,
        ImGuizmo = 4,
        DebugDraw = 8,
        ImGuiWidgets = 16,
        All = SceneGraph | ImGui | ImGuizmo | ImGuiWidgets,
    }

    public class Window : SdlWindow, IRenderWindow
    {
        private RenderDispatcher renderDispatcher;
        private Thread renderThread;
        private bool isRunning = true;
        private bool firstFrame;
        private IGraphicsDevice device;
        private IGraphicsContext context;
        private ISwapChain swapChain;
        private Framebuffer framebuffer;
        private SceneRenderer deferredRenderer;

        private bool resize = false;
        private ImGuiRenderer? renderer;

        public RendererFlags Flags;

        public RenderDispatcher Dispatcher => renderDispatcher;

        public IGraphicsDevice Device => device;

        public IGraphicsContext Context => context;

        public ISwapChain SwapChain => swapChain;

        public string? StartupScene;
        private Viewport renderViewport;

        public bool DebugGraphics { get; set; } = true;

        public Viewport RenderViewport => renderViewport;

        public Window()
        {
        }

        protected override void OnShown(ShownEventArgs args)
        {
            renderThread = new(RenderVoid);
            renderThread.Name = "RenderThread";
            renderThread.Start();
            base.OnShown(args);
        }

        [STAThread]
        private void RenderVoid()
        {
            if (OperatingSystem.IsWindows())
            {
                device = Adapter.CreateGraphics(RenderBackend.D3D11, DebugGraphics);
                context = device.Context;
                swapChain = device.CreateSwapChain(this) ?? throw new PlatformNotSupportedException();
                swapChain.Active = true;
                renderDispatcher = new(device, renderThread);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            AudioManager.Initialize();
            ResourceManager.Initialize(device);
            PipelineManager.Initialize(device);
            CullingManager.Initialize(device);
            ObjectPickerManager.Initialize(device, Width, Height);

            framebuffer = new(device);

            bool sceneGraph = Flags.HasFlag(RendererFlags.SceneGraph);
            bool imGuiWidgets = Flags.HasFlag(RendererFlags.ImGuiWidgets);

            if (Flags.HasFlag(RendererFlags.ImGui))
            {
                renderer = new(this, device, swapChain);
                DebugDraw.Init(device);
            }

            if (Flags.HasFlag(RendererFlags.ImGuiWidgets))
                WidgetManager.Init(device);

            if (Flags.HasFlag(RendererFlags.SceneGraph))
                SceneManager.SceneChanged += (_, _) => { firstFrame = true; };

            Time.Initialize();

            OnRendererInitialize(device);

            deferredRenderer = new();
            Task initTask = deferredRenderer.Initialize(device, swapChain, this);
            initTask.ContinueWith(x =>
            {
                if (x.IsCompletedSuccessfully)
                {
                    ImGuiConsole.Log(LogSeverity.Info, "Renderer: Initialized");
                }
                if (x.IsFaulted)
                {
                    ImGuiConsole.Log(LogSeverity.Error, "Renderer: Failed Initialize");
                    ImGuiConsole.Log(x.Exception);
                }
            });

            if (StartupScene != null)
            {
                Task.Run(() => SceneManager.AsyncLoad(StartupScene)).ContinueWith(x => SceneManager.Current.IsSimulating = true);
            }

            while (isRunning)
            {
                if (resize)
                {
                    swapChain.Resize(Width, Height);
                    resize = false;
                    ObjectPickerManager.Resize(Width, Height);
                }

                if (firstFrame)
                {
                    Time.Initialize();
                    firstFrame = false;
                }

                context.ClearDepthStencilView(swapChain.BackbufferDSV, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
                context.ClearRenderTargetView(swapChain.BackbufferRTV, Vector4.Zero);

                renderDispatcher.ExecuteQueue(context);

                renderer?.BeginDraw();

                if (imGuiWidgets && Application.InEditorMode)
                {
                    Designer.Draw();
                    WidgetManager.Draw(context);
                    ImGuiConsole.Draw();
                    framebuffer.SourceViewport = Viewport;
                    framebuffer.Update();
                    framebuffer.Draw();
                }

                if (initTask.IsCompleted && sceneGraph && SceneManager.Current is not null)
                    lock (SceneManager.Current)
                    {
                        SceneManager.Current.Tick();
                        if (firstFrame)
                        {
                            Time.Initialize();
                            firstFrame = false;
                        }
                        renderViewport = Application.InEditorMode ? framebuffer.Viewport : Viewport;
                        deferredRenderer.Render(context, this, renderViewport, SceneManager.Current, CameraManager.Current);
                    }

                OnRender(context);

                renderer?.EndDraw();
                swapChain.Present();
                ProcessInput();
                Time.FrameUpdate();
            }

            OnRendererDispose();

            if (Flags.HasFlag(RendererFlags.ImGuiWidgets))
                WidgetManager.Dispose();

            renderer?.Dispose();

            if (renderer is not null)
                DebugDraw.Dispose();

            if (Flags.HasFlag(RendererFlags.SceneGraph))
                SceneManager.Unload();
            if (!initTask.IsCompleted)
                initTask.Wait();
            deferredRenderer.Dispose();
            renderDispatcher.Dispose();
            ObjectPickerManager.Release();
            CullingManager.Release();
            ResourceManager.Release();
            AudioManager.Release();
            swapChain.Dispose();
            context.Dispose();
            device.Dispose();
        }

        protected virtual void OnRendererInitialize(IGraphicsDevice device)
        {
        }

        protected virtual void OnRender(IGraphicsContext context)
        {
        }

        protected virtual void OnRendererDispose()
        {
        }

        protected override void OnResized(ResizedEventArgs args)
        {
            resize = true;
            base.OnResized(args);
        }

        protected override void OnClose(CloseEventArgs args)
        {
            isRunning = false;
            renderThread?.Join();
            base.OnClose(args);
        }
    }
}