namespace HexaEngine.Editor.Properties.Editors
{
    using HexaEngine.Core.Editor.Properties;
    using ImGuiNET;

    public class StringPropertyEditor : IPropertyEditor
    {
        private readonly string name;

        public StringPropertyEditor(string name)
        {
            this.name = name;
        }

        public bool Draw(object instance, ref object? value)
        {
            string? val = (string?)value;
            if (val == null) return false;
            if (ImGui.InputText(name, ref val, 2048))
            {
                value = val;
                return true;
            }
            return false;
        }
    }
}