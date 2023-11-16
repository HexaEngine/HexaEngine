namespace HexaEngine.Editor.Properties.Editors
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Scripts;
    using Hexa.NET.ImGui;
    using System;
    using System.Reflection;

    public class TypePropertyEditor : IPropertyEditor
    {
        private ImGuiName guiName;
        private readonly Type targetType;

        public TypePropertyEditor(EditorPropertyAttribute attribute, PropertyInfo property)
        {
            Name = attribute.Name;
            Property = property;
            guiName = new(attribute.Name);
            targetType = attribute.TargetType ?? throw new InvalidOperationException();
        }

        public Type TargetType => targetType;

        public string Name { get; }
        public PropertyInfo Property { get; }

        public bool Draw(IGraphicsContext context, object instance, ref object? value)
        {
            var types = AssemblyManager.GetAssignableTypes(targetType);
            var names = AssemblyManager.GetAssignableTypeNames(targetType);

            int index;
            if (value == null)
            {
                index = -1;
            }
            else
            {
                index = types.IndexOf((Type)value);
            }

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(guiName.Name);
            ImGui.TableSetColumnIndex(1);
            if (ImGui.Combo(guiName.Id, ref index, names, names.Length))
            {
                value = types[index];
                return true;
            }
            return false;
        }
    }
}