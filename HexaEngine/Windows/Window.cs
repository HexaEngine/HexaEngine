namespace HexaEngine.Windows
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Events;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Editor;
    using HexaEngine.Graphics;
    using HexaEngine.Rendering;
    using HexaEngine.Resources;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using System;
    using System.Diagnostics;
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

    public class Window : SdlWindow
    {
        private RenderDispatcher renderDispatcher;
        private Thread? renderThread;
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

        public RenderDispatcher RenderDispatcher => renderDispatcher;

        public IGraphicsDevice Device => device;

        public IGraphicsContext Context => context;

        protected override void OnShown(ShownEventArgs args)
        {
            renderThread = new(RenderVoid);
            renderThread.Name = "RenderThread";
            renderThread.Start();
            base.OnShown(args);
        }

        protected override void OnFocusGained(FocusGainedEventArgs args)
        {
            if (swapChain != null)
                renderDispatcher.Invoke(() => swapChain.Active = true);
            base.OnFocusGained(args);
        }

        protected override void OnFocusLost(FocusLostEventArgs args)
        {
            if (swapChain != null)
                renderDispatcher.Invoke(() => swapChain.Active = false);
            base.OnFocusLost(args);
        }

        [STAThread]
        private void RenderVoid()
        {
            if (OperatingSystem.IsWindows())
            {
                device = Adapter.CreateGraphics(RenderBackend.D3D11, this);
                context = device.Context;
                swapChain = device.SwapChain ?? throw new PlatformNotSupportedException();
                swapChain.Active = true;
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            ResourceManager.Initialize(device);
            PipelineManager.Initialize(device);
            CullingManager.Initialize(device);

            renderDispatcher = new(device);
            framebuffer = new(device);

            bool sceneGraph = Flags.HasFlag(RendererFlags.SceneGraph);
            bool imGuiWidgets = Flags.HasFlag(RendererFlags.ImGuiWidgets);

            if (Flags.HasFlag(RendererFlags.ImGui))
            {
                renderer = new(this, device);
                DebugDraw.Init(device);
            }

            if (Flags.HasFlag(RendererFlags.ImGuiWidgets))
                WidgetManager.Init(device);

            if (Flags.HasFlag(RendererFlags.SceneGraph))
                SceneManager.SceneChanged += (_, _) => { firstFrame = true; };

            Time.Initialize();

            OnRendererInitialize(device);

            deferredRenderer = new();
            Task initTask = deferredRenderer.Initialize(device, this);
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
            while (isRunning)
            {
                if (resize)
                {
                    device.SwapChain.Resize(Width, Height);
                    resize = false;
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

                if (imGuiWidgets && Designer.IsShown)
                {
                    Designer.Draw();
                    WidgetManager.Draw(context);
                    ImGuiConsole.Draw();
                    framebuffer.SourceViewport = Viewport;
                    framebuffer.Update(context);
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
                        if (Designer.IsShown)
                            deferredRenderer.Render(context, this, framebuffer.Viewport, SceneManager.Current, CameraManager.Current);
                        else
                            deferredRenderer.Render(context, this, Viewport, SceneManager.Current, CameraManager.Current);
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
            CullingManager.Release();
            ResourceManager.Release();
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