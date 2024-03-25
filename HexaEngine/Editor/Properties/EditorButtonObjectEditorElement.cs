namespace HexaEngine.Editor.Properties
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;
    using System.Reflection;

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

        public bool Draw(IGraphicsContext context, object instance)
        {
            if (!isVisible)
            {
                return false;
            }

            if (Condition == null || ConditionMode == EditorPropertyConditionMode.None)
            {
                return objectEditorButton.Draw(instance);
            }

            ImGui.BeginDisabled(!conditionState);
            var result = objectEditorButton.Draw(instance);
            ImGui.EndDisabled();
            return result;
        }
    }
}