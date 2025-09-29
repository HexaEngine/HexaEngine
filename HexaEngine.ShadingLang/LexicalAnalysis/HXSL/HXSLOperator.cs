namespace HexaEngine.ShadingLang.LexicalAnalysis.HXSL
{
    using System.ComponentModel;

    public enum HXSLOperator : int
    {
        [Description("+")]
        Add,

        [Description("-")]
        Subtract,

        [Description("*")]
        Multiply,

        [Description("/")]
        Divide,

        [Description("%")]
        Modulus,

        [Description("=")]
        Assign,

        [Description("+=")]
        PlusAssign,

        [Description("-=")]
        MinusAssign,

        [Description("*=")]
        MultiplyAssign,

        [Description("/=")]
        DivideAssign,

        [Description("%=")]
        ModulusAssign,

        [Description("~")]
        BitwiseNot,

        [Description("<<")]
        BitwiseShiftLeft,

        [Description(">>")]
        BitwiseShiftRight,

        [Description("&")]
        BitwiseAnd,

        [Description("|")]
        BitwiseOr,

        [Description("^")]
        BitwiseXor,

        [Description("<<=")]
        BitwiseShiftLeftAssign,

        [Description(">>=")]
        BitwiseShiftRightAssign,

        [Description("&=")]
        BitwiseAndAssign,

        [Description("|=")]
        BitwiseOrAssign,

        [Description("^=")]
        BitwiseXorAssign,

        [Description("&&")]
        AndAnd,

        [Description("||")]
        OrOr,

        [Description("<")]
        LessThan,

        [Description(">")]
        GreaterThan,

        [Description("==")]
        Equal,

        [Description("!=")]
        NotEqual,

        [Description("<=")]
        LessThanOrEqual,

        [Description(">=")]
        GreaterThanOrEqual,

        [Description("++")]
        Increment,

        [Description("--")]
        Decrement,

        [Description("!")]
        LogicalNot,

        [Description("-")]
        UnaryMinus,

        [Description("+")]
        UnaryPlus,
    }
}