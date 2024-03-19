namespace UIApp
{
    using HexaEngine.Core;
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Threading;
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Mathematics;
    using HexaEngine.UI;
    using HexaEngine.UI.Controls;
    using HexaEngine.UI.Graphics;
    using System.Numerics;

    public sealed class TestWindow : CoreWindow
    {
        protected ThreadDispatcher renderDispatcher;
        protected UIRenderer uirenderer;
        protected UICommandList commandList;

        private UIWindow window;
        private IGraphicsContext graphicsContext;
        private ISwapChain swapChain;
        private bool resetTime;
        private bool resize;

        public override Viewport RenderViewport { get; }
        public override Viewport WindowViewport { get; }

        public override void Initialize(IAudioDevice audioDevice, IGraphicsDevice graphicsDevice)
        {
            base.Initialize(audioDevice, graphicsDevice);
            graphicsContext = graphicsDevice.Context;
            swapChain = SwapChain;
            renderDispatcher = Dispatcher;

            UISystem system = new();
            system.Load(graphicsDevice);
            UISystem.Current = system;

            uirenderer = new(graphicsDevice);
            commandList = new();

            window = new("", 1280, 720);

            Label label = new() { Content = "Align Left Top", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Padding = new(5), Margin = new(0, 0, 0, 0) };
            Grid.SetColumn(label, 1);
            Grid.SetRow(label, 0);

            Label label1 = new() { Content = "Align Centered", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Padding = new(5), Margin = new(0, 0, 0, 0) };
            Grid.SetColumn(label1, 1);
            Grid.SetRow(label1, 0);

            Label label2 = new() { Content = "Align Right Top", HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top, Padding = new(5), Margin = new(0, 0, 0, 0) };
            Grid.SetColumn(label2, 1);
            Grid.SetRow(label2, 0);

            Label label3 = new() { Content = "Align Right Bottom", HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom, Padding = new(5), Margin = new(0, 0, 0, 0) };
            Grid.SetColumn(label3, 1);
            Grid.SetRow(label3, 0);

            Label label4 = new() { Content = "Align Left Bottom", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Bottom, Padding = new(5), Margin = new(0, 0, 0, 0) };
            Grid.SetColumn(label4, 1);
            Grid.SetRow(label4, 0);

            Label label5 = new() { Content = "Align Top Center", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top, Padding = new(5), Margin = new(0, 0, 0, 0) };
            Grid.SetColumn(label5, 1);
            Grid.SetRow(label5, 0);

            Label label6 = new() { Content = "Align Bottom Center", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom, Padding = new(5), Margin = new(0, 0, 0, 0) };
            Grid.SetColumn(label6, 1);
            Grid.SetRow(label6, 0);

            Label label7 = new() { Content = "Align Center Left", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Padding = new(5), Margin = new(0, 0, 0, 0) };
            Grid.SetColumn(label7, 1);
            Grid.SetRow(label7, 0);

            Label label8 = new() { Content = "Align Center Right", HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center, Padding = new(5), Margin = new(0, 0, 0, 0) };
            Grid.SetColumn(label8, 1);
            Grid.SetRow(label8, 0);

            Label label9 = new() { Content = "Margin Test", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Padding = new(5), Margin = new(0, 0, 0, 0) };
            Grid.SetColumn(label9, 0);
            Grid.SetRow(label9, 3);

            Label label10 = new() { Content = "Label 20px 30px 00px 00px", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, BorderThickness = default, Padding = default, Margin = new(20, 30, 00, 00) };
            Grid.SetColumn(label10, 0);
            Grid.SetRow(label10, 3);

            Label label11 = new() { Content = "Padding Test", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Padding = new(5), Margin = new(0, 0, 0, 0) };
            Grid.SetColumn(label11, 1);
            Grid.SetRow(label11, 3);

            Label label12 = new() { Content = "Label 20px 30px 10px 40px", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, BorderThickness = default, Padding = new(20, 30, 10, 40), Margin = new(0, 0, 0, 0), Border = UIFactory.CreateSolidColorBrush(Colors.Gray) };
            Grid.SetColumn(label12, 1);
            Grid.SetRow(label12, 3);

            Label label13 = new() { Content = "Border Test", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Padding = new(5), Margin = new(0, 0, 0, 0) };
            Grid.SetColumn(label13, 2);
            Grid.SetRow(label13, 3);

            Label label14 = new() { Content = "Label 20px 30px 10px 40px", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, BorderThickness = new(20, 30, 10, 40), Padding = default, Margin = new(0, 0, 0, 0), Border = UIFactory.CreateSolidColorBrush(Colors.Gray) };
            Grid.SetColumn(label14, 2);
            Grid.SetRow(label14, 3);

            Label font0 = new() { Content = "Font 30px", Margin = new(0, 0, 0, 0), Border = UIFactory.CreateSolidColorBrush(Colors.Gray), FontSize = 30 };
            Label font1 = new() { Content = "Font 20px", Margin = new(0, 0, 0, 0), Border = UIFactory.CreateSolidColorBrush(Colors.Gray), FontSize = 20 };
            Label font2 = new() { Content = "Font 14px", Margin = new(0, 0, 0, 0), Border = UIFactory.CreateSolidColorBrush(Colors.Gray), FontSize = 14 };
            Label font3 = new() { Content = "Font 10px", Margin = new(0, 0, 0, 0), Border = UIFactory.CreateSolidColorBrush(Colors.Gray), FontSize = 10 };

            Label fontType0 = new() { Content = "Arial", Margin = new(0, 0, 0, 0), Border = UIFactory.CreateSolidColorBrush(Colors.Gray), FontSize = 30, FontFamilyName = "Arial" };
            Label fontType1 = new() { Content = "Calibri", Margin = new(0, 0, 0, 0), Border = UIFactory.CreateSolidColorBrush(Colors.Gray), FontSize = 30, FontFamilyName = "Calibri" };
            Label fontType2 = new() { Content = "Bradley Hand ITC", Margin = new(0, 0, 0, 0), Border = UIFactory.CreateSolidColorBrush(Colors.Gray), FontSize = 30, FontFamilyName = "Bradley Hand ITC" };

            StackPanel panel = new();
            panel.Children.Add(font0);
            panel.Children.Add(font1);
            panel.Children.Add(font2);
            panel.Children.Add(font3);
            panel.Children.Add(fontType0);
            panel.Children.Add(fontType1);
            panel.Children.Add(fontType2);

            Button buttonWide = new() { Content = "Grid Column Span 2", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center, BorderThickness = new(10), Padding = new(5) };
            Grid.SetColumn(buttonWide, 0);
            Grid.SetColumnSpan(buttonWide, 2);
            Grid.SetRow(buttonWide, 1);

            Button buttonLONG = new() { Content = "Grid Row Span 2", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Stretch, BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };
            Grid.SetColumn(buttonLONG, 2);
            Grid.SetRow(buttonLONG, 0);
            Grid.SetRowSpan(buttonLONG, 2);

            Button buttonS = new() { Content = "Stack Panel Item 1", BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };
            Button buttonS1 = new() { Content = "Stack Panel Item 2", BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };
            Button buttonS2 = new() { Content = "Stack Panel Item 3", BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };

            StackPanel stackPanel = new() { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
            stackPanel.Children.Add(buttonS);
            stackPanel.Children.Add(buttonS1);
            stackPanel.Children.Add(buttonS2);

            Grid grid = new() { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
            //grid.Border = UIFactory.CreateSolidColorBrush(Colors.LightBlue);
            grid.Background = UIFactory.CreateSolidColorBrush(Colors.Beige);
            grid.RowDefinitions.Add(new());
            grid.RowDefinitions.Add(new());
            grid.RowDefinitions.Add(new(new(150, GridUnitType.Auto)));
            grid.RowDefinitions.Add(new(new(150, GridUnitType.Pixel)));
            grid.ColumnDefinitions.Add(new());
            grid.ColumnDefinitions.Add(new());
            grid.ColumnDefinitions.Add(new());
            grid.Children.Add(label);
            grid.Children.Add(label1);
            grid.Children.Add(label2);
            grid.Children.Add(label3);
            grid.Children.Add(label4);
            grid.Children.Add(label5);
            grid.Children.Add(label6);
            grid.Children.Add(label7);
            grid.Children.Add(label8);
            grid.Children.Add(label9);
            grid.Children.Add(label10);
            grid.Children.Add(label11);
            grid.Children.Add(label12);
            grid.Children.Add(label14);
            grid.Children.Add(label13);

            grid.Children.Add(stackPanel);
            grid.Children.Add(panel);
            grid.Children.Add(buttonWide);
            grid.Children.Add(buttonLONG);

            window.Content = grid;

            /*
            Grid grid = new() { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
            grid.RowDefinitions.Add(new());
            grid.RowDefinitions.Add(new());
            grid.RowDefinitions.Add(new(new(100, GridUnitType.Pixel)));
            grid.ColumnDefinitions.Add(new(new(0, GridUnitType.Auto)));
            grid.ColumnDefinitions.Add(new());
            grid.ColumnDefinitions.Add(new());

            Label label0 = new() { Text = "Test Label", BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };
            label0.GridColumn = 0;
            label0.GridRow = 0;

            Label label1 = new() { Text = "Test Label 1", BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };
            label1.GridColumn = 1;
            label1.GridRow = 1;

            Label label2 = new() { Text = "Test Label 2", BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };
            label2.GridColumn = 2;
            label2.GridRow = 2;

            grid.Children.Add(label0);
            grid.Children.Add(label1);
            grid.Children.Add(label2);
            window.Children.Add(grid);
            */

            window.Show();

            Show();
        }

        /// <summary>
        /// Renders the content of the window using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        public override void Render(IGraphicsContext context)
        {
            commandList.BeginDraw();

            window.Render(commandList);

            commandList.Transform = Matrix3x2.Identity;

            commandList.EndDraw();

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

            // Wait for swap chain presentation.
            swapChain.WaitForPresent();

            // Set the render target to swap chain backbuffer.
            context.SetRenderTarget(swapChain.BackbufferRTV, null);

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
                window.Width = Width;
                window.Height = Height;
            }

            resize = true;
            base.OnResized(args);
        }

        protected override void DisposeCore()
        {
            HexaEngine.Web.HttpClientExtensions.WebCache.Save();
            uirenderer.Release();
        }
    }
}