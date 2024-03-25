namespace HexaEngine.Editor.Properties
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;
    using System.Reflection;

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

        public bool Draw(IGraphicsContext context, object instance);
    }
}