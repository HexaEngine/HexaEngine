namespace HexaEngine.Editor.Properties.Editors
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Editor.Properties;
    using ImGuiNET;
    using System.Numerics;

    public class Vector4PropertyEditor : IPropertyEditor
    {
        private readonly string name;
        private readonly EditorPropertyMode mode;
        private readonly float min;
        private readonly float max;

        public Vector4PropertyEditor(string name, EditorPropertyMode mode, float min, float max)
        {
            this.name = name;
            this.mode = mode;
            this.min = min;
            this.max = max;
        }

        public bool Draw(object instance, ref object? value)
        {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            Vector4 val = (Vector4)value;
#pragma warning restore CS8605 // Unboxing a possibly null value.

            switch (mode)
            {
                case EditorPropertyMode.Default:
                    if (ImGui.InputFloat4(name, ref val))
                    {
                        value = val;
                        return true;
                    }
                    break;

                case EditorPropertyMode.Colorpicker:
                    if (ImGui.ColorEdit4(name, ref val))
                    {
                        value = val;
                        return true;
                    }
                    break;

                case EditorPropertyMode.Slider:
                    if (ImGui.SliderFloat4(name, ref val, min, max))
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