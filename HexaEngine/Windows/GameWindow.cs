#nullable disable

namespace HexaEngine.Windows
{
    using HexaEngine.Core;
    using HexaEngine.Core.Events;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Editor;
    using HexaEngine.Rendering;
    using HexaEngine.Scenes;
    using System;
    using System.Diagnostics;
    using System.Threading;

    [Obsolete]
    public class GameWindow : SdlWindow
    {
        private Thread renderThread;
        private bool isRunning = true;
        private bool firstFrame;
        private Dispatcher renderDispatcher;
        private IGraphicsDevice device;
        private IGraphicsContext context;
        private ISwapChain swapChain;
        private bool resize = false;
        private ImGuiRenderer renderer;
        private Framebuffer framebuffer;
        private DeferredRenderer deferredRenderer;

        public GameWindow()
        {
        }

        public Dispatcher RenderDispatcher => renderDispatcher;

        public IGraphicsDevice Device => device;
        public IGraphicsContext Context => context;

        public bool VSync;

        public bool LimitFPS;

        public int TargetFPS = 120;

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
            if (OperatingSystem.IsWindows())
            {
                device = Adapter.CreateGraphics(RenderBackend.D3D11, this);
                context = device.Context;
                swapChain = device.SwapChain;
            }

            DebugDraw.Init(device);
            renderer = new(this, device);
            framebuffer = new(device);
            renderDispatcher = Dispatcher.CurrentDispatcher;
            SceneManager.SceneChanged += (_, _) => { firstFrame = true; };
            Time.Initialize();
            WidgetManager.Init(device);

            SceneManager.Load(new());
            deferredRenderer = new();
            while (isRunning)
            {
                Time.FrameUpdate();
                Dispatcher.ExecuteQueue();
                framebuffer.SourceViewport = Viewport;
                framebuffer.Update(context);
                renderer.BeginDraw();
                if (Designer.InDesignMode)
                {
                    framebuffer.Draw();
                }

                if (resize)
                {
                    device.SwapChain.Resize(Width, Height);
                    resize = false;
                }

                if (SceneManager.Current is not null)
                    lock (SceneManager.Current)
                    {
                        if (firstFrame)
                        {
                            Time.Initialize();
                            firstFrame = false;
                        }
                        //  if (Designer.InDesignMode && !Framebuffer.Fullframe && Designer.IsShown)
                        // SceneManager.Current?.Render(context, this, framebuffer.Viewport);
                        //  else
                        //  SceneManager.Current?.Render(context, this, Viewport);
                    }

                renderer.EndDraw();
                swapChain.Present(VSync ? 1u : 0u);
                LimitFrameRate();
                Keyboard.FrameUpdate();
            }
            WidgetManager.Dispose();
            renderer.Dispose();
            DebugDraw.Dispose();
            Trace.WriteLine("Perfoming Shutdown");
            SceneManager.Unload();
            Context.Dispose();
            Device.Dispose();
        }

        private long fpsFrameCount;
        private long fpsStartTime;

        private void LimitFrameRate()
        {
            if (LimitFPS & !VSync)
            {
                int fps = TargetFPS;
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