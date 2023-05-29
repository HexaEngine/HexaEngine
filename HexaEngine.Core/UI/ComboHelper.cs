namespace HexaEngine.Core.UI
{
    using ImGuiNET;
    using System;

    public static class ComboEnumHelper<T> where T : struct, Enum
    {
        private static readonly T[] values = Enum.GetValues<T>();
        private static readonly string[] names = Enum.GetNames<T>();

        public static bool Combo(string label, ref T value)
        {
            int index = Array.IndexOf(values, value);
            if (ImGui.Combo(label, ref index, names, names.Length))
            {
                value = values[index];
                return true;
            }
            return false;
        }

        public static void Text(T value)
        {
            int index = Array.IndexOf(values, value);
            ImGui.Text(names[index]);
        }
    }

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