using HexaEngine.ShadingLang.Text;

namespace HexaEngine.ShadingLang
{
    public unsafe struct HXSLSymbol
    {
        public TextSpan Span;
        public HXSLSymbolType Type;

        public HXSLSymbol(TextSpan span, HXSLSymbolType type)
        {
            Span = span;
            Type = type;
        }
    }

    public enum HXSLSymbolType
    {
        Unknown,
        Type,
        Function
    }
}