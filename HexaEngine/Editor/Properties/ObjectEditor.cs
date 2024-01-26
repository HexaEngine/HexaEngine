namespace HexaEngine.Editor.Properties
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using Hexa.NET.ImGui;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Implementation of the <see cref="IObjectEditor"/> interface for managing object editing in a graphical context.
    /// </summary>
    public sealed class ObjectEditor : IObjectEditor
    {
        private readonly List<(PropertyInfo, IPropertyEditor)> editors = new();
        private readonly List<ObjectEditorButton> buttons = new();
        private readonly List<EditorCategory> categories = new();
        private readonly Dictionary<string, EditorCategory> nameToCategory = new();
        private ImGuiName guiName;
        private readonly bool isHidden;
        private readonly Type type;
        private object? instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectEditor"/> class.
        /// </summary>
        /// <param name="type">The type of the object being edited.</param>
        /// <param name="factories">A list of property editor factories used to create property editors.</param>
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

        /// <summary>
        /// Gets the name associated with the object editor.
        /// </summary>
        public string Name => guiName.Name;

        /// <summary>
        /// Gets the type of the object being edited.
        /// </summary>
        public Type Type => type;

        /// <summary>
        /// Gets or sets the instance of the object being edited.
        /// </summary>
        public object? Instance { get => instance; set => instance = value; }

        /// <summary>
        /// Gets a value indicating whether the object editor is empty.
        /// </summary>
        public bool IsEmpty => editors.Count == 0 && buttons.Count == 0 && categories.Count == 0;

        /// <summary>
        /// Gets a value indicating whether the object editor is hidden.
        /// </summary>
        public bool IsHidden => isHidden;

        /// <summary>
        /// Gets or sets a value indicating whether to skip the table setup when drawing the object editor.
        /// </summary>
        public bool NoTable { get; set; }

        /// <summary>
        /// Draws the object editor within the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context used for drawing.</param>
        public void Draw(IGraphicsContext context)
        {
            if (instance == null)
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
                    History.Default.Do($"Set Value ({editor.Item2.Name})", (instance, editor.Item1), oldValue, value, DoAction, UndoAction);
                }
            }

            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Draw(instance);
            }

            for (int i = 0; i < categories.Count; i++)
            {
                var category = categories[i];
#nullable disable // analyser being stupid again....
                category.Draw(context, instance, ref instance);
#nullable restore
            }

            if (!NoTable)
            {
                ImGui.EndTable();
            }
        }

        /// <summary>
        /// Action to perform when applying changes during a history action.
        /// </summary>
        /// <param name="context">The context containing information about the history action.</param>
        private static void DoAction(object context)
        {
            var ctx = (HistoryContext<(object, PropertyInfo), object>)context;
            ctx.Target.Item2.SetValue(ctx.Target.Item1, ctx.NewValue);
        }

        /// <summary>
        /// Action to perform when undoing changes during a history action.
        /// </summary>
        /// <param name="context">The context containing information about the history action.</param>
        private static void UndoAction(object context)
        {
            var ctx = (HistoryContext<(object, PropertyInfo), object>)context;
            ctx.Target.Item2.SetValue(ctx.Target.Item1, ctx.OldValue);
        }

        /// <summary>
        /// Disposes of the resources used by the object editor.
        /// </summary>
        public void Dispose()
        {
        }
    }
}