using HexaEngine.Editor.Properties.Editors;

namespace HexaEngine.Editor.Properties.Factories
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Properties;
    using System.Numerics;
    using System.Reflection;

    public class Vector4PropertyEditorFactory : IPropertyEditorFactory
    {
        public bool CanCreate(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return property.CanWrite && property.CanRead && property.PropertyType == typeof(Vector4);
        }

        public IPropertyEditor Create(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return new Vector4PropertyEditor(nameAttr.Name, property, nameAttr.Mode, (float)(nameAttr.Min ?? float.MinValue), (float)(nameAttr.Max ?? float.MaxValue));
        }
    }
}