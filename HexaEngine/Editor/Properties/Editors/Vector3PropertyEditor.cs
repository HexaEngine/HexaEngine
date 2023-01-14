namespace HexaEngine.Editor.Properties.Editors
{
    using HexaEngine.Editor.Attributes;
    using ImGuiNET;
    using NLua;
    using System;
    using System.Numerics;

    public class Vector3PropertyEditor : IPropertyEditor
    {
        private readonly string name;
        private readonly EditorPropertyMode mode;
        private readonly float min;
        private readonly float max;

        public Vector3PropertyEditor(string name, EditorPropertyMode mode, float min, float max)
        {
            this.name = name;
            this.mode = mode;
            this.min = min;
            this.max = max;
        }

        public bool Draw(object instance, ref object? value)
        {
            Vector3 val = (Vector3)value;

            switch (mode)
            {
                case EditorPropertyMode.Default:
                    if (ImGui.InputFloat3(name, ref val))
                    {
                        value = val;
                        return true;
                    }
                    break;

                case EditorPropertyMode.Colorpicker:
                    if (ImGui.ColorEdit3(name, ref val))
                    {
                        value = val;
                        return true;
                    }
                    break;

                case EditorPropertyMode.Slider:
                    if (ImGui.SliderFloat3(name, ref val, min, max))
                    {
                        value = val;
                        return true;
                    }
                    break;
            }

            return false;
        }
    }
}