namespace CrashReporter
{
    using HexaEngine.Core;
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.ImGuiNET;
    using HexaEngine.Mathematics;
    using HexaEngine.Rendering.Renderers;
    using HexaEngine.Scenes;
    using System;
    using System.Numerics;

    public class CrashWindow : SdlWindow, IRenderWindow
    {
        private ImGuiManager? imGuiRenderer;
        private IAudioDevice audioDevice;
        private IGraphicsDevice graphicsDevice;
        private IGraphicsContext graphicsContext;
        private ISwapChain swapChain;
        private bool resize;
        private bool firstFrame;
        private string reportMessage;

        public CrashWindow() : base(WindowPosCentered, WindowPosCentered, 700, 400, Silk.NET.SDL.WindowFlags.Borderless)
        {
            reportMessage = File.ReadAllText(@"C:\Users\juna\source\repos\JunaMeinhold\HexaEngine\Editor\bin\Debug\net8.0\logs\crash-2023-18-7--11-48-38.log");
        }

        public RenderDispatcher Dispatcher => throw new NotSupportedException();

        public IGraphicsDevice Device => graphicsDevice;

        public IGraphicsContext Context => graphicsContext;

        public IAudioDevice AudioDevice => audioDevice;

        public ISwapChain SwapChain => swapChain;

        public Viewport WindowViewport => throw new NotSupportedException();

        public ISceneRenderer Renderer => throw new NotSupportedException();

        public Viewport RenderViewport => throw new NotSupportedException();

        public virtual void Initialize(IAudioDevice audioDevice, IGraphicsDevice graphicsDevice)
        {
            this.audioDevice = audioDevice;
            this.graphicsDevice = graphicsDevice;

            graphicsContext = graphicsDevice.Context;
            swapChain = graphicsDevice.CreateSwapChain(this) ?? throw new PlatformNotSupportedException();
            swapChain.Active = true;

            imGuiRenderer = new(this, graphicsDevice, graphicsContext, ImGuiConfigFlags.NavEnableKeyboard | ImGuiConfigFlags.NavEnableGamepad | ImGuiConfigFlags.DockingEnable);
        }

        public void Render(IGraphicsContext context)
        {
            if (resize)
            {
                swapChain.Resize(Width, Height);
                resize = false;
            }

            if (firstFrame)
            {
                Time.Initialize();
                firstFrame = false;
            }

            context.ClearDepthStencilView(swapChain.BackbufferDSV, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
            context.ClearRenderTargetView(swapChain.BackbufferRTV, Vector4.Zero);

            imGuiRenderer?.NewFrame();

            bool shown = true;

            if (!ImGui.Begin("Crash report", ref shown, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse))
            {
                ImGui.End();
            }

            ImGui.SetWindowPos(Vector2.Zero);
            ImGui.SetWindowSize(Viewport.Size);

            float footerHeightToReserve = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
            if (ImGui.BeginChild(1, new Vector2(0, -footerHeightToReserve), false, ImGuiWindowFlags.HorizontalScrollbar))
            {
                ImGui.TextColored(new(1, 0, 0, 1), "Oops something went wrong!");
                ImGui.Separator();
                ImGui.Text(reportMessage);
            }
            ImGui.EndChild();

            ImGui.SetNextItemWidth(-100);
            if (ImGui.Button("Ok"))
            {
                Close();
            }

            if (!shown)
            {
                Close();
            }

            ImGui.End();

            swapChain.WaitForPresent();

            context.SetRenderTarget(swapChain.BackbufferRTV, null);
            imGuiRenderer?.EndFrame();

            swapChain.Present();
            swapChain.Wait();
        }

        public virtual void Uninitialize()
        {
            imGuiRenderer?.Dispose();

            swapChain.Dispose();
            graphicsContext.Dispose();
            graphicsDevice.Dispose();
        }

        protected override void OnResized(ResizedEventArgs args)
        {
            resize = true;
            base.OnResized(args);
        }
    }
}