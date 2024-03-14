namespace UIApp
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Mathematics;
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

            Label label = new() { Text = "Label Left Top", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };
            label.GridColumn = 1;
            label.GridRow = 0;

            Label label1 = new() { Text = "Label Centered", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };
            label1.GridColumn = 1;
            label1.GridRow = 0;

            Label label2 = new() { Text = "Label Right Top", HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top, BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };
            label2.GridColumn = 1;
            label2.GridRow = 0;

            Label label3 = new() { Text = "Label Right Bottom", HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom, BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };
            label3.GridColumn = 1;
            label3.GridRow = 0;

            Label label4 = new() { Text = "Label Left Bottom", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Bottom, BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };
            label4.GridColumn = 1;
            label4.GridRow = 0;

            Label label5 = new() { Text = "Label Top Center", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top, BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };
            label5.GridColumn = 1;
            label5.GridRow = 0;

            Label label6 = new() { Text = "Label Bottom Center", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom, BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };
            label6.GridColumn = 1;
            label6.GridRow = 0;

            Label label7 = new() { Text = "Label Center Left", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };
            label7.GridColumn = 1;
            label7.GridRow = 0;

            Label label8 = new() { Text = "Label Center Right", HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center, BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };
            label8.GridColumn = 1;
            label8.GridRow = 0;

            Label label9 = new() { Text = "Margin Test", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };
            label9.GridColumn = 0;
            label9.GridRow = 3;

            Label label10 = new() { Text = "Label 20px 30px 00px 00px", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, BorderThickness = default, Padding = default, Margin = new(20, 30, 00, 00) };
            label10.GridColumn = 0;
            label10.GridRow = 3;

            Label label11 = new() { Text = "Padding Test", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };
            label11.GridColumn = 1;
            label11.GridRow = 3;

            Label label12 = new() { Text = "Label 20px 30px 10px 40px", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, BorderThickness = default, Padding = new(20, 30, 10, 40), Margin = new(0, 0, 0, 0), Border = UIFactory.CreateSolidColorBrush(Colors.Gray) };
            label12.GridColumn = 1;
            label12.GridRow = 3;

            Label label13 = new() { Text = "Border Test", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };
            label13.GridColumn = 2;
            label13.GridRow = 3;

            Label label14 = new() { Text = "Label 20px 30px 10px 40px", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, BorderThickness = new(20, 30, 10, 40), Padding = default, Margin = new(0, 0, 0, 0), Border = UIFactory.CreateSolidColorBrush(Colors.Gray) };
            label14.GridColumn = 2;
            label14.GridRow = 3;

            Button buttonWide = new() { Content = "Column Span 2", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center, BorderThickness = new(10), Padding = new(5) };
            buttonWide.GridColumn = 0;
            buttonWide.GridColumnSpan = 2;
            buttonWide.GridRow = 1;

            Button buttonLONG = new() { Content = "Row Span 2", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Stretch, BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };
            buttonLONG.GridColumn = 2;
            buttonLONG.GridRow = 0;
            buttonLONG.GridRowSpan = 2;

            Button buttonS = new() { Content = "Stack Panel Item 1", BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };
            Button buttonS1 = new() { Content = "Stack Panel Item 2", BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };
            Button buttonS2 = new() { Content = "Stack Panel Item 3", BorderThickness = new(10), Padding = new(5), Margin = new(0, 0, 0, 0) };

            StackPanel stackPanel = new() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            stackPanel.Children.Add(buttonS);
            stackPanel.Children.Add(buttonS1);
            stackPanel.Children.Add(buttonS2);

            Grid grid = new() { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
            grid.Border = UIFactory.CreateSolidColorBrush(Colors.LightBlue);
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
            grid.Children.Add(buttonWide);
            grid.Children.Add(buttonLONG);

            window.Children.Add(grid);

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
            HexaEngine.Web.HttpClientExtensions.WebCache.Save();
            uirenderer.Release();
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
    }
}