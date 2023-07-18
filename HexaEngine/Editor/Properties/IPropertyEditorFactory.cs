namespace HexaEngine.Editor.Properties
{
    using HexaEngine.Editor.Attributes;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    public interface IPropertyEditorFactory
    {
        bool CanCreate(PropertyInfo property, EditorPropertyAttribute nameAttr);

        IPropertyEditor Create(PropertyInfo property, EditorPropertyAttribute nameAttr);

        bool TryCreate(PropertyInfo property, EditorPropertyAttribute nameAttr, [NotNullWhen(true)] out IPropertyEditor? editor)
        {
            if (CanCreate(property, nameAttr))
            {
                editor = Create(property, nameAttr) ?? throw new InvalidOperationException();
                return true;
            }
            editor = null;
            return false;
        }
    }
}