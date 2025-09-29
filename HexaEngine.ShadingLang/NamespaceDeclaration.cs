using HexaEngine.ShadingLang.Text;

namespace HexaEngine.ShadingLang
{
    public unsafe struct NamespaceDeclaration
    {
        public TextSpan Span;
        public TextSpan Target;

        public NamespaceDeclaration(TextSpan span, TextSpan target)
        {
            Span = span;
            Target = target;
        }

        public static NamespaceDeclaration FromStream(ref HXSLParser parser, out bool scoped)
        {
            ref var stream = ref parser.Stream;
            var nsKeywordSpan = stream.LastToken.Span;
            NamespaceDeclaration ns = new(default, ParseQualifiedName(ref stream, out _));
            if (parser.TryEnterScope(ns.Target, ScopeType.Namespace, null))
            {
                scoped = true;
            }
            else
            {
                scoped = false;
                stream.ExpectDelimiter(';');
            }

            ns.Span = nsKeywordSpan.Merge(stream.LastToken.Span);
            return ns;
        }

        public static TextSpan ParseQualifiedName(ref TokenStream stream, out bool hasDot)
        {
            stream.ExpectIdentifier(out var identifier);
            hasDot = false;
            while (stream.TryGetDelimiter('.'))
            {
                hasDot = true;
                stream.ExpectIdentifier(out var secondary);
                identifier = identifier.Merge(secondary);
            }

            return identifier;
        }
    }
}