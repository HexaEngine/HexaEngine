namespace UIApp
{
    using HexaEngine;
    using HexaEngine.Core;
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Threading;
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Mathematics;
    using HexaEngine.UI;
    using HexaEngine.UI.Graphics;
    using HexaEngine.UI.Markup;
    using System.Numerics;

    public sealed class TestWindow : CoreWindow
    {
        private ThreadDispatcher renderDispatcher;
        private UIRenderer uirenderer;
        private UICommandList commandList;
        private Texture2D compositionTexture;

        private UIWindow window;
        private IGraphicsDevice graphicsDevice;
        private IGraphicsContext graphicsContext;
        private ISwapChain swapChain;
        private bool resetTime;
        private bool resize;

        public override Viewport RenderViewport { get; }

        public override Viewport WindowViewport { get; }

        public override void Initialize(IAudioDevice audioDevice, IGraphicsDevice graphicsDevice)
        {
            base.Initialize(audioDevice, graphicsDevice);
            this.graphicsDevice = graphicsDevice;
            graphicsContext = graphicsDevice.Context;
            swapChain = SwapChain;
            renderDispatcher = (ThreadDispatcher)Dispatcher;

            swapChain.VSync = true;

            UISystem system = new();
            system.Load(graphicsDevice);
            UISystem.Current = system;

            uirenderer = new(graphicsDevice);
            commandList = new();

            compositionTexture = new(swapChain.Backbuffer.Description.Format, Width, Height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);

            MakeUI();

            Show();
        }

        private void MakeUI()
        {
            XamlReader reader = new();
            window = (UIWindow)reader.Parse("Test.xaml");

            Width = (int)window.Width;
            Height = (int)window.Height;
            window.Show();
        }

        /// <summary>
        /// Renders the content of the window using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        public override void Render(IGraphicsContext context)
        {
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

            commandList.BeginDraw();

            window.Render(commandList);

            commandList.Transform = Matrix3x2.Identity;

            commandList.EndDraw();

            // Clear render target view.
            context.ClearRenderTargetView(compositionTexture.RTV, default);

            // Execute rendering commands from the render dispatcher.
            renderDispatcher.ExecuteQueue();

            // Wait for swap chain presentation.
            swapChain.WaitForPresent();

            // Set the render target to swap chain backbuffer.
            context.SetRenderTarget(compositionTexture.RTV, null);

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

            context.CopyResource(swapChain.Backbuffer, compositionTexture);

            // Present and swap buffers.
            swapChain.Present();

            // Wait for swap chain presentation to complete.
            swapChain.Wait();
        }

        /// <summary>
        /// Raises the <see cref="HexaEngine.Core.Windows.SdlWindow.Resized" /> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected override void OnResized(ResizedEventArgs args)
        {
            if (window != null)
            {
                window.Width = args.NewWidth;
                window.Height = args.NewHeight;
                compositionTexture.Resize(swapChain.Backbuffer.Description.Format, args.NewWidth, args.NewHeight, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            }

            resize = true;
            base.OnResized(args);
        }

        protected override void DisposeCore()
        {
            uirenderer.Release();
        }
    }
}