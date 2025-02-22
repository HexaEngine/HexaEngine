namespace CrashReporter
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Mathematics;
    using HexaEngine;
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Scenes;
    using System;
    using System.Diagnostics;
    using System.Numerics;

    public sealed class CrashWindow : CoreWindow
    {
        private ImGuiManager? imGuiRenderer;
#nullable disable
        private IGraphicsContext graphicsContext;
        private ISwapChain swapChain;
#nullable restore
        private bool resize;
        private bool firstFrame;
        private readonly string? reportFile;
        private readonly string reportMessage;

        public CrashWindow()
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

        public override Viewport WindowViewport => throw new NotSupportedException();

        public ISceneRenderer Renderer => throw new NotSupportedException();

        public override Viewport RenderViewport => throw new NotSupportedException();

        public override void Initialize(IAudioDevice audioDevice, IGraphicsDevice graphicsDevice)
        {
            base.Initialize(audioDevice, graphicsDevice);
            graphicsContext = graphicsDevice.Context;
            swapChain = SwapChain;
            imGuiRenderer = new(this, graphicsDevice, graphicsContext, ImGuiConfigFlags.NavEnableKeyboard | ImGuiConfigFlags.NavEnableGamepad | ImGuiConfigFlags.DockingEnable);
            Show();
        }

        public override void Render(IGraphicsContext context)
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

            ImGuiP.SetWindowPos(Vector2.Zero);
            ImGuiP.SetWindowSize(Viewport.Size);

            float footerHeightToReserve = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
            if (ImGui.BeginChild(1, new Vector2(0, -footerHeightToReserve), ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar))
            {
                ImGui.TextColored(new(1, 0, 0, 1), "Oops something went wrong!");
                ImGui.Text(reportFile ?? string.Empty);
                ImGui.Separator();
                var msg = reportMessage;
                var avail = ImGui.GetContentRegionAvail();
                ImGui.InputTextMultiline("##Msg", ref msg, (nuint)reportMessage.Length, avail, ImGuiInputTextFlags.ReadOnly);
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

        protected override void DisposeCore()
        {
            imGuiRenderer?.Dispose();
        }

        protected override void OnResized(ResizedEventArgs args)
        {
            resize = true;
            base.OnResized(args);
        }
    }
}