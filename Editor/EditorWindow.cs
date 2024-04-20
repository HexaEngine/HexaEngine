#define PROFILE
//#define SINGLE_THREADED

namespace Editor
{
    using HexaEngine;
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Editor;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using HexaEngine.Windows;
    using System.Numerics;

    /// <summary>
    /// Represents the application window that implements the <see cref="ICoreWindow"/> interface.
    /// </summary>
    public sealed class EditorWindow : Window
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(Editor));
        private ImGuiManager imGuiRenderer;
        private Task initEditorTask;
        private bool editorInitialized;
#nullable disable

        /// <summary>
        /// Initializes a new instance of the <see cref="Window"/> class.
        /// </summary>
        public EditorWindow()
        {
            Flags = RendererFlags.None;
            Title = "Editor";
        }

        private bool firstTime = true;
        private readonly EditorConfig config = EditorConfig.Default;

        protected override void OnShown(ShownEventArgs args)
        {
            if (firstTime)
            {
                X = config.X;
                Y = config.Y;
                Width = config.Width;
                Height = config.Height;
                if (!config.SetupDone)
                {
                    State = WindowState.Maximized;
                }
                if (config.State == WindowState.Normal || config.State == WindowState.Maximized)
                {
                    State = config.State;
                }

                firstTime = false;
            }
            base.OnShown(args);
        }

        protected override void OnClosed(CloseEventArgs args)
        {
            config.X = X;
            config.Y = Y;
            config.Width = Width;
            config.Height = Height;
            config.State = State;
            config.Save();
            base.OnClosed(args);
        }

#nullable restore

        protected override void OnRendererInitialize(IGraphicsDevice device)
        {
            imGuiRenderer = new(this, graphicsDevice, graphicsContext);

            initEditorTask = Task.Factory.StartNew(() =>
            {
                Designer.Init(graphicsDevice);
            });
            initEditorTask.ContinueWith(x =>
            {
                if (x.IsCompletedSuccessfully)
                {
                    LoggerFactory.General.Info("Editor: Initialized");
                }
                if (x.IsFaulted)
                {
                    LoggerFactory.General.Error("Editor: Failed Initialize");
                    LoggerFactory.General.Log(x.Exception);
                }

                editorInitialized = true;
            });
        }

#if SINGLE_THREADED

        protected override void UpdateScene()
        {
        }

