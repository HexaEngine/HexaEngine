namespace HexaEngine.Editor.Attributes
{
    using System;

    public class EditorPropertyTypeSelectorAttribute<T> : EditorPropertyAttribute
    {
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