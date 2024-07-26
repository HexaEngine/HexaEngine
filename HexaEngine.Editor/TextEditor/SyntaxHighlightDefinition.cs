namespace HexaEngine.Editor.TextEditor
{
    using HexaEngine.Editor.Themes;

    public class SyntaxHighlightDefinition(string name, string pattern, ColorRGBA color)
    {
        public string Name { get; set; } = name;

        public string Pattern { get; set; } = pattern;

        public ColorRGBA Color { get; set; } = color;
    }
}