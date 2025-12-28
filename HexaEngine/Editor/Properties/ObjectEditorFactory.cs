namespace HexaEngine.Editor.Properties
{
    using HexaEngine.Editor.Attributes;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    /// <summary>
    /// Factory class responsible for creating and managing object editors.
    /// </summary>
    public class ObjectEditorFactory
    {
        private static readonly Dictionary<Type, IObjectEditor> editors = new();
        private static readonly List<IPropertyEditorFactory> factories = new();
        private static readonly Lock _lock = new();

        /// <summary>
        /// Static constructor for initializing the ObjectEditorFactory.
        /// </summary>
        static ObjectEditorFactory()
        {
        }

        public static void Reset()
        {
            editors.Clear();
            factories.Clear();
        }

        /// <summary>
        /// Adds a custom property editor factory to the list of factories.
        /// </summary>
        /// <param name="factory">The property editor factory to be added.</param>
        public static void AddFactory(IPropertyEditorFactory factory)
        {
            factories.Add(factory);
        }

        /// <summary>
        /// Adds a new instance of a specified property editor factory to the list of factories.
        /// </summary>
        /// <typeparam name="T">The type of the property editor factory to be added.</typeparam>
        public static void AddFactory<T>() where T : IPropertyEditorFactory, new()
        {
            factories.Add(new T());
        }

        /// <summary>
        /// Removes a property editor factory from the list of factories.
        /// </summary>
        /// <param name="factory">The property editor factory to be removed.</param>
        public static void RemoveFactory(IPropertyEditorFactory factory)
        {
            factories.Remove(factory);
        }

        /// <summary>
        /// Registers a custom object editor for a specific type.
        /// </summary>
        /// <param name="target">The type for which the object editor is registered.</param>
        /// <param name="editor">The object editor to be registered.</param>
        public static void RegisterEditor(Type target, IObjectEditor editor)
        {
            editors.Add(target, editor);
        }

        /// <summary>
        /// Registers a custom object editor for a specific generic type.
        /// </summary>
        /// <typeparam name="T">The generic type for which the object editor is registered.</typeparam>
        /// <param name="editor">The object editor to be registered.</param>
        public static void RegisterEditor<T>(IObjectEditor editor)
        {
            RegisterEditor(typeof(T), editor);
        }

        /// <summary>
        /// Registers a custom object editor for a specific generic type.
        /// </summary>
        /// <typeparam name="T">The generic type for which the object editor is registered.</typeparam>
        /// <typeparam name="TEditor">The type of the object editor to be registered.</typeparam>
        public static void RegisterEditor<T, TEditor>() where TEditor : IObjectEditor, new()
        {
            RegisterEditor(typeof(T), new TEditor());
        }

        /// <summary>
        /// Destroys an object editor for a specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        public static void DestroyEditor(Type type)
        {
            lock (_lock)
            {
                if (editors.TryGetValue(type, out var editor))
                {
                    editors.Remove(type);
                    editor.Dispose();
                    editor.Instance = null;
                }
            }
        }

        /// <summary>
        /// Destroys an object editor for a specified type.
        /// </summary>
        /// <typeparam name="T">The generic type.</typeparam>
        public static void DestroyEditor<T>()
        {
            DestroyEditor(typeof(T));
        }

        /// <summary>
        /// Destroys an object editor.
        /// </summary>
        /// <param name="editor">The editor.</param>
        public static void DestroyEditor(IObjectEditor editor)
        {
            DestroyEditor(editor.Type);
        }

        /// <summary>
        /// Creates an object editor for a specified type.
        /// </summary>
        /// <param name="type">The type for which an object editor is created.</param>
        /// <returns>An instance of the object editor for the specified type.</returns>
        public static IObjectEditor CreateEditor([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods)] Type type)
        {
            if (!editors.TryGetValue(type, out IObjectEditor? editor))
            {
                editor = new ObjectEditor(type, factories);
                editors.Add(type, editor);
            }
            return editor;
        }

        /// <summary>
        /// Creates an object editor for a specified generic type.
        /// </summary>
        /// <typeparam name="T">The generic type for which an object editor is created.</typeparam>
        /// <returns>An instance of the object editor for the specified generic type.</returns>
        public static IObjectEditor CreateEditor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods)] T>()
        {
            return CreateEditor(typeof(T));
        }

        private static readonly EditorPropertyAttribute dummy = new(null!, EditorPropertyMode.Default);

        public static IPropertyEditor? CreatePropertyEditor(PropertyInfo property)
        {
            dummy.Name = null!;
            var nameAttr = property.GetCustomAttribute<EditorPropertyAttribute>() ?? dummy;

            if (string.IsNullOrEmpty(nameAttr.Name))
            {
                nameAttr.Name = property.Name;
            }

            for (int i = 0; i < factories.Count; i++)
            {
                if (factories[i].TryCreate(property, nameAttr, out var editor))
                {
                    return editor;
                }
            }

            return null;
        }

        public static IPropertyEditor? CreatePropertyEditor(PropertyInfo property, EditorPropertyAttribute nameAttr)
        {
            if (string.IsNullOrEmpty(nameAttr.Name))
            {
                nameAttr.Name = property.Name;
            }

            for (int i = 0; i < factories.Count; i++)
            {
                if (factories[i].TryCreate(property, nameAttr, out var editor))
                {
                    return editor;
                }
            }

            return null;
        }
    }
}