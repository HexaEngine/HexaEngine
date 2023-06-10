namespace HexaEngine.Editor.Properties
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Editor.Properties;
    using HexaEngine.Core.Graphics;
    using ImGuiNET;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class ObjectEditorButton
    {
        private readonly string name;
        private readonly MethodInfo info;

        public ObjectEditorButton(string name, MethodInfo info)
        {
            this.name = name;
            this.info = info;
        }

        public void Draw(object instance)
        {
            if (ImGui.Button(name))
            {
                info.Invoke(instance, null);
            }
        }
    }

    public class ObjectEditor : IObjectEditor
    {
        private readonly List<(PropertyInfo, IPropertyEditor)> editors = new();
        private readonly List<ObjectEditorButton> buttons = new();
        private readonly string name;
        private readonly bool isHidden;
        private readonly Type type;
        private object? instance;

        public ObjectEditor(Type type, List<IPropertyEditorFactory> factories)
        {
            this.type = type;
            PropertyInfo[] properties = type.GetProperties();
            MethodInfo[] methods = type.GetMethods();
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

            foreach (var method in methods)
            {
                var buttonAttr = method.GetCustomAttribute<EditorButtonAttribute>();
                if (buttonAttr == null)
                {
                    continue;
                }

                buttons.Add(new(buttonAttr.Name, method));
            }
        }

        public string Name => name;

        public Type Type => type;

        public object? Instance { get => instance; set => instance = value; }
        public bool IsEmpty => editors.Count == 0 && buttons.Count == 0;

        public void Draw(IGraphicsContext context)
        {
            if (instance == null)
            {
                return;
            }

            if (isHidden)
            {
                return;
            }

            for (int i = 0; i < editors.Count; i++)
            {
                var editor = editors[i];
                var value = editor.Item1.GetValue(instance);
                var oldValue = value;

                if (editor.Item2.Draw(instance, ref value))
                {
                    Designer.History.Do((instance, editor.Item1), oldValue, value, DoAction, UndoAction);
                }
            }
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Draw(instance);
            }
        }

        private static void DoAction(object context)
        {
            var ctx = (HistoryContext<(object, PropertyInfo), object>)context;
            ctx.Target.Item2.SetValue(ctx.Target.Item1, ctx.NewValue);
        }

        private static void UndoAction(object context)
        {
            var ctx = (HistoryContext<(object, PropertyInfo), object>)context;
            ctx.Target.Item2.SetValue(ctx.Target.Item1, ctx.OldValue);
        }

        public void Dispose()
        {
        }
    }
}