namespace HexaEngine.Editor.Editors
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Properties;
    using Hexa.NET.ImGui;
    using System.Reflection;
    using System.Collections;
    using HexaEngine.Mathematics;

    public class FloatPropertyEditor : IPropertyEditor
    {
        private readonly ImGuiName guiName;
        private readonly EditorPropertyMode mode;
        private readonly float min = float.MinValue;
        private readonly float max = float.MaxValue;

        public FloatPropertyEditor(string name, PropertyInfo property, EditorPropertyMode mode, object? min, object? max)
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

            if (min is float fMin)
            {
                this.min = fMin;
            }

            if (max is float fMax)
            {
                this.max = fMax;
            }

            if (min is double dMin)
            {
                this.min = (float)dMin;
            }

            if (max is double dMax)
            {
                this.max = (float)dMax;
            }

            if (mode == EditorPropertyMode.SliderAngle)
            {
                this.min = this.min.ToDeg();
                this.max = this.max.ToDeg();
            }

            if (mode == EditorPropertyMode.SliderAngle && min == null)
            {
                this.min = -360;
            }

            if (mode == EditorPropertyMode.SliderAngle && max == null)
            {
                this.max = 360;
            }
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