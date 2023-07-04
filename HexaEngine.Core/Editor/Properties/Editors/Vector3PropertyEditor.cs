namespace HexaEngine.Editor.Properties.Editors
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Editor.Properties;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Reflection;
    using HexaEngine.Core.UI;
    using ImGuiNET;
    using Newtonsoft.Json.Linq;
    using System.Numerics;
    using System.Reflection;

    public class Vector3PropertyEditor : IPropertyEditor
    {
        private ImGuiName guiName;
        private readonly EditorPropertyMode mode;
        private readonly float min;
        private readonly float max;

        public Vector3PropertyEditor(string name, PropertyInfo property, EditorPropertyMode mode, float min, float max)
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
            Vector3 val = (Vector3)value;

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(guiName.Name);
            ImGui.TableSetColumnIndex(1);
            ImGui.PushItemWidth(0);
            switch (mode)
            {
                case EditorPropertyMode.Default:
                    if (ImGui.InputFloat3(guiName.Id, ref val))
                    {
                        value = val;
                        ImGui.PopItemWidth();
                        return true;
                    }
                    break;

                case EditorPropertyMode.Colorpicker:
                    if (ImGui.ColorEdit3(guiName.Id, ref val))
                    {
                        value = val;
                        ImGui.PopItemWidth();
                        return true;
                    }
                    break;

                case EditorPropertyMode.Slider:
                    if (ImGui.SliderFloat3(guiName.Id, ref val, min, max))
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