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
    using System;
    using System.Numerics;
    using CullingManager = CullingManager;

    public enum RendererFlags
    {
        None = 0,
        SceneGraph = 1,
        DebugDraw = 2,
        ImGui = 4,
        ImGuizmo = 8,
        ImGuiWidgets = 16,
        ImGuiViewport = 32,
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

        public Viewport RenderViewport => renderViewport;

        public ISceneRenderer Renderer => deferredRenderer;

#pragma warning disable CS8618 // Non-nullable field 'renderDispatcher' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS8618 // Non-nullable field 'context' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS8618 // Non-nullable field 'frameviewer' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS8618 // Non-nullable field 'deferredRenderer' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS8618 // Non-nullable field 'initTask' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS8618 // Non-nullable field 'swapChain' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS8618 // Non-nullable field 'device' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.

        public Window()
#pragma warning restore CS8618 // Non-nullable field 'device' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning restore CS8618 // Non-nullable field 'swapChain' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning restore CS8618 // Non-nullable field 'initTask' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning restore CS8618 // Non-nullable field 'deferredRenderer' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning restore CS8618 // Non-nullable field 'frameviewer' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning restore CS8618 // Non-nullable field 'context' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning restore CS8618 // Non-nullable field 'renderDispatcher' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        {
        }

        public void RenderInitialize(IGraphicsDevice device)
        {
            if (OperatingSystem.IsWindows())
            {
                this.device = device;
                context = device.Context;
                swapChain = device.CreateSwapChain(this) ?? throw new PlatformNotSupportedException();
                swapChain.Active = true;
                renderDispatcher = new(device, Thread.CurrentThread);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            if (Application.MainWindow == this)
            {
                AudioManager.Initialize();
                ResourceManager.Initialize(device);
                PipelineManager.Initialize(device);
                CullingManager.Initialize(device);
                ObjectPickerManager.Initialize(device, Width, Height);
            }

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

            OnRendererInitialize(device);

            if (sceneGraph)
            {
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
#pragma warning disable CS8604 // Possible null reference argument for parameter 'e' in 'void ImGuiConsole.Log(Exception e)'.
                        ImGuiConsole.Log(x.Exception);
#pragma warning restore CS8604 // Possible null reference argument for parameter 'e' in 'void ImGuiConsole.Log(Exception e)'.
                    }
                });
            }

            if (StartupScene != null)
            {
                Task.Run(() => SceneManager.AsyncLoad(StartupScene)).ContinueWith(x => SceneManager.Current.IsSimulating = true);
            }
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

            var drawing = sceneGraph && initTask.IsCompleted && SceneManager.Current is not null;

            if (drawing)
#pragma warning disable CS8602 // Dereference of a possibly null reference.
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
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            OnRender(context);

            renderer?.EndDraw();

            swapChain.Present();
            if (drawing)
                deferredRenderer.Profiler.End(deferredRenderer);
            swapChain.Wait();
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
            if (sceneGraph)
                deferredRenderer.Dispose();
            renderDispatcher.Dispose();
            ObjectPickerManager.Release();
            CullingManager.Release();
            ResourceManager.Dispose();
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