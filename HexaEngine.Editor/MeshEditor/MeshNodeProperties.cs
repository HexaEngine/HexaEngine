namespace HexaEngine.Editor.MeshEditor
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.UI;
    using Hexa.NET.ImGui;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public class MeshNodeProperties : EditorWindow
    {
        private readonly MeshEditorWindow editor;
        private readonly MeshNodes nodes;

        public MeshNodeProperties(MeshEditorWindow editor, MeshNodes nodes)
        {
            IsShown = true;
            this.editor = editor;
            this.nodes = nodes;
        }

        protected override string Name => "Node Properties";

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            if (editor.Current == null || nodes.Selected == null)
                return;

            var selected = nodes.Selected;

            ImGui.InputText("Name", ref selected.Name, 2048);
            Matrix4x4.Decompose(selected.Transform, out Vector3 scale, out Quaternion rotation, out Vector3 translation);
            if (TransformEdit("Transform", ref translation, ref rotation, ref scale))
            {
                var newMatrix = Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(translation); ;
                History.Default.Do("Transform Object", selected, selected.Transform, newMatrix, SetMatrix, RestoreMatrix);
            }
            var flagString = selected.Flags.ToString();
            ImGui.InputText("Flags", ref flagString, 256, ImGuiInputTextFlags.ReadOnly);

            if (selected.Meshes.Count != 0)
            {
                if (ImGui.TreeNodeEx($"Meshes"))
                {
                    for (int i = 0; i < selected.Meshes.Count; i++)
                    {
                        var id = selected.Meshes[i];
                        var data = editor.Current.Meshes[(int)id];
                        if (ImGui.TreeNodeEx($"Mesh data: {id}"))
                        {
                            Models($"Mesh data: {id}", data);

                            ImGui.TreePop();
                        }
                    }
                    ImGui.TreePop();
                }
            }
        }

        public static unsafe void Models(string label, MeshData data)
        {
            var g = ImGui.GetCurrentContext();
            var window = ImGui.GetCurrentWindow();
            var style = ImGui.GetStyle();
            int id = ImGui.ImGuiWindowGetID(window, label, (byte*)null);

            Vector2 frame_size = ImGui.CalcItemSize(default, ImGui.CalcItemWidth(), g.FontSize * 5f + style.FramePadding.Y * 2.0f); // Arbitrary default of 8 lines high for multi-line

            ImGui.PushStyleColor(ImGuiCol.ChildBg, style.Colors[(int)ImGuiCol.FrameBg]);
            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, style.FrameRounding);
            ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, style.FrameBorderSize);

            bool child_visible = ImGui.BeginChildEx(label, id, frame_size, true, ImGuiWindowFlags.NoMove);

            ImGui.PopStyleVar(2);
            ImGui.PopStyleColor();

            if (child_visible)
            {
                ImGui.Text($"Vertices: {data.VerticesCount}");
                TooltipHelper.Tooltip($"Vertices: {data.VerticesCount}");
                ImGui.Text($"Indices: {data.IndicesCount}");
                TooltipHelper.Tooltip($"Indices: {data.IndicesCount}");
                ImGui.Text($"Flags: {data.Flags}");
                TooltipHelper.Tooltip($"Flags: {data.Flags}");
            }

            ImGui.EndChild();
        }

        public static unsafe bool TransformEdit(string label, ref Vector3 translation, ref Quaternion rotation, ref Vector3 scale)
        {
            bool changed = false;

            var window = ImGui.GetCurrentWindow();
            var style = ImGui.GetStyle();

            int id = ImGui.ImGuiWindowGetID(window, label, (byte*)null);
            Vector2 label_size = ImGui.CalcTextSize(label, true);
            Vector2 frame_size = ImGui.CalcItemSize(default, ImGui.CalcItemWidth(), label_size.Y + style.FramePadding.Y * 2.0f); // Arbitrary default of 8 lines high for multi-line
            Vector2 total_size = new(frame_size.X + (label_size.X > 0.0f ? style.ItemInnerSpacing.X + label_size.X : 0.0f), frame_size.Y);

            ImRect frame_bb = new() { Min = window.DC.CursorPos, Max = window.DC.CursorPos + frame_size };
            ImRect total_bb = new() { Min = frame_bb.Min, Max = frame_bb.Min + total_size };

            ImGui.PushStyleColor(ImGuiCol.ChildBg, style.Colors[(int)ImGuiCol.FrameBg]);
            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, style.FrameRounding);
            ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, style.FrameBorderSize);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));

            bool child_visible = ImGui.BeginChildEx(label, id, ImGui.ImRectGetSize(ref frame_bb), true, ImGuiWindowFlags.NoMove);

            ImGui.PopStyleVar(3);
            ImGui.PopStyleColor();

            if (child_visible)
            {
                ImGui.Text($"<Pos: {translation}, Rot: {rotation.ToYawPitchRoll().ToDeg()}, Scale: {scale}>");
                ImGui.OpenPopupOnItemClick("TransformEdit", ImGuiPopupFlags.MouseButtonLeft);

                if (ImGui.BeginPopup("TransformEdit"))
                {
                    {
                        var val = translation;
                        if (ImGui.InputFloat3("Position", ref val))
                        {
                            changed = true;
                            translation = val;
                        }
                    }
                    {
                        var val = rotation.ToYawPitchRoll().ToDeg();
                        if (ImGui.InputFloat3("Rotation", ref val))
                        {
                            changed = true;
                            rotation = val.ToRad().ToQuaternion();
                        }
                    }
                    {
                        var val = scale;
                        if (ImGui.InputFloat3("Scale", ref val))
                        {
                            changed = true;
                            scale = val;
                        }
                    }

                    ImGui.EndPopup();
                }
            }

            ImGui.EndChild();

            ImGui.SameLine();
            ImGui.TextUnformatted(label);

            return changed;
        }

        private static void SetMatrix(object context)
        {
            var ctx = (HistoryContext<Node, Matrix4x4>)context;
            ctx.Target.Transform = ctx.NewValue;
        }

        private static void RestoreMatrix(object context)
        {
            var ctx = (HistoryContext<Node, Matrix4x4>)context;
            ctx.Target.Transform = ctx.OldValue;
        }
    }
}