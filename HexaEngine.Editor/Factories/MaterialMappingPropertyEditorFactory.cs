namespace HexaEngine.Editor.Factories
{
    using HexaEngine.Components.Renderer;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Editors;
    using HexaEngine.Editor.Properties;
    using System.Reflection;

    public class MaterialMappingPropertyEditorFactory : IPropertyEditorFactory
    {
        public bool CanCreate(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return property.CanRead && property.PropertyType == typeof(MaterialMappingCollection);
        }

        public IPropertyEditor Create(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return new MaterialMappingPropertyEditor(nameAttr.Name, property);
        }
    }
}