#endif

        /// <summary>
        /// Raises the <see cref="E:HexaEngine.Core.Windows.SdlWindow.Resized" /> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected override void OnResized(ResizedEventArgs args)
        {
            resize = true;
            base.OnResized(args);
        }

        /// <summary>
        /// Renders the content of the window using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        public override void Render(IGraphicsContext context)
        {
            signal.Set();

#if PROFILE
            // Begin profiling frame and total time if profiling is enabled.
            Application.GPUProfiler.BeginFrame();
            Application.GPUProfiler.Begin(GraphicsContext, "Total");
            CPUProfiler2.Global.BeginFrame();
            CPUProfiler2.Global.Begin("Total");
#endif

            // Resize the swap chain if necessary.
            if (resize)
            {
                swapChain.Resize(Width, Height);
                resize = false;
            }

            // Initialize time if requested.
            if (resetTime)
            {
                Time.ResetTime();
                resetTime = false;
            }

            CPUProfiler2.Global.Begin("SwapChain.Clear");
            // Clear depth-stencil and render target views.
            context.ClearDepthStencilView(swapChain.BackbufferDSV, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
            context.ClearRenderTargetView(swapChain.BackbufferRTV, new Vector4(0.10f, 0.10f, 0.10f, 1.00f));
            CPUProfiler2.Global.End("SwapChain.Clear");

            CPUProfiler2.Global.Begin("Dispatcher.Execute");
            // Execute rendering commands from the render dispatcher.
            renderDispatcher.ExecuteQueue();
            CPUProfiler2.Global.End("Dispatcher.Execute");

            CPUProfiler2.Global.Begin("ImGui.NewFrame");
            // Start ImGui frame rendering.
            imGuiRenderer?.NewFrame();
            CPUProfiler2.Global.End("ImGui.NewFrame");

            // Invoke virtual method for pre-render operations.
            OnRenderBegin(context);

            // Determine if rendering should occur based on initialization status.
            var drawing = rendererInitialized;

            CPUProfiler2.Global.Begin("Editor.Update");
            // Update and draw the frame viewer.
            SceneWindow.SourceViewport = Viewport;
            SceneWindow.Update();
            SceneWindow.Draw();
            drawing &= SceneWindow.IsVisible;
            windowViewport = Application.InEditorMode ? SceneWindow.RenderViewport : Viewport;
            CPUProfiler2.Global.End("Editor.Update");

            // Set the camera for DebugDraw based on the current camera's view projection matrix.

            DebugDraw.SetCamera(CameraManager.Current?.Transform.ViewProjection ?? Matrix4x4.Identity);

            // Wait for swap chain presentation.
            swapChain.WaitForPresent();

            IScene? scene = SceneManager.Current;

            // Render the scene if drawing is enabled.
            if (drawing && scene is not null)
            {
#if SINGLE_THREADED

                // Update the current scene
                scene.Update();

                // Do fixed update tick if necessary.
                Time.FixedUpdateTick();
#endif
                CPUProfiler2.Global.Begin("Scene.Update");
                scene.GraphicsUpdate(context);
                CPUProfiler2.Global.End("Scene.Update");

                CPUProfiler2.Global.Begin("Scene.Render");
                sceneRenderer.Render(context, windowViewport, scene, CameraManager.Current);
                CPUProfiler2.Global.End("Scene.Render");
            }

            // Draw additional elements like Designer, WindowManager, ImGuiConsole, MessageBoxes, etc.
            if (editorInitialized)
            {
                CPUProfiler2.Global.Begin("Editor");
                Designer.Draw(context);
                CPUProfiler2.Global.End("Editor");
            }

            // Invoke virtual method for post-render operations.
            OnRender(context);

            // Set the render target to swap chain backbuffer.
            context.SetRenderTarget(swapChain.BackbufferRTV, null);

#if PROFILE
            // Begin profiling ImGui if profiling is enabled.
            Application.GPUProfiler.Begin(GraphicsContext, "ImGui");
            CPUProfiler2.Global.Begin("ImGui");
#endif

            // End the ImGui frame rendering.
            imGuiRenderer?.EndFrame();
#if PROFILE
            // End profiling ImGui if profiling is enabled.
            CPUProfiler2.Global.End("ImGui");
            Application.GPUProfiler.End(GraphicsContext, "ImGui");
#endif
            // Invoke virtual method for post-render operations.
            OnRenderEnd(context);

            // Present and swap buffers.
            CPUProfiler2.Global.Begin("SwapChain.Present");
            swapChain.Present();
            CPUProfiler2.Global.End("SwapChain.Present");
            // Wait for swap chain presentation to complete.
            CPUProfiler2.Global.Begin("SwapChain.Wait");
            swapChain.Wait();
            CPUProfiler2.Global.End("SwapChain.Wait");
#if !SINGLE_THREADED
            // Signal and wait for synchronization with the update thread.
            syncBarrier.SignalAndWait();
#endif

#if PROFILE
            // End profiling frame and total time if profiling is enabled.
            CPUProfiler2.Global.End("Total");
            CPUProfiler2.Global.EndFrame();
            Application.GPUProfiler.End(GraphicsContext, "Total");
            Application.GPUProfiler.EndFrame(context);
#endif
        }

        protected override void OnRendererDispose()
        {
            if (!initEditorTask.IsCompleted)
            {
                initEditorTask.Wait();
            }
            imGuiRenderer.Dispose();
            Designer.Dispose();
        }
    }
}