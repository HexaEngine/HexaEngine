namespace HexaEngine.Editor.Properties.Editors
{
    using HexaEngine.Core.Editor.Properties;
    using HexaEngine.Scripting;
    using ImGuiNET;

    public class EnumPropertyEditor : IPropertyEditor
    {
        private readonly string name;
        private readonly Type propType;

        public EnumPropertyEditor(string name, Type propType)
        {
            this.name = name;
            this.propType = propType;
        }

        public bool Draw(object instance, ref object? value)
        {
            var enums = AssemblyManager.GetEnumValues(propType);
            var names = AssemblyManager.GetEnumNames(propType);
            int index = Array.IndexOf(enums, value);
            if (ImGui.Combo(name, ref index, names, names.Length))
            {
                value = enums.GetValue(index);
                return true;
            }
            return false;
        }
    }
}