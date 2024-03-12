namespace UIApp
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.UI;
    using HexaEngine.UI.Controls;
    using HexaEngine.UI.Graphics;
    using HexaEngine.Windows;
    using System.Numerics;

    public class TestWindow : Window
    {
        protected UIRenderer uirenderer;
        protected UICommandList commandList;

        private UIWindow window;

        protected override void OnRendererInitialize(IGraphicsDevice device)
        {
            UISystem system = new();
            system.Load(device);
            UISystem.Current = system;

            uirenderer = new(graphicsDevice);
            commandList = new();

            window = new("", 1280, 720);
            Button button = new() { Content = "Test Button", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };
            window.Children.Add(button);
            window.Show();
        }

        /// <summary>
        /// Renders the content of the window using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        public override void Render(IGraphicsContext context)
        {
            window.Width = Width;
            window.Height = Height;
            commandList.BeginDraw();

            window.Draw(commandList);

            commandList.Transform = Matrix3x2.Identity;

            commandList.EndDraw();

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
                Time.ResetTime();
                resetTime = false;
            }

            // Clear depth-stencil and render target views.
            context.ClearDepthStencilView(swapChain.BackbufferDSV, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
            context.ClearRenderTargetView(swapChain.BackbufferRTV, new Vector4(0.10f, 0.10f, 0.10f, 1.00f));

            // Execute rendering commands from the render dispatcher.
            renderDispatcher.ExecuteQueue();

            // Invoke virtual method for pre-render operations.
            OnRenderBegin(context);

            // Determine if rendering should occur based on initialization status.
            var drawing = rendererInitialized;

            // Wait for swap chain presentation.
            swapChain.WaitForPresent();

            // Invoke virtual method for post-render operations.
            OnRender(context);

            // Set the render target to swap chain backbuffer.
            context.SetRenderTarget(swapChain.BackbufferRTV, null);

#if PROFILE
            // Begin profiling ImGui if profiling is enabled.
            Device.Profiler.Begin(Context, "ImGui");
            sceneRenderer.Profiler.Begin("ImGui");
#endif

            float L = 0;
            float R = Width;
            float T = 0;
            float B = Height;
            Matrix4x4 mvp = new
                (
                 2.0f / (R - L), 0.0f, 0.0f, 0.0f,
                 0.0f, 2.0f / (T - B), 0.0f, 0.0f,
                 0.0f, 0.0f, 0.5f, 0.0f,
                 (R + L) / (L - R), (T + B) / (B - T), 0.5f, 1.0f
                 );

            // End the ImGui frame rendering.
            uirenderer?.RenderDrawData(context, swapChain.Viewport, mvp, commandList);
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

        protected override void OnRendererDispose()
        {
            uirenderer.Release();
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