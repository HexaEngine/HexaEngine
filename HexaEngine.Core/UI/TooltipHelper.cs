namespace HexaEngine.Core.UI
{
    using Hexa.NET.ImGui;
    using HexaEngine.Mathematics;

    /// <summary>
    /// A tooltip helper for ImGui
    /// </summary>
    public static class TooltipHelper
    {
        /// <summary>
        /// Shows a tooltip if the item is hovered with <paramref name="desc"/> as text.
        /// </summary>
        /// <param name="desc">The text of the tooltip.</param>
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

        /// <summary>
        /// Shows a tooltip if the item is hovered with <paramref name="name"/> as text.
        /// </summary>
        /// <param name="name">The text of the tooltip.</param>
        public static void Tooltip(ImGuiName name)
        {
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayShort) && ImGui.BeginTooltip())
            {
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted(name.Name);
                ImGui.SameLine();
                ImGui.TextColored(Colors.Gray, name.RawId);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }
    }
}