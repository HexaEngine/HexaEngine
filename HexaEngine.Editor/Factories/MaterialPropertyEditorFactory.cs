namespace HexaEngine.Editor.Factories
{
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Editors;
    using HexaEngine.Editor.Properties;
    using System.Reflection;

    public class MaterialPropertyEditorFactory : IPropertyEditorFactory
    {
        public bool CanCreate(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return property.CanWrite && property.CanRead && property.PropertyType == typeof(MaterialData);
        }

        public IPropertyEditor Create(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return new MaterialPropertyEditor(nameAttr.Name, property);
        }
    }
}