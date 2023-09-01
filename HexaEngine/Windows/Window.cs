namespace HexaEngine.Windows
{
    using HexaEngine.Core;
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Editor;
    using HexaEngine.Mathematics;
    using HexaEngine.Rendering.Renderers;
    using HexaEngine.Resources;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using System;
    using System.Numerics;

    public class Window : SdlWindow, IRenderWindow
    {
        private RenderDispatcher renderDispatcher;
        private bool running;
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

        private Thread updateThread;

        private readonly Barrier syncBarrier = new(2);

        private ImGuiManager? imGuiRenderer;
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
            running = true;
            this.audioDevice = audioDevice;
            this.graphicsDevice = graphicsDevice;
#if PROFILE
            graphicsDevice.Profiler.CreateBlock("Total");
            graphicsDevice.Profiler.CreateBlock("DebugDraw");
            graphicsDevice.Profiler.CreateBlock("ImGui");
#endif
            graphicsContext = graphicsDevice.Context;
            swapChain = graphicsDevice.CreateSwapChain(this) ?? throw new PlatformNotSupportedException();
            swapChain.Active = true;
            renderDispatcher = new(Thread.CurrentThread);

            if (Application.MainWindow == this)
            {
                AudioManager.Initialize(audioDevice);
                ResourceManager.Initialize(graphicsDevice);
                PipelineManager.Initialize(graphicsDevice);
            }

            if (Application.InEditorMode)
            {
                Designer.Init(graphicsDevice);
            }

            frameviewer = new(graphicsDevice);

            imGuiWidgets = (Flags & RendererFlags.ImGuiWidgets) != 0;

            if ((Flags & RendererFlags.ImGui) != 0)
            {
                imGuiRenderer = new(this, graphicsDevice, graphicsContext);
            }

            if ((Flags & RendererFlags.ImGuiWidgets) != 0)
            {
                WindowManager.Init(graphicsDevice);
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
                    Logger.Info("Renderer: Initialized");
                }
                if (x.IsFaulted)
                {
                    Logger.Error("Renderer: Failed InitializeAsync");
                    Logger.Log(x.Exception);
                }

                renderViewport = new(sceneRenderer.Width, sceneRenderer.Height);

                rendererInitialized = true;
            });

            if (StartupScene != null)
            {
                SceneManager.Load(StartupScene);
                SceneManager.Current.IsSimulating = true;
            }

            updateThread = new(UpdateScene);
            updateThread.Name = "Scene Update Worker";
            updateThread.Start();
        }

        private void UpdateScene()
        {
            while (running)
            {
#if PROFILE
                syncBarrier.SignalAndWait();
#endif
                SceneManager.Current?.Update();
                syncBarrier.SignalAndWait();
            }
        }

        private void SceneChanged(object? sender, SceneChangedEventArgs e)
        {
            firstFrame = true;
        }

        public void Render(IGraphicsContext context)
        {
#if PROFILE
            Device.Profiler.BeginFrame();
            Device.Profiler.Begin(Context, "Total");
            sceneRenderer.Profiler.BeginFrame();
            sceneRenderer.Profiler.Begin("Total");
#endif
#if PROFILE
            syncBarrier.SignalAndWait();
#endif
            if (resize)
            {
                swapChain.Resize(Width, Height);
                resize = false;
            }

            if (firstFrame)
            {
                Time.Initialize();
                firstFrame = false;
            }

            context.ClearDepthStencilView(swapChain.BackbufferDSV, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
            context.ClearRenderTargetView(swapChain.BackbufferRTV, Vector4.Zero);

            renderDispatcher.ExecuteQueue();

            imGuiRenderer?.NewFrame();
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

                DebugDraw.SetCamera(CameraManager.Current.Transform.ViewProjection);
            }

            swapChain.WaitForPresent();

            drawing &= SceneManager.Current is not null;

            if (drawing)
            {
                SceneManager.Current.GraphicsUpdate(context);
                sceneRenderer.Render(context, this, windowViewport, SceneManager.Current, CameraManager.Current);
            }

            Designer.Draw();
            WindowManager.Draw(context);
            ImGuiConsole.Draw();
            MessageBoxes.Draw();

            OnRender(context);

            if (Application.InEditorMode)
            {
#if PROFILE
                Device.Profiler.Begin(Context, "DebugDraw");
                sceneRenderer.Profiler.Begin("DebugDraw");
#endif
                debugDrawRenderer?.EndDraw();
#if PROFILE
                sceneRenderer.Profiler.End("DebugDraw");
                Device.Profiler.End(Context, "DebugDraw");
#endif
            }

#if PROFILE
            Device.Profiler.Begin(Context, "ImGui");
            sceneRenderer.Profiler.Begin("ImGui");
#endif
            context.SetRenderTarget(swapChain.BackbufferRTV, null);
            imGuiRenderer?.EndFrame();
#if PROFILE
            sceneRenderer.Profiler.End("ImGui");
            Device.Profiler.End(Context, "ImGui");
#endif

            OnRenderEnd(context);

            swapChain.Present();

            swapChain.Wait();

#if PROFILE
            sceneRenderer.Profiler.End("Total");
            Device.Profiler.End(Context, "Total");
            Device.Profiler.EndFrame(context);
#endif
            syncBarrier.SignalAndWait();
        }

        public virtual void Uninitialize()
        {
            running = false;
            OnRendererDispose();

            syncBarrier.RemoveParticipant();
            updateThread.Join();

            Device.Profiler.Dispose();

            if (Flags.HasFlag(RendererFlags.ImGuiWidgets))
            {
                WindowManager.Dispose();
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

        protected virtual void OnRenderEnd(IGraphicsContext context)
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