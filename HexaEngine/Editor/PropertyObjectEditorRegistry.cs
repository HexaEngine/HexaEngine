namespace HexaEngine.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public static class PropertyObjectEditorRegistry
    {
        private static readonly List<IPropertyObjectEditor> editors = new();
        private static readonly Dictionary<Type, IPropertyObjectEditor> lookupTable = new();
        private static readonly Lock _lock = new();

        public static void Reset()
        {
            lock (_lock)
            {
                editors.Clear();
                lookupTable.Clear();
            }
        }

        public static void RegisterEditor(IPropertyObjectEditor editor)
        {
            lock (_lock)
            {
                var type = editor.Type;
                if (!lookupTable.TryAdd(type, editor))
                {
                    editors.Remove(editor);
                    lookupTable[type] = editor;
                }

                editors.Add(editor);
            }
        }

        public static void RegisterEditor<T>() where T : IPropertyObjectEditor, new()
        {
            RegisterEditor(new T());
        }

        public static void UnregisterEditor(IPropertyObjectEditor editor)
        {
            lock (_lock)
            {
                var type = editor.Type;
                editors.Remove(editor);
            }
        }

        public static void UnregisterEditor<T>()
        {
            lock (_lock)
            {
                var index = editors.FindIndex(x => x is T);
                if (index == -1)
                {
                    return;
                }
                var editor = editors[index];
                editors.RemoveAt(index);
            }
        }

        public static IPropertyObjectEditor? GetEditor(Type type)
        {
            lock (_lock)
            {
                for (int i = 0; i < editors.Count; i++)
                {
                    var editor = editors[i];
                    if (editor.Type.IsAssignableFrom(type))
                    {
                        return editor;
                    }
                }
            }
            return null;
        }

        public static bool TryGetEditor(Type type, [NotNullWhen(true)] out IPropertyObjectEditor? editor)
        {
            lock (_lock)
            {
                for (int i = 0; i < editors.Count; i++)
                {
                    var entry = editors[i];
                    if (entry.Type.IsAssignableFrom(type))
                    {
                        editor = entry;
                        return true;
                    }
                }

                editor = null;
                return false;
            }
        }
    }
}