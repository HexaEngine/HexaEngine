namespace Editor
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Editor;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using HexaEngine.Windows;
    using System.Numerics;

    /// <summary>
    /// Represents the application window that implements the <see cref="IRenderWindow"/> interface.
    /// </summary>
    public class EditorWindow : Window, IRenderWindow
    {
        protected Frameviewer frameviewer;
#nullable disable

        /// <summary>
        /// Initializes a new instance of the <see cref="Window"/> class.
        /// </summary>
        public EditorWindow()
        {
            Flags = RendererFlags.All;
            Title = "Editor";
        }

        private bool firstTime = true;
        private readonly EditorConfig config = EditorConfig.Load();

        protected override void OnShown(ShownEventArgs args)
        {
            if (firstTime)
            {
                X = config.X;
                Y = config.Y;
                Width = config.Width;
                Height = config.Height;
                State = config.State;
                firstTime = false;
            }
            base.OnShown(args);
        }

        protected override void OnClose(CloseEventArgs args)
        {
            config.X = X;
            config.Y = Y;
            config.Width = Width;
            config.Height = Height;
            config.State = State;
            config.Save();
            base.OnClose(args);
        }

#nullable restore

        protected override void OnRendererInitialize(IGraphicsDevice device)
        {
            Designer.Init(graphicsDevice);
            frameviewer = new(graphicsDevice);

            // Initialize WindowManager if ImGuiWidgets flag is set
            if ((Flags & RendererFlags.ImGuiWidgets) != 0)
            {
                WindowManager.Init(graphicsDevice);
            }
        }

        /// <summary>
        /// Renders the content of the window using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        public override void Render(IGraphicsContext context)
        {
#if PROFILE
            // Begin profiling frame and total time if profiling is enabled.
            Device.Profiler.BeginFrame();
            Device.Profiler.Begin(Context, "Total");
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
                Time.Initialize();
                resetTime = false;
            }

            // Clear depth-stencil and render target views.
            context.ClearDepthStencilView(swapChain.BackbufferDSV, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
            context.ClearRenderTargetView(swapChain.BackbufferRTV, Vector4.Zero);

            // Execute rendering commands from the render dispatcher.
            renderDispatcher.ExecuteQueue();

            // Start ImGui frame rendering.
            imGuiRenderer?.NewFrame();

            // Invoke virtual method for pre-render operations.
            OnRenderBegin(context);

            // Determine if rendering should occur based on initialization status.
            var drawing = rendererInitialized;

            // Check if ImGui widgets should be rendered and the application is in editor mode.
            if (imGuiWidgets && Application.InEditorMode)
            {
                // Update and draw the frame viewer.
                frameviewer.SourceViewport = Viewport;
                frameviewer.Update();
                frameviewer.Draw();
                drawing &= frameviewer.IsVisible;
                windowViewport = Application.InEditorMode ? frameviewer.RenderViewport : Viewport;

                // Set the camera for DebugDraw based on the current camera's view projection matrix.

#nullable disable // cant be null because CameraManager.Current would be the editor camera because of Application.InEditorMode.
                DebugDraw.SetCamera(CameraManager.Current.Transform.ViewProjection);
#nullable restore
            }

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
                sceneRenderer.Render(context, this, windowViewport, SceneManager.Current, CameraManager.Current);
            }

            // Draw additional elements like Designer, WindowManager, ImGuiConsole, MessageBoxes, etc.
            Designer.Draw();
            WindowManager.Draw(context);
            ImGuiConsole.Draw();
            MessageBoxes.Draw();

            // Invoke virtual method for post-render operations.
            OnRender(context);

            // Set the render target to swap chain backbuffer.
            context.SetRenderTarget(swapChain.BackbufferRTV, null);

#if PROFILE
            // Begin profiling ImGui if profiling is enabled.
            Device.Profiler.Begin(Context, "ImGui");
            sceneRenderer.Profiler.Begin("ImGui");
#endif
            // End the ImGui frame rendering.
            imGuiRenderer?.EndFrame();
#if PROFILE
            // End profiling ImGui if profiling is enabled.
            sceneRenderer.Profiler.End("ImGui");
            Device.Profiler.End(Context, "ImGui");
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
            Device.Profiler.End(Context, "Total");
            Device.Profiler.EndFrame(context);
#endif
        }

        /// <summary>
        /// Uninitializes the window, releasing resources and stopping rendering.
        /// </summary>
        public override void Uninitialize()
        {
            base.Uninitialize();
            // Dispose of resources related to ImGui widgets if enabled.
            if (Flags.HasFlag(RendererFlags.ImGuiWidgets))
            {
                WindowManager.Dispose();
            }
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