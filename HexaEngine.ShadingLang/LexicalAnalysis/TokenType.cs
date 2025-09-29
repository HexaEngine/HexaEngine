namespace HexaEngine.ShadingLang.LexicalAnalysis
{
    public enum TokenType
    {
        Unknown,
        Keyword,
        Identifier,
        Literal,
        Numeric,
        Codeblock,
        Delimiter,
        Operator,
        Comment,
        NewLine,
        Whitespace
    }
}