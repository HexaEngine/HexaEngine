namespace HexaEngine.Editor.Properties.Editors
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Scripts;
    using Hexa.NET.ImGui;
    using System.Reflection;

    public class EnumPropertyEditor : IPropertyEditor
    {
        private readonly ImGuiName guiName;
        private readonly Type propType;

        public EnumPropertyEditor(string name, PropertyInfo property)
        {
            Name = name;
            Property = property;
            guiName = new(name);
            propType = property.PropertyType;
        }

        public string Name { get; }

        public PropertyInfo Property { get; }

        public bool Draw(IGraphicsContext context, object instance, ref object? value)
        {
            var enums = AssemblyManager.GetEnumValues(propType);
            var names = AssemblyManager.GetEnumNames(propType);
            int index = Array.IndexOf(enums, value);
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(guiName.Name);
            ImGui.TableSetColumnIndex(1);
            ImGui.PushItemWidth(0);
            if (ImGui.Combo(guiName.Id, ref index, names, names.Length))
            {
                value = enums.GetValue(index);
                ImGui.PopItemWidth();
                return true;
            }
            ImGui.PopItemWidth();
            return false;
        }
    }
}