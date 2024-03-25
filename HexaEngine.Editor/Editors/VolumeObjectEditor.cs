﻿namespace HexaEngine.Editor.Editors
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Properties;
    using HexaEngine.PostFx;
    using HexaEngine.Volumes;
    using System;
    using System.Reflection;

    public class VolumeObjectEditor : IObjectEditor
    {
        private readonly Dictionary<PropertyInfo, IPropertyEditor?> editorCache = new();
        private ImGuiName guiName = new("Volume");

        public string Name => "Volume";

        public Type Type => typeof(Volume);

        public object? Instance { get; set; }

        public bool IsEmpty => false;

        public bool IsHidden => false;

        public bool NoTable { get; set; }

        public unsafe bool Draw(IGraphicsContext context)
        {
            PostProcessingManager? postManager = PostProcessingManager.Current;
            if (postManager == null || Instance is not Volume volume)
            {
                return false;
            }

            bool changed = false;

            if (!NoTable)
            {
                ImGui.BeginTable("Volume", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.PreciseWidths);
                ImGui.TableSetupColumn("", 0.0f);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);
            }

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Volume Mode");
            ImGui.TableSetColumnIndex(1);

            var mode = volume.Mode;
            if (ComboEnumHelper<VolumeMode>.Combo("##Mode", ref mode))
            {
                volume.Mode = mode;
            }

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Volume Shape");
            ImGui.TableSetColumnIndex(1);

            var shape = volume.Shape;
            if (ComboEnumHelper<VolumeShape>.Combo("##Shape", ref shape))
            {
                volume.Shape = shape;
            }

            switch (shape)
            {
                case VolumeShape.Box:

                    break;

                case VolumeShape.Sphere:
                    break;
            }

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.EndTable();

            ImGui.Separator();

            if (!NoTable)
            {
                ImGui.BeginTable(guiName.RawId, 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.PreciseWidths);
                ImGui.TableSetupColumn("", 0.0f);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);
            }

            for (int i = 0; i < postManager.Effects.Count; i++)
            {
                var effect = postManager.Effects[i];
                var proxy = volume.Container[effect];

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                int id = ImGui.GetID(effect.DisplayName.Id);

                if (ImGui.TreeNodeEx(effect.DisplayName.Id))
                {
                    DrawEnable(id, effect, proxy, true);

                    ImGui.TableSetColumnIndex(0);
                    for (int j = 0; j < proxy.Properties.Count; j++)
                    {
                        var property = proxy.Properties[j];

                        if (property.Name == "Initialized")
                            continue;

                        if (property.Name == "Enabled")
                            continue;

                        if (property.Name == "Name")
                            continue;

                        var value = proxy.Data[property.Name];

                        if (!editorCache.TryGetValue(property, out var editor))
                        {
                            editor = ObjectEditorFactory.CreatePropertyEditor(property);
                            editorCache.Add(property, editor);
                        }

                        if (editor != null && editor.Draw(context, effect, ref value))
                        {
                            proxy.Data[property.Name] = value;

                            if (mode == VolumeMode.Global)
                            {
                                property.SetValue(effect, value);
                            }

                            changed = true;
                        }
                    }
                    ImGui.TreePop();

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Separator();
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Separator();
                }
                else
                {
                    DrawEnable(id, effect, proxy, false);

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Separator();
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Separator();
                }
            }

            if (!NoTable)
            {
                ImGui.EndTable();
            }

            return changed;
        }

        private static void DrawEnable(int id, IPostFx effect, PostFxProxy proxy, bool isOpen)
        {
            ImGui.SameLine();
            ImGui.Dummy(new(1, 0));
            ImGui.SameLine();

            if ((effect.Flags & (PostFxFlags.AlwaysEnabled | PostFxFlags.Optional)) == 0)
            {
                bool enabled = effect.Enabled;
                if (ImGui.Checkbox($"{effect.DisplayName.Id}Enable", ref enabled))
                {
                    effect.Enabled = enabled;
                    proxy.Enabled = enabled;
                }
                ImGui.SameLine();
                ImGui.Text(effect.DisplayName.Name);
            }
            else
            {
                ImGui.Text(effect.DisplayName.Name);
            }

            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                ImGui.TreeNodeSetOpen(id, !isOpen);
            }

            ImGui.Dummy(new(0, 1));
        }

        public void Dispose()
        {
        }
    }
}