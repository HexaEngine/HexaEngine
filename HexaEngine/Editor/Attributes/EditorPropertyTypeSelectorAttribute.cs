namespace HexaEngine.Editor.Attributes
{
    using System;

    /// <summary>
    /// Specifies a type selector mode for an editor property with a specified base type.
    /// </summary>
    /// <typeparam name="T">The base type for the type selector.</typeparam>
    public class EditorPropertyTypeSelectorAttribute<T> : EditorPropertyAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorPropertyTypeSelectorAttribute{T}"/> class.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="types">The allowed types for the type selector.</param>
        public EditorPropertyTypeSelectorAttribute(string name, params Type[] types)
            : base(name,
                  typeof(T),
                  types,
                  types.Select(x => x.Name).ToArray(),
                  EditorPropertyMode.TypeSelector)
        {
        }
    }
}