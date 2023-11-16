namespace HexaEngine.Editor.Attributes
{
    /// <summary>
    /// Specifies the category of an editor window.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EditorWindowCategoryAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorWindowCategoryAttribute"/> class.
        /// </summary>
        /// <param name="category">The category of the editor window.</param>
        public EditorWindowCategoryAttribute(string category)
        {
            Category = category;
        }

        /// <summary>
        /// Gets the category of the editor window.
        /// </summary>
        public string Category { get; }
    }
}