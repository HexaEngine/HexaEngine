namespace HexaEngine.Editor.Editors
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Properties;
    using System.Reflection;

    public class IntPropertyEditor : IPropertyEditor
    {
        private readonly ImGuiName guiName;
        private readonly EditorPropertyMode mode;
        private readonly int min = int.MinValue;
        private readonly int max = int.MaxValue;

        public IntPropertyEditor(string name, PropertyInfo property, EditorPropertyMode mode, object? min, object? max)
        {
            Name = name;
            Property = property;
            guiName = new(name);
            this.mode = mode;

            if (min is int iMin)
            {
                this.min = iMin;
            }

            if (max is int iMax)
            {
                this.max = iMax;
            }
        }

        public string Name { get; }

        public PropertyInfo Property { get; }

        public bool Draw(IGraphicsContext context, object instance, ref object? value)
        {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            int val = (int)value;
#pragma warning restore CS8605 // Unboxing a possibly null value.
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(guiName.Name);
            ImGui.TableSetColumnIndex(1);
            ImGui.PushItemWidth(0);
            switch (mode)
            {
                case EditorPropertyMode.Default:
                    if (ImGui.DragInt(guiName.Id, ref val))
                    {
                        value = val;
                        ImGui.PopItemWidth();
                        return true;
                    }
                    break;

                case EditorPropertyMode.Slider:
                    if (ImGui.SliderInt(guiName.Id, ref val, min, max))
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