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
    using YamlDotNet.Core;

    public sealed class TestWindow : CoreWindow
    {
        private ThreadDispatcher renderDispatcher;
        private UIRenderer uirenderer;
        private UICommandList commandList;

        private IGraphicsDevice graphicsDevice;
        private IGraphicsContext graphicsContext;
        private ISwapChain swapChain;
        private bool resetTime;
        private bool resize;

        private ParticleSystem particleSystem = new();
        private Emitter emitter = new();

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

            Show();
        }

        private SolidColorBrush brush = new(Colors.AliceBlue);

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

            particleSystem.Update(emitter);

            emitter.Position = new Vector2(Width / 2, Height / 2);
            emitter.Boundaries = new(0, 0, Width, Height);
            particleSystem.Emit(emitter);
            particleSystem.Simulate(emitter, Time.Delta);
            commandList.PushClipRect(new(0, 0, Width, Height));
            particleSystem.Draw(commandList, brush);
            commandList.PopClipRect();
            commandList.Transform = Matrix3x2.Identity;

            commandList.EndDraw();

            // Execute rendering commands from the render dispatcher.
            renderDispatcher.ExecuteQueue();

            // Wait for swap chain presentation.
            swapChain.WaitForPresent();

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

            context.ClearRenderTargetView(swapChain.BackbufferRTV, default);
            context.SetRenderTarget(swapChain.BackbufferRTV, null);

            // End the ImGui frame rendering.
            uirenderer?.RenderDrawData(context, swapChain.Viewport, mvp, commandList);

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
            resize = true;
            base.OnResized(args);
        }

        protected override void DisposeCore()
        {
            uirenderer.Release();
        }
    }
}