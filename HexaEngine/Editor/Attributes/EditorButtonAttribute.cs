namespace HexaEngine.Editor.Attributes
{
    using System;

    /// <summary>
    /// Represents an attribute used to mark methods as editor buttons.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EditorButtonAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorButtonAttribute"/> class with the specified button name.
        /// </summary>
        /// <param name="name">The name of the editor button.</param>
        public EditorButtonAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the name of the editor button.
        /// </summary>
        public string Name { get; }
    }
}