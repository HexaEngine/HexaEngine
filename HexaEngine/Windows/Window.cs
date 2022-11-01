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
    using HexaEngine.Scenes;
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
        private Dispatcher renderDispatcher;
        private Thread? renderThread;
        private bool isRunning = true;
        private bool firstFrame;
        private IGraphicsDevice device;
        private IGraphicsContext context;
        private ISwapChain swapChain;
        private Framebuffer framebuffer;
        private DeferredRenderer deferredRenderer;
        private Task? rendererUpdateTask;

        private bool resize = false;
        private ImGuiRenderer? renderer;

        public RendererFlags Flags;

        public Dispatcher RenderDispatcher => renderDispatcher;

        public IGraphicsDevice Device => device;

        public IGraphicsContext Context => context;

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
            renderDispatcher = Dispatcher.CurrentDispatcher;
            if (OperatingSystem.IsWindows())
            {
                device = Adapter.CreateGraphics(RenderBackend.D3D11, this);
                context = device.Context;
                swapChain = device.SwapChain ?? throw new PlatformNotSupportedException();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            framebuffer = new(device);

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
            deferredRenderer.Initialize(device, this);

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

                Dispatcher.ExecuteQueue();

                if (renderer is not null)
                    renderer.BeginDraw();

                if (Flags.HasFlag(RendererFlags.ImGuiWidgets) && Designer.IsShown)
                {
                    Designer.Draw();
                    WidgetManager.Draw(context);
                    ImGuiConsole.Draw();
                    framebuffer.SourceViewport = Viewport;
                    framebuffer.Update(context);
                    framebuffer.Draw();
                    deferredRenderer.DrawSettings();
                }

                if (Flags.HasFlag(RendererFlags.SceneGraph) && SceneManager.Current is not null)
                    lock (SceneManager.Current)
                    {
                        SceneManager.Current.Tick();
                        if (rendererUpdateTask?.IsCompleted ?? true)
                            rendererUpdateTask = deferredRenderer.Update(SceneManager.Current);
                        if (firstFrame)
                        {
                            rendererUpdateTask.Wait();
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
                DebugDraw.Render(CameraManager.Current, Viewport);
                swapChain.Present();
                Keyboard.FrameUpdate();
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

            deferredRenderer.Dispose();

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