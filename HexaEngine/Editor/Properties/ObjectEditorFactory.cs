namespace HexaEngine.Editor.Properties
{
    using HexaEngine.Editor.Properties.Factories;
    using System;
    using System.Collections.Generic;

    public class ObjectEditorFactory
    {
        private static readonly Dictionary<Type, IObjectEditor> editors = new();
        private static readonly List<IPropertyEditorFactory> factories = new();

        static ObjectEditorFactory()
        {
            factories.Add(new BoolPropertyEditorFactory());
            factories.Add(new EnumPropertyEditorFactory());
            factories.Add(new FloatPropertyEditorFactory());
            factories.Add(new StringPropertyEditorFactory());
            factories.Add(new TypePropertyFactory());
            factories.Add(new Vector2PropertyEditorFactory());
            factories.Add(new Vector3PropertyEditorFactory());
            factories.Add(new Vector4PropertyEditorFactory());
            factories.Add(new SubTypePropertyFactory());
        }

        public static void AddFactory(IPropertyEditorFactory factory)
        {
            factories.Add(factory);
        }

        public static void RemoveFactory(IPropertyEditorFactory factory)
        {
            factories.Remove(factory);
        }

        public static void RegisterEditor(Type target, IObjectEditor editor)
        {
            editors.Add(target, editor);
        }

        public static void RegisterEditor<T>(IObjectEditor editor)
        {
            RegisterEditor(typeof(T), editor);
        }

        public static IObjectEditor CreateEditor(Type type)
        {
            if (!editors.TryGetValue(type, out IObjectEditor? editor))
            {
                editor = new ObjectEditor(type, factories);
                editors.Add(type, editor);
            }
            return editor;
        }

        public static IObjectEditor CreateEditor<T>()
        {
            return CreateEditor(typeof(T));
        }
    }
}