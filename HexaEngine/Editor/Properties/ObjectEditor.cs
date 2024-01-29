namespace HexaEngine.Editor.Properties
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using Hexa.NET.ImGui;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Diagnostics.CodeAnalysis;

    public interface IObjectEditorElement
    {
        /// <summary>
        /// Gets the name associated with the editor element.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the property information associated with the editor element.
        /// </summary>
        PropertyInfo? Property { get; }

        /// <summary>
        /// Gets the method information associated with the editor element.
        /// </summary>
        MethodInfo? Method { get; }

        public bool IsVisible { get; }

        EditorPropertyCondition? Condition { get; set; }

        EditorPropertyConditionMode ConditionMode { get; set; }

        public bool UpdateVisibility(object instance);

        public void Draw(IGraphicsContext context, object instance);
    }

    public class PropertyEditorObjectEditorElement : IObjectEditorElement
    {
        private readonly IPropertyEditor propertyEditor;
        private bool conditionState;
        private bool isVisible;

        public PropertyEditorObjectEditorElement(IPropertyEditor propertyEditor)
        {
            this.propertyEditor = propertyEditor;
        }

        public string Name => propertyEditor.Name;

        public PropertyInfo? Property => propertyEditor.Property;

        public MethodInfo? Method { get; }

        public EditorPropertyCondition? Condition { get; set; }

        public EditorPropertyConditionMode ConditionMode { get; set; }

        public bool IsVisible => isVisible;

        public bool UpdateVisibility(object instance)
        {
            if (Condition != null)
            {
                conditionState = Condition(instance);

                isVisible = ConditionMode != EditorPropertyConditionMode.Visible || !conditionState;
                return isVisible;
            }
            else
            {
                return isVisible = true;
            }
        }

        public void Draw(IGraphicsContext context, object instance)
        {
            if (!isVisible)
            {
                return;
            }

            var value = propertyEditor.Property.GetValue(instance);
            var oldValue = value;

            if (ConditionMode == EditorPropertyConditionMode.None || ConditionMode == EditorPropertyConditionMode.Visible)
            {
                DrawEditor(context, instance, value, oldValue);
                return;
            }

            ImGui.BeginDisabled(!conditionState);
            DrawEditor(context, instance, value, oldValue);
            ImGui.EndDisabled();
        }

        private void DrawEditor(IGraphicsContext context, object instance, object? value, object? oldValue)
        {
            if (propertyEditor.Draw(context, instance, ref value))
            {
                History.Default.Do($"Set Value ({propertyEditor.Name})", (instance, propertyEditor.Property), oldValue, value, DoAction, UndoAction);
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
    }

    public class EditorButtonObjectEditorElement : IObjectEditorElement
    {
        private readonly ObjectEditorButton objectEditorButton;
        private bool isVisible;
        private bool conditionState;

        public EditorButtonObjectEditorElement(ObjectEditorButton objectEditorButton)
        {
            this.objectEditorButton = objectEditorButton;
        }

        public string Name => objectEditorButton.Name;

        public PropertyInfo? Property { get; }

        public MethodInfo? Method => objectEditorButton.Method;

        public EditorPropertyCondition? Condition { get; set; }

        public EditorPropertyConditionMode ConditionMode { get; set; }

        public bool IsVisible => isVisible;

        public bool UpdateVisibility(object instance)
        {
            if (Condition != null)
            {
                conditionState = Condition(instance);
                return isVisible = ConditionMode != EditorPropertyConditionMode.Visible || conditionState;
            }
            else
            {
                return isVisible = true;
            }
        }

        public void Draw(IGraphicsContext context, object instance)
        {
            if (!isVisible)
            {
                return;
            }

            if (Condition == null || ConditionMode == EditorPropertyConditionMode.None)
            {
                objectEditorButton.Draw(instance);
                return;
            }

            ImGui.BeginDisabled(!conditionState);
            objectEditorButton.Draw(instance);
            ImGui.EndDisabled();
        }
    }

    public class BaseTypeSorter
    {
        public List<PropertyInfo> propertyInfos = new();
    }

    /// <summary>
    /// Implementation of the <see cref="IObjectEditor"/> interface for managing object editing in a graphical context.
    /// </summary>
    public sealed class ObjectEditor : IObjectEditor
    {
        private readonly List<IObjectEditorElement> elements = new();
        private readonly List<EditorCategory> categories = new();
        private readonly Dictionary<string, EditorCategory> nameToCategory = new();

        private ImGuiName guiName;
        private readonly bool isHidden;
        private readonly Type type;
        private object? instance;

        private static PropertyInfo[] GetBasePropertiesFirst([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type)
        {
            var orderList = new List<Type>();
            var iteratingType = type;
            do
            {
                orderList.Insert(0, iteratingType);
                iteratingType = iteratingType.BaseType;
            } while (iteratingType != null);

            var props = type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance)
                .OrderBy(x => orderList.IndexOf(x.DeclaringType))
                .ToArray();

            return props;
        }

        private static MethodInfo[] GetBaseMethodsFirst([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type type)
        {
            var orderList = new List<Type>();
            var iteratingType = type;
            do
            {
                orderList.Insert(0, iteratingType);
                iteratingType = iteratingType.BaseType;
            } while (iteratingType != null);

            var props = type.GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance)
                .OrderBy(x => orderList.IndexOf(x.DeclaringType))
                .ToArray();

            return props;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectEditor"/> class.
        /// </summary>
        /// <param name="type">The type of the object being edited.</param>
        /// <param name="factories">A list of property editor factories used to create property editors.</param>
        public ObjectEditor([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods)] Type type, List<IPropertyEditorFactory> factories)
        {
            this.type = type;
            PropertyInfo[] properties = GetBasePropertiesFirst(type); //type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
            MethodInfo[] methods = GetBaseMethodsFirst(type); //type.GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);

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
                var conditionAttr = property.GetCustomAttribute<EditorPropertyConditionAttribute>();

                for (int i = 0; i < factories.Count; i++)
                {
                    if (factories[i].TryCreate(property, nameAttr, out var editor))
                    {
                        PropertyEditorObjectEditorElement element = new(editor)
                        {
                            Condition = conditionAttr?.Condition,
                            ConditionMode = conditionAttr?.Mode ?? EditorPropertyConditionMode.None
                        };

                        if (categoryAttr != null)
                        {
                            var category = CreateOrGetCategory(categoryAttr);
                            category.Elements.Add(element);
                        }
                        else
                        {
                            elements.Add(element);
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

                var categoryAttr = method.GetCustomAttribute<EditorCategoryAttribute>();
                var conditionAttr = method.GetCustomAttribute<EditorPropertyConditionAttribute>();

                EditorButtonObjectEditorElement element = new(new(buttonAttr, method))
                {
                    Condition = conditionAttr?.Condition,
                    ConditionMode = conditionAttr?.Mode ?? EditorPropertyConditionMode.None
                };

                if (categoryAttr != null)
                {
                    var category = CreateOrGetCategory(categoryAttr);
                    category.Elements.Add(element);
                }
                else
                {
                    elements.Add(element);
                    break;
                }
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
        public bool IsEmpty => elements.Count == 0 && categories.Count == 0;

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

            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].UpdateVisibility(instance);
            }

            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].Draw(context, instance);
            }

            object? nullObj = null;

            for (int i = 0; i < categories.Count; i++)
            {
                var category = categories[i];

                category.Draw(context, instance, ref nullObj);
            }

            if (!NoTable)
            {
                ImGui.EndTable();
            }
        }

        /// <summary>
        /// Disposes of the resources used by the object editor.
        /// </summary>
        public void Dispose()
        {
        }
    }
}