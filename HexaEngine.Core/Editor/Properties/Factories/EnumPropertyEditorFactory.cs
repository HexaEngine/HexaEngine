﻿using HexaEngine.Editor.Properties.Editors;

namespace HexaEngine.Editor.Properties.Factories
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Editor.Properties;
    using System.Reflection;

    public class EnumPropertyEditorFactory : IPropertyEditorFactory
    {
        public bool CanCreate(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return property.CanWrite && property.CanRead && property.PropertyType.IsEnum && nameAttr.Mode == EditorPropertyMode.Enum;
        }

        public IPropertyEditor Create(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            return new EnumPropertyEditor(nameAttr.Name, property);
        }
    }
}