namespace HexaEngine.Core.UI
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Mathematics;
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public static unsafe class ImGuiColorTrackballWidget
    {
        private static uint ImColor32(byte r, byte g, byte b, byte a)
        {
            return (uint)((a << 24) | (b << 16) | (g << 8) | r);
        }

        public static bool ColorTrackballs(string label, ref Vector4 value, Vector2 size)
        {
            return ColorTrackballs(label, (Vector4*)Unsafe.AsPointer(ref value), size);
        }

        public static bool ColorTrackballs(string label, Vector4* value, Vector2 size)
        {
            ImGuiWindow* window = ImGuiP.GetCurrentWindow();

            uint id = ImGui.GetID(label);
            if (window->SkipItems != 0)
                return false;

            float labelHeight = ImGui.GetTextLineHeight();
            const float slider_height = 20.0f;

            var cursorPos = ImGui.GetCursorScreenPos();

            ImRect bb = new()
            {
                Min = cursorPos,
                Max = cursorPos + size
            };

            ImGuiP.ItemSize(bb, -1);
            if (!ImGuiP.ItemAdd(bb, id, &bb, ImGuiItemFlags.None))
                return false;

            ImGui.PushID(label);

            bool hovered = ImGuiP.ItemHoverable(bb, id, 0);

            ImGuiStylePtr style = ImGui.GetStyle();
            ImGuiIOPtr io = ImGui.GetIO();

            ImDrawList* drawList = ImGui.GetWindowDrawList();

            var mousePos = ImGui.GetMousePos();

            ColorHSVA colorHSVA = new(new Color(*value));

            bool modified = false;

            ImGuiP.RenderTextClipped(bb.Min, bb.Max, label, (byte*)null, null, new(0.5f, 0), default);

            {
                Vector2 wheelSize = size - new Vector2(0, slider_height + labelHeight * 2 + 20);
                Vector2 center = bb.Min + wheelSize * 0.5f + new Vector2(0, labelHeight * 2);
                float radius = Math.Min(wheelSize.X, wheelSize.Y) * 0.5f - style.FramePadding.X;

                // bg grid.
                {
                    // draw frame
                    drawList->AddCircleFilled(center, radius + 10f, ImGui.GetColorU32(ImGuiCol.Border, 1));
                    drawList->AddCircleFilled(center, radius + 5f, ImGui.GetColorU32(ImGuiCol.FrameBg, 1));

                    Vector2 pt = new(center.X, center.Y - radius);
                    Vector2 pb = new(center.X, center.Y + radius);
                    Vector2 pl = new(center.X - radius, center.Y);
                    Vector2 pr = new(center.X + radius, center.Y);

                    drawList->AddLine(pt, pb, 0xFF3C3C3C, 2f);
                    drawList->AddLine(pl, pr, 0xFF3C3C3C, 2f);
                }

                // hue wheel.
                {
                    byte styleAlpha = (byte)(style.Alpha * byte.MaxValue);

                    uint* col_hues = stackalloc uint[] { ImColor32(255, 0, 0, styleAlpha), ImColor32(255, 255, 0, styleAlpha), ImColor32(0, 255, 0, styleAlpha), ImColor32(0, 255, 255, styleAlpha), ImColor32(0, 0, 255, styleAlpha), ImColor32(255, 0, 255, styleAlpha), ImColor32(255, 0, 0, styleAlpha) };

                    const float wheel_thickness = 10;
                    float wheel_r_outer = radius;
                    float wheel_r_inner = radius - wheel_thickness;

                    float aeps = 0.5f / wheel_r_outer;
                    int segment_per_arc = Math.Max(4, (int)wheel_r_outer / 12);
                    for (int n = 0; n < 6; n++)
                    {
                        float a0 = ((n)) / 6.0f * 2.0f * float.Pi - aeps;
                        float a1 = ((n + 1.0f)) / 6.0f * 2.0f * float.Pi + aeps;

                        a0 += -MathF.PI / 2;
                        a1 += -MathF.PI / 2;

                        a0 = float.Pi - a0;
                        a1 = float.Pi - a1;

                        {
                            int vert_start_idx = drawList->VtxBuffer.Size;
                            drawList->PathArcTo(center, (wheel_r_inner + wheel_r_outer) * 0.5f, a0, a1, segment_per_arc);
                            drawList->PathStroke(uint.MaxValue, 0, wheel_thickness);
                            int vert_end_idx = drawList->VtxBuffer.Size;

                            Vector2 gradient_p0 = new(center.X + MathF.Cos(a0) * wheel_r_inner, center.Y + MathF.Sin(a0) * wheel_r_inner);
                            Vector2 gradient_p1 = new(center.X + MathF.Cos(a1) * wheel_r_inner, center.Y + MathF.Sin(a1) * wheel_r_inner);
                            ImGuiP.ShadeVertsLinearColorGradientKeepAlpha(drawList, vert_start_idx, vert_end_idx, gradient_p0, gradient_p1, col_hues[n], col_hues[n + 1]);
                        }
                    }
                }

                // Draw selector
                {
                    float selector_angle = float.Pi - (colorHSVA.H * 2.0f * MathF.PI - MathF.PI / 2);
                    Vector2 selector_pos = center + new Vector2(MathF.Cos(selector_angle), MathF.Sin(selector_angle)) * radius * colorHSVA.S;
                    drawList->AddCircle(selector_pos, 6.0f, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1)));

                    float distanceToCenter = Vector2.Distance(center, mousePos);

                    if (distanceToCenter < radius && ImGui.IsMouseDown(ImGuiMouseButton.Left))
                    {
                        Vector2 relativeMousePos = mousePos - center;

                        float angle = MathF.Atan2(relativeMousePos.Y, relativeMousePos.X);

                        float hue = (3 * MathF.PI / 2 - angle) / (2.0f * MathF.PI);

                        if (hue < 0)
                        {
                            hue += 1.0f;
                        }
                        else if (hue > 1)
                        {
                            hue -= 1.0f;
                        }

                        float saturation = Math.Min(distanceToCenter / radius, 1.0f);

                        colorHSVA.H = hue;
                        colorHSVA.S = saturation;

                        *value = colorHSVA.ToRGBA();
                        modified = true;
                    }
                }
            }

            // Value Slider
            {
                ImRect slider_bb = new() { Min = new Vector2(bb.Min.X, bb.Max.Y - slider_height - labelHeight), Max = new Vector2(bb.Min.X, bb.Max.Y - labelHeight) + new Vector2(size.X - style.FramePadding.X, 0) };
                Vector2 slider_center = new(bb.Min.X + size.X * 0.5f, bb.Max.Y - labelHeight - slider_height * 0.5f);

                float sliderWidth = (slider_bb.Max.X - slider_bb.Min.X);

                ImGuiP.RenderFrame(slider_bb.Min, slider_bb.Max, ImGui.GetColorU32(ImGuiCol.FrameBg, 1), true, style.FrameRounding);

                float handle_radius = slider_height * 0.35f;

                Vector2 handle_center = slider_center;

                if (ImGui.IsMouseHoveringRect(slider_bb.Min, slider_bb.Max) && ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    Vector2 relativeMousePos = ImGui.GetMousePos() - handle_center;
                    handle_center.X += relativeMousePos.X;
                    var normalized = relativeMousePos.X / (sliderWidth * 0.5f);
                    colorHSVA.V += normalized * io.DeltaTime;
                    *value = colorHSVA.ToRGBA();
                    modified = true;
                }

                drawList->AddCircleFilled(handle_center, handle_radius, ImGui.GetColorU32(ImGuiCol.SliderGrab));
            }

            {
                float textSize = ImGui.CalcTextSize("0,00").X;
                var col = *value;
                ImRect text_bb = new() { Min = new Vector2(bb.Min.X, bb.Max.Y - labelHeight), Max = new Vector2(bb.Min.X, bb.Max.Y) + new Vector2(size.X, 0) };

                float width = size.X;
                ImGuiP.RenderTextClipped(text_bb.Min, text_bb.Max, $"{col.X:N2}", (byte*)null, null, new(0, 0), default);
                drawList->AddLine(new Vector2(bb.Min.X, bb.Max.Y), new Vector2(bb.Min.X + textSize, bb.Max.Y), 0xFF0000FF, 2);
                ImGuiP.RenderTextClipped(text_bb.Min, text_bb.Max, $"{col.Y:N2}", (byte*)null, null, new(0.5f, 0), default);
                drawList->AddLine(new Vector2(text_bb.Min.X + width * 0.5f - textSize * 0.5f, bb.Max.Y), new Vector2(text_bb.Min.X + width * 0.5f + textSize * 0.5f, bb.Max.Y), 0xFF00FF00, 2);
                ImGuiP.RenderTextClipped(text_bb.Min, text_bb.Max, $"{col.Z:N2}", (byte*)null, null, new(1f, 0), default);
                drawList->AddLine(new Vector2(text_bb.Max.X - textSize, bb.Max.Y), new Vector2(text_bb.Max.X, bb.Max.Y), 0xFFFF0000, 2);
            }

            // buttons; @todo: mirror, smooth, tessellate
            if (ImGui.BeginPopupContextItem(label))
            {
                if (ImGui.Selectable("Reset"))
                {
                    *value = Vector4.One;
                    modified = true;
                }

                ImGui.Separator();
                if (ImGui.BeginMenu("Presets"))
                {
                    ImGui.PushID("curve_items");

                    ImGui.PopID();
                    ImGui.EndMenu();
                }

                ImGui.EndPopup();
            }

            ImGui.PopID();

            ImGui.PushItemWidth(size.X - style.FramePadding.X);
            //  modified |= ImGui.DragFloat4("##", (float*)value);
            ImGui.PopItemWidth();

            return modified;
        }

        public static bool ColorTrackballs(string label, ref Vector3 value, Vector2 size)
        {
            return ColorTrackballs(label, (Vector3*)Unsafe.AsPointer(ref value), size);
        }

        public static bool ColorTrackballs(string label, Vector3* value, Vector2 size)
        {
            ImGuiWindow* window = ImGuiP.GetCurrentWindow();

            uint id = ImGui.GetID(label);
            if (window->SkipItems != 0)
                return false;

            float labelHeight = ImGui.GetTextLineHeight();
            const float slider_height = 20.0f;

            var cursorPos = ImGui.GetCursorScreenPos();

            ImRect bb = new()
            {
                Min = cursorPos,
                Max = cursorPos + size
            };

            ImGuiP.ItemSize(bb, -1);
            if (!ImGuiP.ItemAdd(bb, id, &bb, ImGuiItemFlags.None))
                return false;

            ImGui.PushID(label);

            bool hovered = ImGuiP.ItemHoverable(bb, id, 0);

            ImGuiStylePtr style = ImGui.GetStyle();
            ImGuiIOPtr io = ImGui.GetIO();

            ImDrawList* drawList = ImGui.GetWindowDrawList();

            var mousePos = ImGui.GetMousePos();

            ColorHSVA colorHSVA = new(new Color(*value, 1));

            bool modified = false;

            ImGuiP.RenderTextClipped(bb.Min, bb.Max, label, (byte*)null, null, new(0.5f, 0), default);

            {
                Vector2 wheelSize = size - new Vector2(0, slider_height + labelHeight * 2 + 20);
                Vector2 center = bb.Min + wheelSize * 0.5f + new Vector2(0, labelHeight * 2);
                float radius = Math.Min(wheelSize.X, wheelSize.Y) * 0.5f - style.FramePadding.X;

                // bg grid.
                {
                    // draw frame
                    drawList->AddCircleFilled(center, radius + 10f, ImGui.GetColorU32(ImGuiCol.Border, 1));
                    drawList->AddCircleFilled(center, radius + 5f, ImGui.GetColorU32(ImGuiCol.FrameBg, 1));

                    Vector2 pt = new(center.X, center.Y - radius);
                    Vector2 pb = new(center.X, center.Y + radius);
                    Vector2 pl = new(center.X - radius, center.Y);
                    Vector2 pr = new(center.X + radius, center.Y);

                    drawList->AddLine(pt, pb, 0xFF3C3C3C, 2f);
                    drawList->AddLine(pl, pr, 0xFF3C3C3C, 2f);
                }

                // hue wheel.
                {
                    byte styleAlpha = (byte)(style.Alpha * byte.MaxValue);

                    uint* col_hues = stackalloc uint[] { ImColor32(255, 0, 0, styleAlpha), ImColor32(255, 255, 0, styleAlpha), ImColor32(0, 255, 0, styleAlpha), ImColor32(0, 255, 255, styleAlpha), ImColor32(0, 0, 255, styleAlpha), ImColor32(255, 0, 255, styleAlpha), ImColor32(255, 0, 0, styleAlpha) };

                    const float wheel_thickness = 10;
                    float wheel_r_outer = radius;
                    float wheel_r_inner = radius - wheel_thickness;

                    float aeps = 0.5f / wheel_r_outer;
                    int segment_per_arc = Math.Max(4, (int)wheel_r_outer / 12);
                    for (int n = 0; n < 6; n++)
                    {
                        float a0 = ((n)) / 6.0f * 2.0f * float.Pi - aeps;
                        float a1 = ((n + 1.0f)) / 6.0f * 2.0f * float.Pi + aeps;

                        a0 += -MathF.PI / 2;
                        a1 += -MathF.PI / 2;

                        a0 = float.Pi - a0;
                        a1 = float.Pi - a1;

                        {
                            int vert_start_idx = drawList->VtxBuffer.Size;
                            drawList->PathArcTo(center, (wheel_r_inner + wheel_r_outer) * 0.5f, a0, a1, segment_per_arc);
                            drawList->PathStroke(uint.MaxValue, 0, wheel_thickness);
                            int vert_end_idx = drawList->VtxBuffer.Size;

                            Vector2 gradient_p0 = new(center.X + MathF.Cos(a0) * wheel_r_inner, center.Y + MathF.Sin(a0) * wheel_r_inner);
                            Vector2 gradient_p1 = new(center.X + MathF.Cos(a1) * wheel_r_inner, center.Y + MathF.Sin(a1) * wheel_r_inner);
                            ImGuiP.ShadeVertsLinearColorGradientKeepAlpha(drawList, vert_start_idx, vert_end_idx, gradient_p0, gradient_p1, col_hues[n], col_hues[n + 1]);
                        }
                    }
                }

                // Draw selector
                {
                    float selector_angle = float.Pi - (colorHSVA.H * 2.0f * MathF.PI - MathF.PI / 2);
                    Vector2 selector_pos = center + new Vector2(MathF.Cos(selector_angle), MathF.Sin(selector_angle)) * radius * colorHSVA.S;
                    drawList->AddCircle(selector_pos, 6.0f, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1)));

                    float distanceToCenter = Vector2.Distance(center, mousePos);

                    if (distanceToCenter < radius && ImGui.IsMouseDown(ImGuiMouseButton.Left))
                    {
                        Vector2 relativeMousePos = mousePos - center;

                        float angle = MathF.Atan2(relativeMousePos.Y, relativeMousePos.X);

                        float hue = (3 * MathF.PI / 2 - angle) / (2.0f * MathF.PI);

                        if (hue < 0)
                        {
                            hue += 1.0f;
                        }
                        else if (hue > 1)
                        {
                            hue -= 1.0f;
                        }

                        float saturation = Math.Min(distanceToCenter / radius, 1.0f);

                        colorHSVA.H = hue;
                        colorHSVA.S = saturation;

                        *value = colorHSVA.ToRGBA().ToVector4().ToVec3();
                        modified = true;
                    }
                }
            }

            // Value Slider
            {
                ImRect slider_bb = new() { Min = new Vector2(bb.Min.X, bb.Max.Y - slider_height - labelHeight), Max = new Vector2(bb.Min.X, bb.Max.Y - labelHeight) + new Vector2(size.X - style.FramePadding.X, 0) };
                Vector2 slider_center = new(bb.Min.X + size.X * 0.5f, bb.Max.Y - labelHeight - slider_height * 0.5f);

                float sliderWidth = (slider_bb.Max.X - slider_bb.Min.X);

                ImGuiP.RenderFrame(slider_bb.Min, slider_bb.Max, ImGui.GetColorU32(ImGuiCol.FrameBg, 1), true, style.FrameRounding);

                float handle_radius = slider_height * 0.35f;

                Vector2 handle_center = slider_center;

                if (ImGui.IsMouseHoveringRect(slider_bb.Min, slider_bb.Max) && ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    Vector2 relativeMousePos = ImGui.GetMousePos() - handle_center;
                    handle_center.X += relativeMousePos.X;
                    var normalized = relativeMousePos.X / (sliderWidth * 0.5f);
                    colorHSVA.V += normalized * io.DeltaTime;
                    *value = colorHSVA.ToRGBA().ToVector4().ToVec3();
                    modified = true;
                }

                drawList->AddCircleFilled(handle_center, handle_radius, ImGui.GetColorU32(ImGuiCol.SliderGrab));
            }

            {
                float textSize = ImGui.CalcTextSize("0,00").X;
                var col = *value;
                ImRect text_bb = new() { Min = new Vector2(bb.Min.X, bb.Max.Y - labelHeight), Max = new Vector2(bb.Min.X, bb.Max.Y) + new Vector2(size.X, 0) };

                float width = size.X;
                ImGuiP.RenderTextClipped(text_bb.Min, text_bb.Max, $"{col.X:N2}", (byte*)null, null, new(0, 0), default);
                drawList->AddLine(new Vector2(bb.Min.X, bb.Max.Y), new Vector2(bb.Min.X + textSize, bb.Max.Y), 0xFF0000FF, 2);
                ImGuiP.RenderTextClipped(text_bb.Min, text_bb.Max, $"{col.Y:N2}", (byte*)null, null, new(0.5f, 0), default);
                drawList->AddLine(new Vector2(text_bb.Min.X + width * 0.5f - textSize * 0.5f, bb.Max.Y), new Vector2(text_bb.Min.X + width * 0.5f + textSize * 0.5f, bb.Max.Y), 0xFF00FF00, 2);
                ImGuiP.RenderTextClipped(text_bb.Min, text_bb.Max, $"{col.Z:N2}", (byte*)null, null, new(1f, 0), default);
                drawList->AddLine(new Vector2(text_bb.Max.X - textSize, bb.Max.Y), new Vector2(text_bb.Max.X, bb.Max.Y), 0xFFFF0000, 2);
            }

            ImGui.PopID();

            return modified;
        }
    }
}