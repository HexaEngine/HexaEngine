namespace HexaEngine.Editor.Editors
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Properties;
    using System.Reflection;

    public class UIntPropertyEditor : IPropertyEditor
    {
        private readonly ImGuiName guiName;
        private readonly EditorPropertyMode mode;
        private readonly uint min = uint.MinValue;
        private readonly uint max = uint.MaxValue;

        public UIntPropertyEditor(string name, PropertyInfo property, EditorPropertyMode mode, object? min, object? max)
        {
            Name = name;
            Property = property;
            guiName = new(name);
            this.mode = mode;

            if (min is uint iMin)
            {
                this.min = iMin;
            }

            if (max is uint iMax)
            {
                this.max = iMax;
            }
        }

        public string Name { get; }

        public PropertyInfo Property { get; }

        public unsafe bool Draw(IGraphicsContext context, object instance, ref object? value)
        {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            uint val = (uint)value;
#pragma warning restore CS8605 // Unboxing a possibly null value.
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(guiName.Name);
            ImGui.TableSetColumnIndex(1);
            ImGui.PushItemWidth(0);
            switch (mode)
            {
                case EditorPropertyMode.Default:
                    if (ImGui.DragScalar(guiName.Id, ImGuiDataType.U32, &val))
                    {
                        value = val;
                        ImGui.PopItemWidth();
                        return true;
                    }
                    break;

                case EditorPropertyMode.Slider:
                    uint minI = min;
                    uint maxI = max;
                    if (ImGui.SliderScalar(guiName.Id, ImGuiDataType.U32, &val, &minI, &maxI))
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