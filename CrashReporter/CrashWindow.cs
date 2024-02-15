namespace CrashReporter
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core;
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Threading;
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using System;
    using System.Diagnostics;
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
        private string? reportFile;
        private string reportMessage;

        public CrashWindow() : base(WindowPosCentered, WindowPosCentered, 700, 400, Silk.NET.SDL.WindowFlags.Borderless)
        {
            var args = Environment.GetCommandLineArgs();

            if (args.Length > 1 && File.Exists(args[1]))
            {
                reportFile = args[1];
                reportMessage = File.ReadAllText(args[1]);
            }
            else
            {
                reportMessage = "Couldn't load crash report!";
            }
        }

        public ThreadDispatcher Dispatcher => throw new NotSupportedException();

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
                Time.ResetTime();
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
                ImGui.Text(reportFile ?? string.Empty);
                ImGui.Separator();
                ImGui.Text(reportMessage);
            }
            ImGui.EndChild();

            ImGui.SetNextItemWidth(-100);
            if (ImGui.Button("Ok"))
            {
                Close();
            }
            ImGui.SameLine();
            if (reportFile != null && ImGui.Button("Open Log"))
            {
                Process.Start(reportFile);
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