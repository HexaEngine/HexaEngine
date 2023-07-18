namespace HexaEngine.Editor.Properties
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Properties.Editors;
    using HexaEngine.ImGuiNET;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class ObjectEditor : IObjectEditor
    {
        private readonly List<(PropertyInfo, IPropertyEditor)> editors = new();
        private readonly List<ObjectEditorButton> buttons = new();
        private readonly List<EditorCategory> categories = new();
        private readonly Dictionary<string, EditorCategory> nameToCategory = new();
        private ImGuiName guiName;
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
                guiName = new(type.Name);
            }
            else
            {
                guiName = new(componentNameAttr.Name);
                isHidden = componentNameAttr.IsHidden;
            }

            var nodeNameAttr = type.GetCustomAttribute<EditorGameObjectAttribute>();
            if (nodeNameAttr != null)
            {
                guiName = new(nodeNameAttr.Name);
            }

            foreach (var property in properties)
            {
                var nameAttr = property.GetCustomAttribute<EditorPropertyAttribute>();
                if (nameAttr == null)
                {
                    continue;
                }
                if (string.IsNullOrEmpty(nameAttr.Name))
                {
                    nameAttr.Name = property.Name;
                }

                var categoryAttr = property.GetCustomAttribute<EditorCategoryAttribute>();

                for (int i = 0; i < factories.Count; i++)
                {
                    if (factories[i].TryCreate(property, nameAttr, out var editor))
                    {
                        if (categoryAttr != null)
                        {
                            var category = CreateOrGetCategory(categoryAttr);
                            category.Properties.Add(new(property, editor));
                        }
                        else
                        {
                            editors.Add(new(property, editor));
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo? method = methods[i];
                var buttonAttr = method.GetCustomAttribute<EditorButtonAttribute>();
                if (buttonAttr == null)
                {
                    continue;
                }

                buttons.Add(new(buttonAttr, method));
            }

            Queue<EditorCategory> removeQueue = new();
            for (int i = 0; i < categories.Count; i++)
            {
                var category = categories[i];
                if (category.CategoryParent != null)
                {
                    var parent = CreateOrGetCategory(category.CategoryParent);
                    parent.ChildCategories.Add(category);
                    removeQueue.Enqueue(category);
                }
            }

            while (removeQueue.TryDequeue(out var category))
            {
                categories.Remove(category);
            }

            for (int i = 0; i < categories.Count; i++)
            {
                categories[i].Sort();
            }
        }

        private EditorCategory CreateOrGetCategory(EditorCategoryAttribute categoryAttr)
        {
            if (nameToCategory.TryGetValue(categoryAttr.Name, out var editor))
            {
                return editor;
            }

            editor = new(categoryAttr);
            nameToCategory.Add(categoryAttr.Name, editor);
            categories.Add(editor);
            return editor;
        }

        private EditorCategory CreateOrGetCategory(string category)
        {
            if (nameToCategory.TryGetValue(category, out var editor))
            {
                return editor;
            }

            editor = new(category);
            nameToCategory.Add(category, editor);
            categories.Add(editor);
            return editor;
        }

        public string Name => guiName.Name;

        public Type Type => type;

        public object? Instance { get => instance; set => instance = value; }

        public bool IsEmpty => editors.Count == 0 && buttons.Count == 0 && categories.Count == 0;

        public bool NoTable { get; set; }

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

            if (!NoTable)
            {
                ImGui.BeginTable(guiName.RawId, 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.PreciseWidths);
                ImGui.TableSetupColumn("", 0.0f);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);
            }

            for (int i = 0; i < editors.Count; i++)
            {
                var editor = editors[i];
                var value = editor.Item1.GetValue(instance);
                var oldValue = value;

                if (editor.Item2.Draw(context, instance, ref value))
                {
                    History.Default.Do((instance, editor.Item1), oldValue, value, DoAction, UndoAction);
                }
            }

            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Draw(instance);
            }

            for (int i = 0; i < categories.Count; i++)
            {
                var category = categories[i];
                category.Draw(context, instance, ref instance);
            }

            if (!NoTable)
            {
                ImGui.EndTable();
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