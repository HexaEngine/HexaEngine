namespace HexaEngine.Editor.Factories
{
    using HexaEngine.Core.Assets;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Editors;
    using HexaEngine.Editor.Properties;
    using System.Reflection;

    public class AssetRefPropertyEditorFactory : IPropertyEditorFactory
    {
        public bool CanCreate(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return property.CanWrite && property.CanRead && property.PropertyType == typeof(AssetRef);
        }

        public IPropertyEditor Create(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return new AssetRefPropertyEditor(nameAttr.Name, property, nameAttr.AssetType);
        }
    }
}