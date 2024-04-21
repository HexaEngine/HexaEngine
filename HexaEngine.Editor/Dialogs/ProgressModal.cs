namespace HexaEngine.Editor.Dialogs
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.UI;
    using System;
    using System.Numerics;

    public enum ProgressType
    {
        Spinner,
        Bar,
    }

    public enum ProgressFlags
    {
        None = 0,
        NoOverlay = 1 << 1,
        NoModal = 1 << 2,
        TopLeft = 1 << 3,
        TopRight = 1 << 4,
        BottomLeft = 1 << 5,
        BottomRight = 1 << 6,
    }

    public sealed class ProgressModal : Modal, IDisposable, IProgress<float>
    {
        private readonly string title;
        private readonly string message;
        private readonly ProgressType type;
        private readonly ProgressFlags progressFlags;
        private readonly Vector2 offsetCenter;
        private float progress;

        public ProgressModal(string title, string message, ProgressType type = ProgressType.Spinner, ProgressFlags progressFlags = ProgressFlags.None)
        {
            this.title = title;
            this.message = message;
            this.type = type;
            this.progressFlags = progressFlags;
        }

        public ProgressModal(string title, string message, ProgressType type, ProgressFlags progressFlags, Vector2 offsetCenter)
        {
            this.title = title;
            this.message = message;
            this.type = type;
            this.progressFlags = progressFlags;
            this.offsetCenter = offsetCenter;
        }

        public override string Name => title;

        protected override ImGuiWindowFlags Flags { get; } = ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoInputs;

        public override void Reset()
        {
            progress = 0;
        }

        public override unsafe void Draw()
        {
            if (!shown || signalClose)
            {
                if ((progressFlags & ProgressFlags.NoModal) != 0)
                {
                    shown = false;
                    return;
                }
                base.Draw();
                return;
            }

            if ((progressFlags & ProgressFlags.NoOverlay) == 0)
            {
                Vector2 main_viewport_pos = ImGui.GetMainViewport().Pos;
                Vector2 main_viewport_size = ImGui.GetMainViewport().Size;
                ImGui.SetNextWindowPos(main_viewport_pos);
                ImGui.SetNextWindowSize(main_viewport_size);
                ImGui.SetNextWindowBgAlpha(0.9f);
                ImGui.Begin("Overlay", null, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoInputs);
                ImGui.End();
            }
            if ((progressFlags & ProgressFlags.NoModal) != 0)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, 30);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 30);
                ImGui.PushStyleColor(ImGuiCol.Border, 0xff4c4c4c);
                ImGui.PushStyleColor(ImGuiCol.WindowBg, 0xff1c1c1c);
                if (ImGui.Begin(Name, ref shown, Flags))
                {
                    DrawContent();
                    ImGui.End();
                }
                ImGui.PopStyleColor();
                ImGui.PopStyleColor();
                ImGui.PopStyleVar();
                ImGui.PopStyleVar();
            }
            else
            {
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, 30);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 30);
                ImGui.PushStyleColor(ImGuiCol.Border, 0xff4c4c4c);

                base.Draw();

                ImGui.PopStyleColor();
                ImGui.PopStyleVar();
                ImGui.PopStyleVar();
            }
        }

        protected override unsafe void DrawContent()
        {
            var windowSize = ImGui.GetWindowSize();
            var mainViewport = ImGui.GetMainViewport();
            float offsetX = mainViewport.Size.X * offsetCenter.X;
            float offsetY = mainViewport.Size.Y * offsetCenter.Y;
            Vector2 winPos;
            if ((progressFlags & ProgressFlags.TopLeft) != 0)
            {
                Vector2 pos = mainViewport.Pos;
                Vector2 size = mainViewport.Size;
                pos.Y += offsetY;
                pos.X += offsetX;
                ImGui.SetWindowPos(winPos = pos);
            }
            else if ((progressFlags & ProgressFlags.BottomLeft) != 0)
            {
                Vector2 pos = mainViewport.Pos;
                Vector2 size = mainViewport.Size;
                pos.Y += size.Y - windowSize.Y - offsetY;
                pos.X += offsetX;
                ImGui.SetWindowPos(winPos = pos);
            }
            else
            {
                Vector2 mainViewportPos = mainViewport.Pos;
                var s = ImGui.GetPlatformIO().Monitors.Data[0].MainSize;
                ImGui.SetWindowPos(winPos = mainViewportPos + (s / 2 - windowSize / 2));
            }

            if (type == ProgressType.Spinner)
            {
                ImGuiSpinner.Spinner(message, ImGui.GetTextLineHeight() / 2 - 3, 3, 0xffcf7334);
                ImGui.SameLine();
            }

            if (type == ProgressType.Bar)
            {
                const float padding = 5;
                var win = ImGui.GetCurrentWindow();
                var pos = win.OuterRectClipped.Min + new Vector2(padding);
                var drawList = ImGui.GetWindowDrawList();
                drawList.PushClipRect(win.OuterRectClipped.Min, win.OuterRectClipped.Max);
                drawList.AddRectFilled(pos, pos + win.SizeFull * new Vector2(progress, 1) - new Vector2(padding * 2), 0xffcf7334, 30, ImDrawFlags.RoundCornersAll);
                drawList.PopClipRect();
            }

            Vector2 cursor = ImGui.GetCursorPos();

            var windowHeight = ImGui.GetWindowSize().Y;
            var textSize = ImGui.CalcTextSize(message);

            ImGui.SetCursorPosY((windowHeight - textSize.Y) * 0.5f);
            ImGui.Text(message);
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