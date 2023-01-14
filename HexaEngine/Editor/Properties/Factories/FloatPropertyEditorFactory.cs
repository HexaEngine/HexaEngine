using HexaEngine.Editor.Properties.Editors;

namespace HexaEngine.Editor.Properties.Factories
{
    using HexaEngine.Editor.Attributes;
    using System.Reflection;

    public class FloatPropertyEditorFactory : IPropertyEditorFactory
    {
        public bool CanCreate(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return property.CanWrite && property.CanRead && property.PropertyType == typeof(float);
        }

        public IPropertyEditor Create(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return new FloatPropertyEditor(nameAttr.Name, nameAttr.Mode, (float)(nameAttr.Min ?? float.MinValue), (float)(nameAttr.Max ?? float.MaxValue));
        }
    }
}