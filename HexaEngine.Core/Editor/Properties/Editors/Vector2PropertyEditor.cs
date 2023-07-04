namespace HexaEngine.Editor.Properties.Editors
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Editor.Properties;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using ImGuiNET;
    using System;
    using System.Numerics;
    using System.Reflection;

    public class Vector2PropertyEditor : IPropertyEditor
    {
        private ImGuiName guiName;
        private readonly EditorPropertyMode mode;
        private readonly float min;
        private readonly float max;

        public Vector2PropertyEditor(string name, PropertyInfo property, EditorPropertyMode mode, float min, float max)
        {
            Name = name;
            Property = property;
            guiName = new(name);
            this.mode = mode;
            this.min = min;
            this.max = max;
        }

        public string Name { get; }
        public PropertyInfo Property { get; }

        public bool Draw(IGraphicsContext context, object instance, ref object? value)
        {
            Vector2 val = (Vector2)value;

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(guiName.Name);
            ImGui.TableSetColumnIndex(1);
            ImGui.PushItemWidth(ImGui.GetColumnWidth());
            switch (mode)
            {
                case EditorPropertyMode.Default:
                    if (ImGui.InputFloat2(guiName.Id, ref val))
                    {
                        value = val;
                        ImGui.PopItemWidth();
                        return true;
                    }
                    break;

                case EditorPropertyMode.Slider:
                    if (ImGui.SliderFloat2(guiName.Id, ref val, min, max))
                    {
                        value = val;
                        ImGui.PopItemWidth();
                        return true;
                    }
                    break;
            }
            ImGui.PopItemWidth();
            return false;
        }
    }
}