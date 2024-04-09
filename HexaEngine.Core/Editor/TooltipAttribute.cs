namespace HexaEngine.Editor.Attributes
{
    using System;

    /// <summary>
    /// Represents an attribute that provides tooltip text.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Method)]
    public class TooltipAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TooltipAttribute"/> class with the specified text.
        /// </summary>
        /// <param name="text">The tooltip text.</param>
        public TooltipAttribute(string text)
        {
            Text = text;
        }

        /// <summary>
        /// Gets the tooltip text.
        /// </summary>
        public string Text { get; }
    }
}