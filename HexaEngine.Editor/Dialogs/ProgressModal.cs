namespace HexaEngine.Editor.Dialogs
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.UI;
    using System;
    using System.Numerics;

    public enum ProgressType
    {
        Spinner,
        Bar
    }

    public sealed class ProgressModal : Modal, IDisposable, IProgress<float>
    {
        private readonly string title;
        private readonly string message;
        private readonly ProgressType type;
        private float progress;

        public ProgressModal(string title, string message, ProgressType type = ProgressType.Spinner)
        {
            this.title = title;
            this.message = message;
            this.type = type;
        }

        public override string Name => title;

        protected override ImGuiWindowFlags Flags { get; } = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove;

        public override void Reset()
        {
            progress = 0;
        }

        public override unsafe void Draw()
        {
            Vector2 main_viewport_pos = ImGui.GetMainViewport().Pos;
            Vector2 main_viewport_size = ImGui.GetMainViewport().Size;
            ImGui.SetNextWindowPos(main_viewport_pos);
            ImGui.SetNextWindowSize(main_viewport_size);
            ImGui.SetNextWindowBgAlpha(0.9f);
            ImGui.Begin("Overlay", null, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoInputs);
            ImGui.End();
            base.Draw();
        }

        protected override unsafe void DrawContent()
        {
            var size = ImGui.GetWindowSize();
            Vector2 mainViewportPos = ImGui.GetMainViewport().Pos;
            var s = ImGui.GetPlatformIO().Monitors.Data[0].MainSize;
            ImGui.SetWindowPos(mainViewportPos + (s / 2 - size / 2));
            if (type == ProgressType.Spinner)
            {
                ImGuiSpinner.Spinner(message, 6, 3, 0xffcf7334);
                ImGui.SameLine();
            }

            ImGui.Text(message);

            if (type == ProgressType.Bar)
            {
                ImGuiBufferingBar.BufferingBar(message, progress, new(200, 6), 0xff424242, 0xffcf7334);
            }
        }

        public void Dispose()
        {
            Close();
        }

        public void Report(float value)
        {
            progress = value;
        }
    }
}