namespace HexaEngine.Editor.UI
{
    using Hexa.NET.ImGui;
    using HexaEngine.Graphics.Renderers;
    using System.Numerics;

    public delegate void DrawMainMenuBar(TitleBarContext context);

    public class TitleBarMainMenuBar : TitleBarElement
    {
        private readonly DrawMainMenuBar drawMainMenuBar;
        private Vector2 size;

        public TitleBarMainMenuBar(DrawMainMenuBar drawMainMenuBar)
        {
            this.drawMainMenuBar = drawMainMenuBar;
        }

        public override Vector2 Size => size;

        public override string Label { get; } = "MainMenuBar";

        public override bool IsVisible { get; } = true;

        public override void Draw(TitleBarContext context)
        {
            ImGuiManager.PushFont("Default");
            ImGui.PushStyleColor(ImGuiCol.MenuBarBg, context.BackgroundColor);
            if (ImGui.BeginMainMenuBar())
            {
                var start = ImGui.GetCursorPos();
                drawMainMenuBar(context);
                var end = ImGui.GetCursorPos();
                context.Offset = size = end - start;
                ImGui.EndMainMenuBar();
            }
            ImGui.PopStyleColor();
            ImGuiManager.PopFont();
        }
    }
}