﻿namespace Editor
{
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
    using Silk.NET.Direct3D11;
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

        /// <summary>
        /// Renders the content of the window using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        public override void Render(IGraphicsContext context)
        {
#if PROFILE
            // Begin profiling frame and total time if profiling is enabled.
            GraphicsDevice.Profiler.BeginFrame();
            GraphicsDevice.Profiler.Begin(GraphicsContext, "Total");
            sceneRenderer.Profiler.BeginFrame();
            sceneRenderer.Profiler.Begin("Total");
#endif
#if PROFILE
            // Signal and wait for synchronization if profiling is enabled.
            syncBarrier.SignalAndWait();
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

            // Clear depth-stencil and render target views.
            context.ClearDepthStencilView(swapChain.BackbufferDSV, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
            context.ClearRenderTargetView(swapChain.BackbufferRTV, new Vector4(0.10f, 0.10f, 0.10f, 1.00f));

            // Execute rendering commands from the render dispatcher.
            renderDispatcher.ExecuteQueue();

            // Start ImGui frame rendering.
            imGuiRenderer?.NewFrame();

            // Invoke virtual method for pre-render operations.
            OnRenderBegin(context);

            // Determine if rendering should occur based on initialization status.
            var drawing = rendererInitialized;

            // Update and draw the frame viewer.
            SceneWindow.SourceViewport = Viewport;
            SceneWindow.Update();
            SceneWindow.Draw();
            drawing &= SceneWindow.IsVisible;
            windowViewport = Application.InEditorMode ? SceneWindow.RenderViewport : Viewport;

            // Set the camera for DebugDraw based on the current camera's view projection matrix.

            DebugDraw.SetCamera(CameraManager.Current?.Transform.ViewProjection ?? Matrix4x4.Identity);

            // Wait for swap chain presentation.
            swapChain.WaitForPresent();

            // Check if rendering should occur based on the active scene.
            drawing &= SceneManager.Current is not null;

            // Render the scene if drawing is enabled.
            if (drawing)
            {
#nullable disable // a few lines above there is a null check.
                SceneManager.Current.GraphicsUpdate(context);
#nullable restore
                sceneRenderer.Render(context, windowViewport, SceneManager.Current, CameraManager.Current);
            }

            // Draw additional elements like Designer, WindowManager, ImGuiConsole, MessageBoxes, etc.
            if (editorInitialized)
            {
                Designer.Draw(context);
            }

            // Invoke virtual method for post-render operations.
            OnRender(context);

            // Set the render target to swap chain backbuffer.
            context.SetRenderTarget(swapChain.BackbufferRTV, null);

#if PROFILE
            // Begin profiling ImGui if profiling is enabled.
            GraphicsDevice.Profiler.Begin(GraphicsContext, "ImGui");
            sceneRenderer.Profiler.Begin("ImGui");
#endif
            // End the ImGui frame rendering.
            imGuiRenderer?.EndFrame();
#if PROFILE
            // End profiling ImGui if profiling is enabled.
            sceneRenderer.Profiler.End("ImGui");
            GraphicsDevice.Profiler.End(GraphicsContext, "ImGui");
#endif
            // Invoke virtual method for post-render operations.
            OnRenderEnd(context);

            // Present and swap buffers.
            swapChain.Present();

            // Wait for swap chain presentation to complete.
            swapChain.Wait();

            // Signal and wait for synchronization with the update thread.
            syncBarrier.SignalAndWait();

#if PROFILE
            // End profiling frame and total time if profiling is enabled.
            sceneRenderer.Profiler.End("Total");
            GraphicsDevice.Profiler.End(GraphicsContext, "Total");
            GraphicsDevice.Profiler.EndFrame(context);
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

        /// <summary>
        /// Raises the <see cref="E:HexaEngine.Core.Windows.SdlWindow.Resized" /> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected override void OnResized(ResizedEventArgs args)
        {
            resize = true;
            base.OnResized(args);
        }
    }
}