namespace HexaEngine.Editor.Attributes
{
    using System;

    /// <summary>
    /// Represents an attribute used to mark properties as editor categories.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public class EditorCategoryAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorCategoryAttribute"/> class with the specified category name and optional parent category.
        /// </summary>
        /// <param name="name">The name of the editor category.</param>
        /// <param name="parent">The name of the parent category. If not specified, the category is considered a top-level category.</param>
        public EditorCategoryAttribute(string name, string? parent = null)
        {
            Name = name;
            Parent = parent;
        }

        /// <summary>
        /// Gets or sets the name of the editor category.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the parent category. If not specified, the category is considered a top-level category.
        /// </summary>
        public string? Parent { get; set; }
    }
}