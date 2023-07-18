using HexaEngine.Editor.Properties.Editors;

namespace HexaEngine.Editor.Properties.Factories
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Properties;
    using System.Reflection;

    public class BoolPropertyEditorFactory : IPropertyEditorFactory
    {
        public bool CanCreate(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return property.CanWrite && property.CanRead && property.PropertyType == typeof(bool);
        }

        public IPropertyEditor Create(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return new BoolPropertyEditor(nameAttr.Name, property);
        }
    }
}