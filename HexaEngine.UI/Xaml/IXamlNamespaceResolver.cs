namespace HexaEngine.UI.Xaml
{
    using HexaEngine.UI.Markup;

    public interface IXamlNamespaceResolver
    {
        public string GetNamespace(string prefix);

        public IEnumerable<NamespaceDeclaration> GetNamespacePrefixes();
    }
}