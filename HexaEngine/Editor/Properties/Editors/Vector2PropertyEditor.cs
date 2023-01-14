namespace HexaEngine.Editor.Properties.Editors
{
    using HexaEngine.Editor.Attributes;
    using ImGuiNET;
    using System.Numerics;

    public class Vector2PropertyEditor : IPropertyEditor
    {
        private readonly string name;
        private readonly EditorPropertyMode mode;
        private readonly float min;
        private readonly float max;

        public Vector2PropertyEditor(string name, EditorPropertyMode mode, float min, float max)
        {
            this.name = name;
            this.mode = mode;
            this.min = min;
            this.max = max;
        }

        public bool Draw(object instance, ref object? value)
        {
            Vector2 val = (Vector2)value;

            switch (mode)
            {
                case EditorPropertyMode.Default:
                    if (ImGui.InputFloat2(name, ref val))
                    {
                        value = val;
                        return true;
                    }
                    break;

                case EditorPropertyMode.Slider:
                    if (ImGui.SliderFloat2(name, ref val, min, max))
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