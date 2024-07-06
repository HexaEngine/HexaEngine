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

        public bool Draw(IGraphicsContext context, object instance, ProxyBase proxy, bool setOnInstance)
        {
            if (!isVisible)
            {
                return false;
            }

            var value = propertyEditor.Property.GetValue(instance);
            var oldValue = value;

            if (ConditionMode == EditorPropertyConditionMode.None || ConditionMode == EditorPropertyConditionMode.Visible)
            {
                return DrawEditor(context, instance, value, oldValue, proxy, setOnInstance);
            }

            ImGui.BeginDisabled(!conditionState);
            var result = DrawEditor(context, instance, value, oldValue, proxy, setOnInstance);
            ImGui.EndDisabled();
            return result;
        }

        private bool DrawEditor(IGraphicsContext context, object instance, object? value, object? oldValue, ProxyBase proxy, bool setOnInstance)
        {
            if (propertyEditor.Draw(context, instance, ref value))
            {
                History.Default.Do($"Set Value ({propertyEditor.Name})", (instance, propertyEditor.Property, proxy, setOnInstance), oldValue, value, DoActionProxy, UndoActionProxy);
                return true;
            }
            return false;
        }

        private static void DoAction(object context)
        {
            var ctx = (HistoryContext<(object Instance, PropertyInfo Property), object>)context;
            ctx.Target.Property.SetValue(ctx.Target.Instance, ctx.NewValue);
        }

        private static void UndoAction(object context)
        {
            var ctx = (HistoryContext<(object Instance, PropertyInfo Property), object>)context;
            ctx.Target.Property.SetValue(ctx.Target.Instance, ctx.OldValue);
        }

        private static void DoActionProxy(object context)
        {
            var ctx = (HistoryContext<(object Instance, PropertyInfo Property, ProxyBase Proxy, bool SetOnInstance), object>)context;
            ctx.Target.Proxy.Data[ctx.Target.Property.Name] = ctx.NewValue;
            if (ctx.Target.SetOnInstance)
            {
                ctx.Target.Property.SetValue(ctx.Target.Instance, ctx.NewValue);
            }
        }

        private static void UndoActionProxy(object context)
        {
            var ctx = (HistoryContext<(object Instance, PropertyInfo Property, ProxyBase Proxy, bool SetOnInstance), object>)context;
            ctx.Target.Proxy.Data[ctx.Target.Property.Name] = ctx.OldValue;
            if (ctx.Target.SetOnInstance)
            {
                ctx.Target.Property.SetValue(ctx.Target.Instance, ctx.OldValue);
            }
        }
    }
}