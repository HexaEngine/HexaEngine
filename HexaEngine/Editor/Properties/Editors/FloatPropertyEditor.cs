namespace HexaEngine.Editor.Properties.Editors
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Properties;
    using Hexa.NET.ImGui;
    using System.Reflection;

    public class FloatPropertyEditor : IPropertyEditor
    {
        private readonly ImGuiName guiName;
        private readonly EditorPropertyMode mode;
        private readonly float min;
        private readonly float max;

        public FloatPropertyEditor(string name, PropertyInfo property, EditorPropertyMode mode, float min, float max)
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
#pragma warning disable CS8605 // Unboxing a possibly null value.
            float val = (float)value;
#pragma warning restore CS8605 // Unboxing a possibly null value.
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(guiName.Name);
            ImGui.TableSetColumnIndex(1);
            ImGui.PushItemWidth(0);
            switch (mode)
            {
                case EditorPropertyMode.Default:
                    if (ImGui.InputFloat(guiName.Id, ref val))
                    {
                        value = val;
                        ImGui.PopItemWidth();
                        return true;
                    }
                    break;

                case EditorPropertyMode.Slider:
                    if (ImGui.SliderFloat(guiName.Id, ref val, min, max))
                    {
                        value = val;
                        ImGui.PopItemWidth();
                        return true;
                    }
                    break;

                case EditorPropertyMode.SliderAngle:
                    if (ImGui.SliderAngle(guiName.Id, ref val, min, max))
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