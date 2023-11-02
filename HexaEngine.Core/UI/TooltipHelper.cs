namespace HexaEngine.Core.UI
{
    using Hexa.NET.ImGui;

    public static class TooltipHelper
    {
        public static void Tooltip(string desc)
        {
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayShort) && ImGui.BeginTooltip())
            {
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted(desc);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }
    }
}