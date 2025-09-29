using HexaEngine.ShadingLang.LexicalAnalysis.HXSL;
using HexaEngine.ShadingLang.Text;

namespace HexaEngine.ShadingLang
{
    public struct UsingDeclaration
    {
        public TextSpan Span;
        public TextSpan Target;
        public TextSpan Alias;
        public bool IsAlias;

        public static UsingDeclaration FromStream(ref TokenStream stream)
        {
            var nsKeywordSpan = stream.LastToken.Span;
            UsingDeclaration us = new();
            TextSpan identifier = NamespaceDeclaration.ParseQualifiedName(ref stream, out bool hasDot);
            if (hasDot)
            {
                us.Target = identifier;
            }
            else if (stream.TryGetOperator(HXSLOperator.Equal))
            {
                us.Alias = identifier;
                us.Target = NamespaceDeclaration.ParseQualifiedName(ref stream, out _);
                us.IsAlias = true;
            }
            else
            {
                us.Target = identifier;
            }
            stream.ExpectDelimiter(';');
            us.Span = nsKeywordSpan.Merge(stream.LastToken.Span);
            return us;
        }
    }

}