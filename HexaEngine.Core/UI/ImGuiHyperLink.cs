namespace HexaEngine.Core.UI
{
    using Hexa.NET.ImGui;
    using System.Numerics;

    public static unsafe class ImGuiHyperLink
    {
        public static bool HyperLink(byte* label, bool underlineWhenHoveredOnly = false)
        {
            uint linkColor = ImGui.ColorConvertFloat4ToU32(new(0.2f, 0.3f, 0.8f, 1));
            uint linkHoverColor = ImGui.ColorConvertFloat4ToU32(new(0.4f, 0.6f, 0.8f, 1));
            uint linkFocusColor = ImGui.ColorConvertFloat4ToU32(new(0.6f, 0.4f, 0.8f, 1));
            int id = ImGui.GetID(label);

            ImGuiWindow* window = ImGui.GetCurrentWindow();
            ImDrawList* draw = ImGui.GetWindowDrawList();

            Vector2 pos = ImGui.GetCursorScreenPos();
            Vector2 size = ImGui.CalcTextSize(label);
            ImRect bb = new() { Min = pos, Max = new(pos.X + size.X, pos.Y + size.Y) };

            ImGui.ItemSizeRect(bb, 0.0f);
            if (!ImGui.ItemAdd(bb, id, &bb, ImGuiItemFlags.None))
                return false;

            bool isHovered = false;
            bool isClicked = ImGui.ButtonBehavior(bb, id, &isHovered, null, 0);
            bool isFocused = ImGui.IsItemFocused();

            uint color = isHovered ? linkHoverColor : isFocused ? linkFocusColor : linkColor;

            draw->AddText(bb.Min, color, label);

            if (isFocused)
                draw->AddRect(bb.Min, bb.Max, color);
            else if (!underlineWhenHoveredOnly || isHovered)
                draw->AddLine(new(bb.Min.X, bb.Max.Y), bb.Max, color);

            return isClicked;
        }

        public static bool HyperLink(string label, bool underlineWhenHoveredOnly = false)
        {
            uint linkColor = ImGui.ColorConvertFloat4ToU32(new(0.2f, 0.3f, 0.8f, 1));
            uint linkHoverColor = ImGui.ColorConvertFloat4ToU32(new(0.4f, 0.6f, 0.8f, 1));
            uint linkFocusColor = ImGui.ColorConvertFloat4ToU32(new(0.6f, 0.4f, 0.8f, 1));
            int id = ImGui.GetID(label);

            ImGuiWindow* window = ImGui.GetCurrentWindow();
            ImDrawList* draw = ImGui.GetWindowDrawList();

            Vector2 pos = ImGui.GetCursorScreenPos();
            Vector2 size = ImGui.CalcTextSize(label);
            ImRect bb = new() { Min = pos, Max = new(pos.X + size.X, pos.Y + size.Y) };

            ImGui.ItemSizeRect(bb, 0.0f);
            if (!ImGui.ItemAdd(bb, id, &bb, ImGuiItemFlags.None))
                return false;

            bool isHovered = false;
            bool isClicked = ImGui.ButtonBehavior(bb, id, &isHovered, null, 0);
            bool isFocused = ImGui.IsItemFocused();

            uint color = isHovered ? linkHoverColor : isFocused ? linkFocusColor : linkColor;

            draw->AddText(bb.Min, color, label);

            if (isFocused)
                draw->AddRect(bb.Min, bb.Max, color);
            else if (!underlineWhenHoveredOnly || isHovered)
                draw->AddLine(new(bb.Min.X, bb.Max.Y), bb.Max, color);

            return isClicked;
        }

        public static bool HyperLink(ref byte label, bool underlineWhenHoveredOnly = false)
        {
            uint linkColor = ImGui.ColorConvertFloat4ToU32(new(0.2f, 0.3f, 0.8f, 1));
            uint linkHoverColor = ImGui.ColorConvertFloat4ToU32(new(0.4f, 0.6f, 0.8f, 1));
            uint linkFocusColor = ImGui.ColorConvertFloat4ToU32(new(0.6f, 0.4f, 0.8f, 1));
            int id = ImGui.GetID(ref label);

            ImGuiWindow* window = ImGui.GetCurrentWindow();
            ImDrawList* draw = ImGui.GetWindowDrawList();

            Vector2 pos = ImGui.GetCursorScreenPos();
            Vector2 size = ImGui.CalcTextSize(ref label);
            ImRect bb = new() { Min = pos, Max = new(pos.X + size.X, pos.Y + size.Y) };

            ImGui.ItemSizeRect(bb, 0.0f);
            if (!ImGui.ItemAdd(bb, id, &bb, ImGuiItemFlags.None))
                return false;

            bool isHovered = false;
            bool isClicked = ImGui.ButtonBehavior(bb, id, &isHovered, null, 0);
            bool isFocused = ImGui.IsItemFocused();

            uint color = isHovered ? linkHoverColor : isFocused ? linkFocusColor : linkColor;

            draw->AddText(bb.Min, color, ref label);

            if (isFocused)
                draw->AddRect(bb.Min, bb.Max, color);
            else if (!underlineWhenHoveredOnly || isHovered)
                draw->AddLine(new(bb.Min.X, bb.Max.Y), bb.Max, color);

            return isClicked;
        }
    }
}