namespace HexaEngine.Editor.Properties.Editors
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Reflection;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Properties;
    using ImGuiNET;
    using Newtonsoft.Json.Linq;
    using System.Numerics;
    using System.Reflection;

    public class Vector4PropertyEditor : IPropertyEditor
    {
        private readonly ImGuiName guiName;
        private readonly EditorPropertyMode mode;
        private readonly float min;
        private readonly float max;

        public Vector4PropertyEditor(string name, PropertyInfo property, EditorPropertyMode mode, float min, float max)
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
            Vector4 val = (Vector4)value;

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(guiName.Name);
            ImGui.TableSetColumnIndex(1);
            ImGui.PushItemWidth(-1);
            switch (mode)
            {
                case EditorPropertyMode.Default:
                    if (ImGui.InputFloat4(guiName.Id, ref val))
                    {
                        value = val;
                        ImGui.PopItemWidth();
                        return true;
                    }
                    break;

                case EditorPropertyMode.Colorpicker:
                    if (ImGui.ColorEdit4(guiName.Id, ref val))
                    {
                        value = val;
                        ImGui.PopItemWidth();
                        return true;
                    }
                    break;

                case EditorPropertyMode.Slider:
                    if (ImGui.SliderFloat4(guiName.Id, ref val, min, max))
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