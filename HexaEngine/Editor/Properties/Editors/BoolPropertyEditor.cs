namespace HexaEngine.Editor.Properties.Editors
{
    using ImGuiNET;
    using System;

    public class BoolPropertyEditor : IPropertyEditor
    {
        private readonly string name;

        public BoolPropertyEditor(string name)
        {
            this.name = name;
        }

        public bool Draw(object instance, ref object? value)
        {
            bool val = (bool)value;
            if (ImGui.Checkbox(name, ref val))
            {
                value = val;
                return true;
            }
            return false;
        }
    }
}