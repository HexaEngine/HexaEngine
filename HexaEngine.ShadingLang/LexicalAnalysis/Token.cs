using HexaEngine.ShadingLang.Text;

namespace HexaEngine.ShadingLang.LexicalAnalysis
{
    public struct Token
    {
        public TextSpan Span;
        public TokenType Type;
        public int Value;

        public Token(TextSpan span, TokenType type)
        {
            Span = span;
            Type = type;
        }

        public Token(TextSpan span, TokenType type, int value)
        {
            Span = span;
            Type = type;
            Value = value;
        }

        public override readonly string ToString()
        {
            return $"Type: {Type}, Span: \"{Span}\"";
        }
    }
}