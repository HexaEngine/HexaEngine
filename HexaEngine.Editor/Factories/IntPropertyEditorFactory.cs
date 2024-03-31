namespace HexaEngine.Editor.Factories
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Editors;
    using HexaEngine.Editor.Properties;
    using System.Reflection;

    public class IntPropertyEditorFactory : IPropertyEditorFactory
    {
        public bool CanCreate(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return property.CanWrite && property.CanRead && property.PropertyType == typeof(int);
        }

        public IPropertyEditor Create(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return new IntPropertyEditor(nameAttr.Name, property, nameAttr.Mode, nameAttr.Min, nameAttr.Max);
        }
    }

    public class UIntPropertyEditorFactory : IPropertyEditorFactory
    {
        public bool CanCreate(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return property.CanWrite && property.CanRead && property.PropertyType == typeof(uint);
        }

        public IPropertyEditor Create(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return new UIntPropertyEditor(nameAttr.Name, property, nameAttr.Mode, nameAttr.Min, nameAttr.Max);
        }
    }
}