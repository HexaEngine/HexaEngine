namespace HexaEngine.Editor.Properties.Factories
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Editor.Properties;
    using HexaEngine.Editor.Properties.Editors;
    using System.Reflection;

    public class SubTypePropertyFactory : IPropertyEditorFactory
    {
        public bool CanCreate(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return property.CanWrite && property.CanRead && property.PropertyType.IsClass;
        }

        public IPropertyEditor Create(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return new SubTypePropertyEditor(nameAttr, property);
        }
    }
}