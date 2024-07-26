namespace HexaEngine.Editor.TextEditor
{
    using HexaEngine.Editor.TextEditor.Highlight.CSharp;

    public static class SyntaxHighlightDefaults
    {
        static SyntaxHighlightDefaults()
        {
            CSharp = new CSharpSyntaxHighlight();
        }

        public static SyntaxHighlight CSharp { get; }
    }
}