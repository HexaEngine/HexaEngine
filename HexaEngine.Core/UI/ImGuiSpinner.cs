namespace HexaEngine.Core.UI
{
    using Hexa.NET.ImGui;
    using System;
    using System.Numerics;

    public static class ImGuiBufferingBar
    {
        public static unsafe void BufferingBar(string label, float value, Vector2 size, uint backgroundColor, uint foregroundColor)
        {
            ImGuiWindow* window = ImGui.GetCurrentWindow();
            if (window->SkipItems == 1)
            {
                return;
            }

            ImDrawList* drawList = ImGui.GetWindowDrawList();
            ImGuiContextPtr g = ImGui.GetCurrentContext();
            ImGuiStylePtr style = ImGui.GetStyle();
            int id = ImGui.GetID(label);

            var cursorPos = ImGui.GetCursorPos();

            Vector2 pos = window->DC.CursorPos;
            size.X -= style.FramePadding.X * 2;

            cursorPos -= window->WindowPadding;

            ImRect bb = new() { Min = pos + cursorPos, Max = pos + cursorPos + size };
            ImGui.ItemSizeRect(bb, style.FramePadding.Y);
            if (!ImGui.ItemAdd(bb, id, null, ImGuiItemFlags.None))
            {
                return;
            }

            // Render
            float circleStart = size.X * 0.7f;
            float circleEnd = size.Y;
            float circleWidth = circleEnd - circleStart;

            drawList->AddRectFilled(bb.Min, new Vector2(pos.X + circleStart, bb.Max.Y), backgroundColor);
            drawList->AddRectFilled(bb.Min, new Vector2(pos.X + circleStart * value, bb.Max.Y), foregroundColor);

            float t = (float)g.Time;
            float r = size.Y / 2;
            float speed = 1.5f;

            float a = speed * 0;
            float b = speed * 0.333f;
            float c = speed * 0.666f;

            float o1 = (circleWidth + r) * (t + a - speed * (int)((t + a) / speed)) / speed;
            float o2 = (circleWidth + r) * (t + b - speed * (int)((t + b) / speed)) / speed;
            float o3 = (circleWidth + r) * (t + c - speed * (int)((t + c) / speed)) / speed;

            drawList->AddCircleFilled(new Vector2(pos.X + circleEnd - o1, bb.Min.Y + r), r, backgroundColor);
            drawList->AddCircleFilled(new Vector2(pos.X + circleEnd - o2, bb.Min.Y + r), r, backgroundColor);
            drawList->AddCircleFilled(new Vector2(pos.X + circleEnd - o3, bb.Min.Y + r), r, backgroundColor);
        }
    }

    public static class ImGuiSpinner
    {
        public static unsafe void Spinner(string label, float radius, float thickness, uint color)
        {
            ImGuiWindow* window = ImGui.GetCurrentWindow();
            if (window->SkipItems == 1)
            {
                return;
            }

            ImDrawList* drawList = ImGui.GetWindowDrawList();
            var g = ImGui.GetCurrentContext();
            var style = ImGui.GetStyle();
            int id = ImGui.GetID(label);

            var cursorPos = ImGui.GetCursorPos();

            Vector2 pos = window->DC.CursorPos;
            Vector2 size = new(radius * 2, (radius + style.FramePadding.Y) * 2);

            ImRect bb = new()
            {
                Min = window->DC.CursorPos + cursorPos,
                Max = window->DC.CursorPos + cursorPos + size
            };

            ImGui.ItemSizeRect(bb, -1);
            if (!ImGui.ItemAdd(bb, id, null, ImGuiItemFlags.None))
            {
                return;
            }

            // Render
            ImGui.PathClear(drawList);

            const int num_segments = 24;
            int start = (int)Math.Abs(MathF.Sin((float)(g.Time * 1.8f)) * (num_segments - 5));

            float a_min = float.Pi * 2.0f * start / num_segments;
            float a_max = float.Pi * 2.0f * ((float)num_segments - 3) / num_segments;

            Vector2 center = pos + new Vector2(radius, radius + style.FramePadding.Y);

            for (var i = 0; i < num_segments; i++)
            {
                float a = a_min + (i / (float)num_segments) * (a_max - a_min);
                var time = (float)g.Time;
                var pp = new Vector2(center.X + MathF.Cos(a + time * 8) * radius, center.Y + MathF.Sin(a + time * 8) * radius);
                drawList->PathLineTo(pp);
            }

            ImGui.PathStroke(drawList, color, 0, thickness);
        }
    }
}