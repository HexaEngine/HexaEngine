namespace HexaEngine.UI.Markup
{
    public class NamespaceDeclaration
    {
        public NamespaceDeclaration(string @namespace, string prefix)
        {
            Namespace = @namespace;
            Prefix = prefix;
        }

        public string Namespace { get; }

        public string Prefix { get; }
    }
}