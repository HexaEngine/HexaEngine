using Hexa.NET.ImGui;

namespace HexaEngine.Core.UI
{
    using Hexa.NET.Utilities;
    using Hexa.NET.Mathematics;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public static unsafe class ImGuiCurveEditor
    {
        private static float NODE_SLOT_RADIUS = 4.0f;

        private static Vector2 node_pos;
        private static int last_node_id;

        public enum CurveEditorFlags
        {
            None = 0,
            NoTangents = 1 << 0,
            ShowGrid = 1 << 1,
            Reset = 1 << 2
        };

        public enum StorageValues : int
        {
            FROM_X = 100,
            FROM_Y,
            WIDTH,
            HEIGHT,
            IS_PANNING,
            POINT_START_X,
            POINT_START_Y
        };

        private static Vector2 start_pan;

        private static float SIZE = 4;

        private static float SIZE_handleTangent = 4;
        private static float LENGTH = 18;

        public static int CurveEditor(string label, Vector2[] values, int pointsCount, Vector2 editorSize, CurveEditorFlags flags, ref int newCount)
        {
            return CurveEditor(label, (Vector2*)Unsafe.AsPointer(ref values[0]), pointsCount, editorSize, flags, (int*)Unsafe.AsPointer(ref newCount));
        }

        public static int CurveEditor(string label, Vector2* values, int pointsCount, Vector2 editorSize, CurveEditorFlags flags, int* newCount)
        {
            float HEIGHT = 100;
            ImGuiStylePtr style = ImGui.GetStyle();
            Vector2 size = editorSize;
            size.X = size.X < 0 ? ImGui.CalcItemWidth() + (style.FramePadding.X * 2) : size.X;
            size.Y = size.Y < 0 ? HEIGHT : size.Y;

            ImGuiWindow* parent_window = ImGui.GetCurrentWindow();
            ImDrawList* DrawList = ImGui.GetWindowDrawList();

            int id = ImGui.ImGuiWindowGetID(parent_window, label, (byte*)null);
            if (!ImGui.BeginChild(id, size, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.EndChild();
                return -1;
            }

            int hovered_idx = -1;
            if (newCount != null)
            {
                *newCount = pointsCount;
            }

            ImGuiWindow* window = ImGui.GetCurrentWindow();
            if (window->SkipItems != 0)
            {
                ImGui.EndChild();
                return -1;
            }

            Vector2 points_min = new(float.MaxValue, float.MaxValue);
            Vector2 points_max = new(-float.MaxValue, -float.MaxValue);
            for (int point_idx = 0; point_idx < pointsCount; ++point_idx)
            {
                Vector2 point;
                if ((flags & CurveEditorFlags.NoTangents) != 0)
                {
                    point = values[point_idx];
                }
                else
                {
                    point = values[1 + point_idx * 3];
                }
                points_max = Vector2.Max(points_max, point);
                points_min = Vector2.Min(points_min, point);
            }
            points_max.Y = MathF.Max(points_max.Y, points_min.Y + 0.0001f);

            float from_x = window->StateStorage.GetFloat((int)StorageValues.FROM_X, points_min.X);
            float from_y = window->StateStorage.GetFloat((int)StorageValues.FROM_Y, points_min.Y);
            float width = window->StateStorage.GetFloat((int)StorageValues.WIDTH, points_max.X - points_min.X);
            float height = window->StateStorage.GetFloat((int)StorageValues.HEIGHT, points_max.Y - points_min.Y);
            window->StateStorage.SetFloat((int)StorageValues.FROM_X, from_x);
            window->StateStorage.SetFloat((int)StorageValues.FROM_Y, from_y);
            window->StateStorage.SetFloat((int)StorageValues.WIDTH, width);
            window->StateStorage.SetFloat((int)StorageValues.HEIGHT, height);

            Vector2 beg_pos = ImGui.GetCursorScreenPos();

            ImRect inner_bb = window->InnerRect;
            ImRect frame_bb = new() { Min = inner_bb.Min - style.FramePadding, Max = inner_bb.Max + style.FramePadding };

            Vector2 transform(Vector2 pos)
            {
                float x = (pos.X - from_x) / width;
                float y = (pos.Y - from_y) / height;

                return new Vector2(inner_bb.Min.X * (1 - x) + inner_bb.Max.X * x, inner_bb.Min.Y * y + inner_bb.Max.Y * (1 - y));
            }

            Vector2 invTransform(Vector2 pos)
            {
                float x = (pos.X - inner_bb.Min.X) / (inner_bb.Max.X - inner_bb.Min.X);
                float y = (inner_bb.Max.Y - pos.Y) / (inner_bb.Max.Y - inner_bb.Min.Y);

                return new Vector2(from_x + width * x, from_y + height * y);
            }

            if ((flags & CurveEditorFlags.ShowGrid) != 0)
            {
                int exp = 0;
                MathUtil.Frexp(width / 5, ref exp);
                float step_x = (float)MathUtil.Ldexp(1.0, exp);
                int cell_cols = (int)(width / step_x);

                float x = step_x * (int)(from_x / step_x);
                byte* buf = stackalloc byte[64];
                for (int i = -1; i < cell_cols + 2; ++i)
                {
                    Vector2 a = transform(new(x + i * step_x, from_y));
                    Vector2 b = transform(new(x + i * step_x, from_y + height));
                    DrawList->AddLine(a, b, 0x55000000);

                    if (exp > 0)
                    {
                        int param = (int)(x + i * step_x);
                        ImGui.ImFormatStringV(buf, 64, " %d", (nuint)(&param));
                    }
                    else
                    {
                        float param = x + i * step_x;

                        ImGui.ImFormatStringV(buf, 64, " %f", (nuint)(&param));
                    }
                    DrawList->AddText(b, 0x55000000, buf);
                }

                MathUtil.Frexp(height / 5, ref exp);
                float step_y = (float)MathUtil.Ldexp(1.0, exp);
                int cell_rows = (int)(height / step_y);

                float y = step_y * (int)(from_y / step_y);

                for (int i = -1; i < cell_rows + 2; ++i)
                {
                    Vector2 a = transform(new(from_x, y + i * step_y));
                    Vector2 b = transform(new(from_x + width, y + i * step_y));
                    DrawList->AddLine(a, b, 0x55000000);

                    if (exp > 0)
                    {
                        int param = (int)(y + i * step_y);
                        ImGui.ImFormatStringV(buf, 64, " %d", (nuint)(&param));
                    }
                    else
                    {
                        float param = y + i * step_y;
                        ImGui.ImFormatStringV(buf, 64, " %f", (nuint)(&param));
                    }
                    DrawList->AddText(a, 0x55000000, buf);
                }
            }

            if (ImGui.GetIO().MouseWheel != 0 && ImGui.IsItemHovered())
            {
                float scale = MathF.Pow(2, ImGui.GetIO().MouseWheel);
                width *= scale;
                height *= scale;
                window->StateStorage.SetFloat((int)StorageValues.WIDTH, width);
                window->StateStorage.SetFloat((int)StorageValues.HEIGHT, height);
            }
            if (ImGui.IsMouseReleased((ImGuiMouseButton)1))
            {
                window->StateStorage.SetBool((int)StorageValues.IS_PANNING, false);
            }
            if (window->StateStorage.GetBool((int)StorageValues.IS_PANNING, false))
            {
                Vector2 drag_offset = ImGui.GetMouseDragDelta(1);
                from_x = start_pan.X;
                from_y = start_pan.Y;
                from_x -= drag_offset.X * width / (inner_bb.Max.X - inner_bb.Min.X);
                from_y += drag_offset.Y * height / (inner_bb.Max.Y - inner_bb.Min.Y);
                window->StateStorage.SetFloat((int)StorageValues.FROM_X, from_x);
                window->StateStorage.SetFloat((int)StorageValues.FROM_Y, from_y);
            }
            else if (ImGui.IsMouseDragging((ImGuiMouseButton)1) && ImGui.IsItemHovered())
            {
                window->StateStorage.SetBool((int)StorageValues.IS_PANNING, true);
                start_pan.X = from_x;
                start_pan.Y = from_y;
            }

            int changed_idx = -1;
            for (int point_idx = pointsCount - 2; point_idx >= 0; --point_idx)
            {
                Vector2* points;
                if ((flags & CurveEditorFlags.NoTangents) != 0)
                {
                    points = ((Vector2*)values) + point_idx;
                }
                else
                {
                    points = ((Vector2*)values) + 1 + point_idx * 3;
                }

                Vector2 p_prev = points[0];
                Vector2 tangent_last = default;
                Vector2 tangent = default;
                Vector2 p;
                if ((flags & CurveEditorFlags.NoTangents) != 0)
                {
                    p = points[1];
                }
                else
                {
                    tangent_last = points[1];
                    tangent = points[2];
                    p = points[3];
                }

                bool handlePoint(Vector2 p, int idx)
                {
                    Vector2 cursor_pos = ImGui.GetCursorScreenPos();
                    Vector2 pos = transform(p);

                    ImGui.SetCursorScreenPos(pos - new Vector2(SIZE, SIZE));
                    ImGui.PushID(idx);
                    ImGui.InvisibleButton("", new Vector2(2 * NODE_SLOT_RADIUS, 2 * NODE_SLOT_RADIUS));

                    uint col = ImGui.IsItemActive() || ImGui.IsItemHovered() ? ImGui.GetColorU32(ImGuiCol.PlotLinesHovered) : ImGui.GetColorU32(ImGuiCol.PlotLines);

                    DrawList->AddLine(pos + new Vector2(-SIZE, 0), pos + new Vector2(0, SIZE), col);
                    DrawList->AddLine(pos + new Vector2(SIZE, 0), pos + new Vector2(0, SIZE), col);
                    DrawList->AddLine(pos + new Vector2(SIZE, 0), pos + new Vector2(0, -SIZE), col);
                    DrawList->AddLine(pos + new Vector2(-SIZE, 0), pos + new Vector2(0, -SIZE), col);

                    if (ImGui.IsItemHovered())
                    {
                        hovered_idx = point_idx + idx;
                    }

                    bool changed = false;
                    if (ImGui.IsItemActive() && ImGui.IsMouseClicked(0))
                    {
                        window->StateStorage.SetFloat((int)StorageValues.POINT_START_X, pos.X);
                        window->StateStorage.SetFloat((int)StorageValues.POINT_START_Y, pos.Y);
                    }

                    if (ImGui.IsItemHovered() || ImGui.IsItemActive() && ImGui.IsMouseDragging(0))
                    {
                        byte* tmp = stackalloc byte[64];

                        ImGui.ImFormatStringV(tmp, 64, "%0.2f, %0.2f", (nuint)(&p));
                        DrawList->AddText(new Vector2(pos.X, pos.Y - ImGui.GetTextLineHeight()), 0xff000000, tmp);
                    }

                    if (ImGui.IsItemActive() && ImGui.IsMouseDragging(0))
                    {
                        pos.X = window->StateStorage.GetFloat((int)StorageValues.POINT_START_X, pos.X);
                        pos.Y = window->StateStorage.GetFloat((int)StorageValues.POINT_START_Y, pos.Y);
                        pos += ImGui.GetMouseDragDelta();
                        Vector2 v = invTransform(pos);

                        p = v;
                        changed = true;
                    }
                    ImGui.PopID();

                    ImGui.SetCursorScreenPos(cursor_pos);
                    return changed;
                }

                bool handleTangent(Vector2 t, Vector2 p, int idx)
                {
                    Vector2 normalized(Vector2 v)
                    {
                        float len = 1.0f / MathF.Sqrt(v.X * v.X + v.Y * v.Y);
                        return new Vector2(v.X * len, v.Y * len);
                    }

                    Vector2 cursor_pos = ImGui.GetCursorScreenPos();
                    Vector2 pos = transform(p);
                    Vector2 tang = pos + normalized(new Vector2(t.X, -t.Y)) * LENGTH;

                    ImGui.SetCursorScreenPos(tang - new Vector2(SIZE_handleTangent, SIZE_handleTangent));
                    ImGui.PushID(-idx);
                    ImGui.InvisibleButton("", new Vector2(2 * NODE_SLOT_RADIUS, 2 * NODE_SLOT_RADIUS));

                    DrawList->AddLine(pos, tang, ImGui.GetColorU32(ImGuiCol.PlotLines));

                    uint col = ImGui.IsItemHovered() ? ImGui.GetColorU32(ImGuiCol.PlotLinesHovered) : ImGui.GetColorU32(ImGuiCol.PlotLines);

                    DrawList->AddLine(tang + new Vector2(-SIZE_handleTangent, SIZE_handleTangent), tang + new Vector2(SIZE_handleTangent, SIZE_handleTangent), col);
                    DrawList->AddLine(tang + new Vector2(SIZE_handleTangent, SIZE_handleTangent), tang + new Vector2(SIZE_handleTangent, -SIZE_handleTangent), col);
                    DrawList->AddLine(tang + new Vector2(SIZE_handleTangent, -SIZE_handleTangent), tang + new Vector2(-SIZE_handleTangent, -SIZE_handleTangent), col);
                    DrawList->AddLine(tang + new Vector2(-SIZE_handleTangent, -SIZE_handleTangent), tang + new Vector2(-SIZE_handleTangent, SIZE_handleTangent), col);

                    bool changed = false;
                    if (ImGui.IsItemActive() && ImGui.IsMouseDragging(0))
                    {
                        tang = ImGui.GetIO().MousePos - pos;
                        tang = normalized(tang);
                        tang.Y *= -1;

                        t = tang;
                        changed = true;
                    }
                    ImGui.PopID();

                    ImGui.SetCursorScreenPos(cursor_pos);
                    return changed;
                };

                ImGui.PushID(point_idx);
                if ((flags & CurveEditorFlags.NoTangents) == 0)
                {
                    DrawList->AddBezierCubic(transform(p_prev), transform(p_prev + tangent_last),
                                                     transform(p + tangent), transform(p), ImGui.GetColorU32(ImGuiCol.PlotLines),
                                                     1.0f, 20);
                    if (handleTangent(tangent_last, p_prev, 0))
                    {
                        points[1] = Vector2.Clamp(tangent_last, new Vector2(0, -1), new Vector2(1, 1));
                        changed_idx = point_idx;
                    }
                    if (handleTangent(tangent, p, 1))
                    {
                        points[2] = Vector2.Clamp(tangent, new Vector2(-1, -1), new Vector2(0, 1));
                        changed_idx = point_idx + 1;
                    }
                    if (handlePoint(p, 1))
                    {
                        if (p.X <= p_prev.X)
                        {
                            p.X = p_prev.X + 0.001f;
                        }

                        if (point_idx < pointsCount - 2 && p.X >= points[6].X)
                        {
                            p.X = points[6].X - 0.001f;
                        }
                        points[3] = p;
                        changed_idx = point_idx + 1;
                    }
                }
                else
                {
                    DrawList->AddLine(transform(p_prev), transform(p), ImGui.GetColorU32(ImGuiCol.PlotLines), 1.0f);
                    if (handlePoint(p, 1))
                    {
                        if (p.X <= p_prev.X)
                        {
                            p.X = p_prev.X + 0.001f;
                        }

                        if (point_idx < pointsCount - 2 && p.X >= points[2].X)
                        {
                            p.X = points[2].X - 0.001f;
                        }
                        points[1] = p;
                        changed_idx = point_idx + 1;
                    }
                }
                if (point_idx == 0)
                {
                    if (handlePoint(p_prev, 0))
                    {
                        if (p.X <= p_prev.X)
                        {
                            p_prev.X = p.X - 0.001f;
                        }

                        points[0] = p_prev;
                        changed_idx = point_idx;
                    }
                }
                ImGui.PopID();
            }

            ImGui.SetCursorScreenPos(inner_bb.Min);

            ImGui.InvisibleButton("bg", inner_bb.Max - inner_bb.Min);

            if (ImGui.IsItemActive() && ImGui.IsMouseDoubleClicked(0) && newCount != null)
            {
                Vector2 mp = ImGui.GetMousePos();
                Vector2 new_p = invTransform(mp);
                Vector2* points = (Vector2*)values;

                if ((flags & CurveEditorFlags.NoTangents) == 0)
                {
                    points[pointsCount * 3 + 0] = new Vector2(-0.2f, 0);
                    points[pointsCount * 3 + 1] = new_p;
                    points[pointsCount * 3 + 2] = new Vector2(0.2f, 0);
                    ;
                    ++*newCount;

                    QSort(values, pointsCount + 1, sizeof(Vector2) * 3, (Pointer a, Pointer b) =>
                    {
                        float fa = (((Vector2*)a) + 1)->X;
                        float fb = (((Vector2*)b) + 1)->X;
                        return fa < fb ? -1 : (fa > fb) ? 1 : 0;
                    });
                }
                else
                {
                    points[pointsCount] = new_p;
                    ++*newCount;

                    QSort(values, pointsCount + 1, sizeof(Vector2), (Pointer a, Pointer b) =>
                    {
                        float fa = ((Vector2*)a)->X;
                        float fb = ((Vector2*)b)->X;
                        return fa < fb ? -1 : (fa > fb) ? 1 : 0;
                    });
                }
            }

            if (hovered_idx >= 0 && ImGui.IsMouseDoubleClicked(0) && (newCount != null) && pointsCount > 2)
            {
                Vector2* points = (Vector2*)values;
                --*newCount;
                if ((flags & CurveEditorFlags.NoTangents) == 0)
                {
                    for (int j = hovered_idx * 3; j < pointsCount * 3 - 3; j += 3)
                    {
                        points[j + 0] = points[j + 3];
                        points[j + 1] = points[j + 4];
                        points[j + 2] = points[j + 5];
                    }
                }
                else
                {
                    for (int j = hovered_idx; j < pointsCount - 1; ++j)
                    {
                        points[j] = points[j + 1];
                    }
                }
            }

            ImGui.EndChild();
            ImGui.RenderText(new Vector2(frame_bb.Max.X + style.ItemInnerSpacing.X, inner_bb.Min.Y), label, (byte*)null, true);
            return changed_idx;
        }
    }
}