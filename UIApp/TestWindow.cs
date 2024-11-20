namespace UIApp
{
    using Hexa.NET.Mathematics;
    using Hexa.NET.Utilities.Threading;
    using HexaEngine;
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.UI;
    using HexaEngine.UI.Animation;
    using HexaEngine.UI.Controls;
    using HexaEngine.UI.Graphics;
    using HexaEngine.UI.Markup;
    using HexaEngine.Windows;
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

            //swapChain.VSync = true;
            swapChain.LimitFPS = true;
            swapChain.TargetFPS = 165;

            UISystem system = new();
            system.Awake(null!);
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
            window = new UIWindow();
            window.Width = Width;
            window.Height = Height;

            window.SetInputTransform(Matrix3x2.CreateTranslation(-new Vector2(X, Y)));

            var grid = new Grid();

            grid.ColumnDefinitions.Add(new(new(1, GridUnitType.Star)));
            grid.ColumnDefinitions.Add(new(new(float.NaN, GridUnitType.Auto)));
            grid.ColumnDefinitions.Add(new(new(1, GridUnitType.Star)));

            grid.RowDefinitions.Add(new(new(1, GridUnitType.Star)));
            grid.RowDefinitions.Add(new(new(float.NaN, GridUnitType.Auto)));
            grid.RowDefinitions.Add(new(new(1, GridUnitType.Star)));

            StackPanel panel = new();

            TextBox textBox = new()
            {
                FontSize = 30,
                MinWidth = 200,
                MinHeight = 30,
                AcceptsReturn = true,
                Background = new SolidColorBrush(new(0x3c3c3cFF)),
                Foreground = new SolidColorBrush(new(0xFFFFFFFF)),
            };
            panel.Children.Add(textBox);

            Grid.SetColumn(panel, 1);
            Grid.SetRow(panel, 1);
            grid.Children.Add(panel);

            window.Content = grid;

            //  window.Background = new SolidColorBrush(Colors.Black);
            window.Show();
            window.OnInvalidateVisual += OnInvalidateVisual;
            window.InvalidateMeasure();
        }

        private bool invalidate = true;

        private void OnInvalidateVisual(object? sender, EventArgs e)
        {
            invalidate = true;
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

            // Execute rendering commands from the render dispatcher.
            renderDispatcher.ExecuteQueue();

            AnimationScheduler.Tick();

            if (invalidate)
            {
                commandList.BeginDraw();

                window.Render(commandList);

                commandList.Transform = Matrix3x2.Identity;

                commandList.EndDraw();

                invalidate = false;

                // Clear render target view.
                context.ClearRenderTargetView(compositionTexture.RTV, default);

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
            }

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
                window.InvalidateArrange();
            }

            resize = true;
            base.OnResized(args);
        }

        protected override void OnMoved(MovedEventArgs args)
        {
            window.SetInputTransform(Matrix3x2.CreateTranslation(-new Vector2(args.NewX, args.NewY)));
            base.OnMoved(args);
        }

        protected override void DisposeCore()
        {
            uirenderer.Release();
        }
    }
}