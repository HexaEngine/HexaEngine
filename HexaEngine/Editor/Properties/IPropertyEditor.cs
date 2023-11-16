namespace HexaEngine.Editor.Properties
{
    using HexaEngine.Core.Graphics;
    using System.Reflection;

    /// <summary>
    /// Represents an interface for a property editor that can draw and handle user interactions for a specific property.
    /// </summary>
    public interface IPropertyEditor
    {
        /// <summary>
        /// Gets the name associated with the property editor.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the property information associated with the property editor.
        /// </summary>
        PropertyInfo Property { get; }

        /// <summary>
        /// Draws the property editor within the specified graphics context, allowing the user to interact with and modify the property value.
        /// </summary>
        /// <param name="context">The graphics context used for drawing.</param>
        /// <param name="instance">The instance of the object containing the property.</param>
        /// <param name="value">The current value of the property. This may be modified by user interaction.</param>
        /// <returns><c>true</c> if the property value was modified; otherwise, <c>false</c>.</returns>
        bool Draw(IGraphicsContext context, object instance, ref object? value);
    }
}