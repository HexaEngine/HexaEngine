namespace HexaEngine.Editor.Factories
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Editors;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Graphics;
    using System.Reflection;

    public class DrawLayerPropertyEditorFactory : IPropertyEditorFactory
    {
        public bool CanCreate(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return property.CanRead && property.PropertyType == typeof(DrawLayerCollection);
        }

        public IPropertyEditor Create(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return new DrawLayerPropertyEditor(nameAttr.Name, property);
        }
    }
}