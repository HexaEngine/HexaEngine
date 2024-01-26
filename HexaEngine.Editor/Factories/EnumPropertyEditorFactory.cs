namespace HexaEngine.Editor.Factories
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Editors;
    using HexaEngine.Editor.Properties;
    using System.Reflection;

    public class EnumPropertyEditorFactory : IPropertyEditorFactory
    {
        public bool CanCreate(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return property.CanWrite && property.CanRead && property.PropertyType.IsEnum && nameAttr.Mode == EditorPropertyMode.Enum;
        }

        public IPropertyEditor Create(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return new EnumPropertyEditor(nameAttr, property);
        }
    }
}