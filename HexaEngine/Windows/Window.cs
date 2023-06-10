using HexaEngine.Core.Culling;

namespace HexaEngine.Windows
{
    using HexaEngine.Core;
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Editor;
    using HexaEngine.Mathematics;
    using HexaEngine.Rendering;
    using HexaEngine.Scenes.Managers;
    using Silk.NET.OpenAL;
    using System;
    using System.Numerics;
    using CullingManager = CullingManager;

    public enum RendererFlags
    {
        None = 0,
        ImGui = 1,
        ImGuiWidgets = 2,
        DebugDraw = 4,
        All = ImGui | ImGuiWidgets | DebugDraw,
    }

    public class Window : SdlWindow, IRenderWindow
    {
        private RenderDispatcher renderDispatcher;
        private bool firstFrame;
        private IAudioDevice audioDevice;
        private IGraphicsDevice graphicsDevice;
        private IGraphicsContext graphicsContext;
        private ISwapChain swapChain;
        private Frameviewer frameviewer;
        private bool imGuiWidgets;
        private SceneRenderer sceneRenderer;
        private Task initTask;
        private bool rendererInitialized;
        private bool resize = false;
        private ImGuiRenderer? imGuiRenderer;
        private DebugDrawRenderer? debugDrawRenderer;

        public RendererFlags Flags;

        public RenderDispatcher Dispatcher => renderDispatcher;

        public IGraphicsDevice Device => graphicsDevice;

        public IGraphicsContext Context => graphicsContext;

        public IAudioDevice AudioDevice => audioDevice;

        public ISwapChain SwapChain => swapChain;

        public string? StartupScene;
        private Viewport windowViewport;
        private Viewport renderViewport;

        public Viewport WindowViewport => windowViewport;

        public ISceneRenderer Renderer => sceneRenderer;

        public Viewport RenderViewport => renderViewport;

        public Window()
        {
        }

        public virtual void Initialize(IAudioDevice audioDevice, IGraphicsDevice graphicsDevice)
        {
            this.audioDevice = audioDevice;
            this.graphicsDevice = graphicsDevice;
            graphicsDevice.Profiler.CreateBlock("Total");
            graphicsContext = graphicsDevice.Context;
            swapChain = graphicsDevice.CreateSwapChain(this) ?? throw new PlatformNotSupportedException();
            swapChain.Active = true;
            renderDispatcher = new(Thread.CurrentThread);

            if (Application.MainWindow == this)
            {
                AudioManager.Initialize(audioDevice);
                ResourceManager.Initialize(graphicsDevice);
                PipelineManager.Initialize(graphicsDevice);
                CullingManager.Initialize(graphicsDevice);
                ObjectPickerManager.Initialize(graphicsDevice, Width, Height);
            }

            if (Application.InEditorMode)
            {
                Designer.Init(graphicsDevice);
            }

            frameviewer = new(graphicsDevice);

            imGuiWidgets = (Flags & RendererFlags.ImGuiWidgets) != 0;

            if ((Flags & RendererFlags.ImGui) != 0)
            {
                imGuiRenderer = new(this, graphicsDevice, swapChain);
            }

            if ((Flags & RendererFlags.ImGuiWidgets) != 0)
            {
                WidgetManager.Init(graphicsDevice);
            }

            if ((Flags & RendererFlags.DebugDraw) != 0)
            {
                debugDrawRenderer = new(graphicsDevice, swapChain);
            }

            SceneManager.SceneChanged += SceneChanged;

            OnRendererInitialize(graphicsDevice);

            sceneRenderer = new();
            initTask = sceneRenderer.Initialize(graphicsDevice, swapChain, this);
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

                renderViewport = new(sceneRenderer.Width, sceneRenderer.Height);

                rendererInitialized = true;
            });

            if (StartupScene != null)
            {
                SceneManager.Load(StartupScene);
                SceneManager.Current.IsSimulating = true;
            }
        }

        private void SceneChanged(object? sender, SceneChangedEventArgs e)
        {
            firstFrame = true;
        }

        public void Render(IGraphicsContext context)
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

            renderDispatcher.ExecuteQueue();

            imGuiRenderer?.BeginDraw();
            debugDrawRenderer?.BeginDraw();

            OnRenderBegin(context);

            var drawing = rendererInitialized;
            if (imGuiWidgets && Application.InEditorMode)
            {
                frameviewer.SourceViewport = Viewport;
                frameviewer.Update();
                frameviewer.Draw();
                drawing &= frameviewer.IsVisible;
                windowViewport = Application.InEditorMode ? frameviewer.Viewport : Viewport;

                DebugDraw.SetCamera(CameraManager.Current);
            }

            drawing &= SceneManager.Current is not null;

            if (drawing)
            {
                lock (SceneManager.Current)
                {
                    SceneManager.Current.Tick();
                    if (firstFrame)
                    {
                        Time.Initialize();
                        firstFrame = false;
                    }
                    Device.Profiler.BeginFrame();
                    Device.Profiler.Begin(Context, "Total");
                    sceneRenderer.Profiler.Clear();
                    sceneRenderer.Profiler.Begin("Total");
                    sceneRenderer.Render(context, this, windowViewport, SceneManager.Current, CameraManager.Current);
                    sceneRenderer.Profiler.End("Total");
                    Device.Profiler.End(Context, "Total");
                    Device.Profiler.EndFrame(context);
                }
            }

            Designer.Draw();
            WidgetManager.Draw(context);
            ImGuiConsole.Draw();

            OnRender(context);

            if (Application.InEditorMode)
            {
                debugDrawRenderer?.EndDraw();
            }

            imGuiRenderer?.EndDraw();

            swapChain.Present();

            swapChain.Wait();
        }

        public virtual void Uninitialize()
        {
            OnRendererDispose();

            if (Flags.HasFlag(RendererFlags.ImGuiWidgets))
            {
                WidgetManager.Dispose();
            }

            if (imGuiRenderer is not null)
            {
                imGuiRenderer?.Dispose();
            }

            if ((Flags & RendererFlags.DebugDraw) != 0)
            {
                debugDrawRenderer?.Dispose();
            }

            SceneManager.Unload();
            if (!initTask.IsCompleted)
            {
                initTask.Wait();
            }

            sceneRenderer.Dispose();
            renderDispatcher.Dispose();
            ObjectPickerManager.Release();
            CullingManager.Release();
            ResourceManager.Dispose();
            AudioManager.Release();
            swapChain.Dispose();
            graphicsContext.Dispose();
            graphicsDevice.Dispose();
        }

        protected virtual void OnRendererInitialize(IGraphicsDevice device)
        {
        }

        protected virtual void OnRenderBegin(IGraphicsContext context)
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