namespace HexaEngine.Editor.Properties.Editors
{
    using HexaEngine.Core.Editor.Properties;
    using ImGuiNET;

    public class BoolPropertyEditor : IPropertyEditor
    {
        private readonly string id;
        private readonly string name;

        public BoolPropertyEditor(string name)
        {
            this.id = Guid.NewGuid().ToString();
            this.name = name;
        }

        public bool Draw(object instance, ref object? value)
        {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            bool val = (bool)value;
#pragma warning restore CS8605 // Unboxing a possibly null value.
            if (ImGui.Checkbox($"{name}##{id}", ref val))
            {
                value = val;
                return true;
            }
            return false;
        }
    }
}