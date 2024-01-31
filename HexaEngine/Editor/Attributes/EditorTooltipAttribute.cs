namespace HexaEngine.Editor.Attributes
{
    using System;

    /// <summary>
    /// Represents an attribute used to add tooltips to the UI.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class EditorTooltipAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorButtonAttribute"/> class with the specified button name.
        /// </summary>
        /// <param name="tooltip">The text of the editor tooltip.</param>
        public EditorTooltipAttribute(string tooltip)
        {
            Tooltip = tooltip;
        }

        /// <summary>
        /// Gets the text of the editor tooltip.
        /// </summary>
        public string Tooltip { get; }
    }
}