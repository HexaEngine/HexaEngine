namespace HexaEngine.Editor.Properties
{
    using HexaEngine.Editor.Attributes;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    /// <summary>
    /// Represents an interface for a property editor factory that creates property editors based on property information and attributes.
    /// </summary>
    public interface IPropertyEditorFactory
    {
        /// <summary>
        /// Determines whether the factory can create a property editor for the specified property and attributes.
        /// </summary>
        /// <param name="property">The property information.</param>
        /// <param name="nameAttr">The editor property attribute.</param>
        /// <returns><c>true</c> if the factory can create a property editor; otherwise, <c>false</c>.</returns>
        bool CanCreate(PropertyInfo property, EditorPropertyAttribute nameAttr);

        /// <summary>
        /// Creates a property editor for the specified property and attributes.
        /// </summary>
        /// <param name="property">The property information.</param>
        /// <param name="nameAttr">The editor property attribute.</param>
        /// <returns>The created property editor.</returns>
        IPropertyEditor Create(PropertyInfo property, EditorPropertyAttribute nameAttr);

        /// <summary>
        /// Tries to create a property editor for the specified property and attributes.
        /// </summary>
        /// <param name="property">The property information.</param>
        /// <param name="nameAttr">The editor property attribute.</param>
        /// <param name="editor">When this method returns, contains the created property editor if successful; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if the factory successfully created a property editor; otherwise, <c>false</c>.</returns>
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