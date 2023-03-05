namespace HexaEngine.Editor.Properties.Editors
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Editor.Properties;
    using ImGuiNET;

    public class FloatPropertyEditor : IPropertyEditor
    {
        private readonly string id;
        private readonly string name;
        private readonly EditorPropertyMode mode;
        private readonly float min;
        private readonly float max;

        public FloatPropertyEditor(string name, EditorPropertyMode mode, float min, float max)
        {
            this.id = Guid.NewGuid().ToString();
            this.name = name;
            this.mode = mode;
            this.min = min;
            this.max = max;
        }

        public bool Draw(object instance, ref object? value)
        {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            float val = (float)value;
#pragma warning restore CS8605 // Unboxing a possibly null value.

            switch (mode)
            {
                case EditorPropertyMode.Default:
                    if (ImGui.InputFloat($"{name}##{id}", ref val))
                    {
                        value = val;
                        return true;
                    }
                    break;

                case EditorPropertyMode.Slider:
                    if (ImGui.SliderFloat($"{name}##{id}", ref val, min, max))
                    {
                        value = val;
                        return true;
                    }
                    break;

                case EditorPropertyMode.SliderAngle:
                    if (ImGui.SliderAngle($"{name}##{id}", ref val, min, max))
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