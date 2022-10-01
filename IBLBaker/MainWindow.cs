namespace IBLBaker
{
    using HexaEngine.Core;
    using HexaEngine.Core.Events;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Editor;
    using HexaEngine.Rendering;
    using HexaEngine.Scenes;
    using IBLBaker.Widgets;
    using System;
    using System.Diagnostics;
    using System.Numerics;

    public class MainWindow : SdlWindow
    {
        private Thread renderThread;
        private bool isRunning = true;
        private bool firstFrame;

        private bool resize = false;
        private ImGuiRenderer renderer;

        protected override void OnShown(ShownEventArgs args)
        {
            renderThread = new(RenderVoid);
            renderThread.Name = "RenderThread";
            renderThread.Start();
            base.OnShown(args);
        }

        [STAThread]
        private void RenderVoid()
        {
            IGraphicsDevice device;
            IGraphicsContext context;
            ISwapChain swapChain;
            if (OperatingSystem.IsWindows())
            {
                device = Adapter.CreateGraphics(RenderBackend.D3D11, this);
                context = device.Context;
                swapChain = device.SwapChain ?? throw new PlatformNotSupportedException();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            renderer = new(this, device);

            SceneManager.SceneChanged += (_, _) => { firstFrame = true; };
            Time.Initialize();

            WidgetManager.Init(device);
            while (isRunning)
            {
                context.ClearDepthStencilView(swapChain.BackbufferDSV, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
                context.ClearRenderTargetView(swapChain.BackbufferRTV, Vector4.Zero);
                Time.FrameUpdate();
                Dispatcher.ExecuteQueue();
                renderer.BeginDraw();
                WidgetManager.Draw(context);

                if (resize)
                {
                    device.SwapChain.Resize(Width, Height);
                    resize = false;
                }

                if (firstFrame)
                {
                    Time.Initialize();
                    firstFrame = false;
                }

                renderer.EndDraw();
                swapChain.Present(Nucleus.Settings.VSync ? 1u : 0u);
                LimitFrameRate();
                Keyboard.FrameUpdate();
            }

            WidgetManager.Dispose();
            renderer.Dispose();
            Trace.WriteLine("Perfoming Shutdown");
            context.Dispose();
            device.Dispose();
        }

        private long fpsFrameCount;
        private long fpsStartTime;

        private void LimitFrameRate()
        {
            if (Nucleus.Settings.LimitFPS & !Nucleus.Settings.VSync)
            {
                int fps = Nucleus.Settings.TargetFPS;
                long freq = Stopwatch.Frequency;
                long frame = Stopwatch.GetTimestamp();
                while ((frame - fpsStartTime) * fps < freq * fpsFrameCount)
                {
                    int sleepTime = (int)((fpsStartTime * fps + freq * fpsFrameCount - frame * fps) * 1000 / (freq * fps));
                    if (sleepTime > 0) Thread.Sleep(sleepTime);
                    frame = Stopwatch.GetTimestamp();
                }
                if (++fpsFrameCount > fps)
                {
                    fpsFrameCount = 0;
                    fpsStartTime = frame;
                }
            }
        }

        protected override void OnResized(ResizedEventArgs args)
        {
            resize = true;
            base.OnResized(args);
        }

        protected override void OnClose(CloseEventArgs args)
        {
            isRunning = false;
            renderThread.Join();
            base.OnClose(args);
        }
    }
}