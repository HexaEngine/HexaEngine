namespace HexaEngine.Editor.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EditorWindowCategoryAttribute(string category) : Attribute
    {
        public string Category { get; } = category;
    }
}