namespace HexaEngine.Editor.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class EditorCategoryAttribute : Attribute
    {
        public EditorCategoryAttribute(string name, string? parent = null)
        {
            Name = name;
            Parent = parent;
        }

        public string Name { get; set; }

        public string? Parent { get; set; }
    }
}