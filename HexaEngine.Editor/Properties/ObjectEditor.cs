namespace HexaEngine.Editor.Properties
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Editor.Properties;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class ObjectEditor : IObjectEditor
    {
        private readonly List<(PropertyInfo, IPropertyEditor)> editors = new();
        private readonly string name;
        private readonly bool isHidden;
        private readonly Type type;
        private object? instance;

        public ObjectEditor(Type type, List<IPropertyEditorFactory> factories)
        {
            this.type = type;
            PropertyInfo[] properties = type.GetProperties();
            var componentNameAttr = type.GetCustomAttribute<EditorComponentAttribute>();
            if (componentNameAttr == null)
            {
                name = type.Name;
            }
            else
            {
                name = componentNameAttr.Name;
                isHidden = componentNameAttr.IsHidden;
            }

            var nodeNameAttr = type.GetCustomAttribute<EditorNodeAttribute>();
            if (nodeNameAttr != null)
            {
                name = nodeNameAttr.Name;
            }

            foreach (var property in properties)
            {
                var nameAttr = property.GetCustomAttribute<EditorPropertyAttribute>();
                if (nameAttr == null)
                {
                    continue;
                }

                for (int i = 0; i < factories.Count; i++)
                {
                    if (factories[i].TryCreate(property, nameAttr, out var editor))
                    {
                        editors.Add(new(property, editor));
                    }
                }
            }
        }

        public string Name => name;

        public Type Type => type;

        public object? Instance { get => instance; set => instance = value; }

        public void Draw()
        {
            if (instance == null) return;
            if (isHidden) return;
            for (int i = 0; i < editors.Count; i++)
            {
                var editor = editors[i];
                var value = editor.Item1.GetValue(instance);
                var oldValue = value;

                if (editor.Item2.Draw(instance, ref value))
                {
                    Designer.History.Do(
                        () => editor.Item1.SetValue(instance, value),
                        () => editor.Item1.SetValue(instance, oldValue));
                }
            }
        }
    }
}