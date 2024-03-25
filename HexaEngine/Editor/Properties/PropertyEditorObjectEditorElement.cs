namespace HexaEngine.Editor.Properties
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;
    using System.Reflection;

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
                isVisible = ConditionMode != EditorPropertyConditionMode.Visible || conditionState;
                return isVisible;
            }
            else
            {
                return isVisible = true;
            }
        }

        public bool Draw(IGraphicsContext context, object instance)
        {
            if (!isVisible)
            {
                return false;
            }

            var value = propertyEditor.Property.GetValue(instance);
            var oldValue = value;

            if (ConditionMode == EditorPropertyConditionMode.None || ConditionMode == EditorPropertyConditionMode.Visible)
            {
                return DrawEditor(context, instance, value, oldValue);
            }

            ImGui.BeginDisabled(!conditionState);
            var result = DrawEditor(context, instance, value, oldValue);
            ImGui.EndDisabled();
            return result;
        }

        private bool DrawEditor(IGraphicsContext context, object instance, object? value, object? oldValue)
        {
            if (propertyEditor.Draw(context, instance, ref value))
            {
                History.Default.Do($"Set Value ({propertyEditor.Name})", (instance, propertyEditor.Property), oldValue, value, DoAction, UndoAction);
                return true;
            }
            return false;
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
}