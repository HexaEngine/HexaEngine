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
    using HexaEngine.Editor.Widgets;
    using HexaEngine.Mathematics;
    using HexaEngine.Rendering;
    using HexaEngine.Scenes.Managers;
    using System;
    using System.Numerics;
    using CullingManager = Core.Scenes.Managers.CullingManager;

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
        private bool firstFrame;
        private IGraphicsDevice device;
        private IGraphicsContext context;
        private ISwapChain swapChain;
        private Frameviewer frameviewer;
        private bool sceneGraph;
        private bool imGuiWidgets;
        private SceneRenderer deferredRenderer;
        private Task initTask;
        private bool resize = false;
        private ImGuiRenderer? renderer;

        public RendererFlags Flags;

        public RenderDispatcher Dispatcher => renderDispatcher;

        public IGraphicsDevice Device => device;

        public IGraphicsContext Context => context;

        public ISwapChain SwapChain => swapChain;

        public string? StartupScene;
        private Viewport renderViewport;

        public bool DebugGraphics { get; set; } = false;

        public Viewport RenderViewport => renderViewport;

        public ISceneRenderer Renderer => deferredRenderer;

        public Window()
        {
        }

        public void RenderInitialize()
        {
            if (OperatingSystem.IsWindows())
            {
                device = Adapter.CreateGraphics(RenderBackend.D3D11, DebugGraphics);
                context = device.Context;
                swapChain = device.CreateSwapChain(this) ?? throw new PlatformNotSupportedException();
                swapChain.Active = true;
                renderDispatcher = new(device, Thread.CurrentThread);
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

            frameviewer = new(device);

            sceneGraph = Flags.HasFlag(RendererFlags.SceneGraph);
            imGuiWidgets = Flags.HasFlag(RendererFlags.ImGuiWidgets);

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
            initTask = deferredRenderer.Initialize(device, swapChain, this);
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
        }

        public void Render()
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
                frameviewer.SourceViewport = Viewport;
                frameviewer.Update();
                frameviewer.Draw();
            }

            var drawing = initTask.IsCompleted && sceneGraph && SceneManager.Current is not null;

            if (drawing)
                lock (SceneManager.Current)
                {
                    SceneManager.Current.Tick();
                    if (firstFrame)
                    {
                        Time.Initialize();
                        firstFrame = false;
                    }
                    deferredRenderer.Profiler.Clear();
                    deferredRenderer.Profiler.Start(deferredRenderer);
                    renderViewport = Application.InEditorMode ? frameviewer.Viewport : Viewport;
                    deferredRenderer.Render(context, this, renderViewport, SceneManager.Current, CameraManager.Current);
                }

            OnRender(context);

            renderer?.EndDraw();

            swapChain.Present();
            if (drawing)
                deferredRenderer.Profiler.End(deferredRenderer);
            swapChain.Wait();

            Time.FrameUpdate();
        }

        public void RenderDispose()
        {
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
    }
}