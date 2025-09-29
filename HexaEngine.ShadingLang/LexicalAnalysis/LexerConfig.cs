namespace HexaEngine.ShadingLang.LexicalAnalysis
{
    using HexaEngine.ShadingLang.Collections;
    using HexaEngine.ShadingLang.LexicalAnalysis.HXSL;
    using System.ComponentModel;
    using System.Reflection;

    public class LexerConfig
    {
        public HashSet<char> Delimiters { get; set; } = [];

        public TernarySearchTreeDictionary<int> Keywords { get; set; } = new();

        public TernarySearchTreeDictionary<int> Operators { get; set; } = new();

        public bool SpecialParseTreatIdentiferAsLiteral { get; set; }

        public bool EnableCodeblock { get; set; }

        public bool EnableNewline { get; set; }

        public bool EnableWhitespace { get; set; }

        static LexerConfig()
        {
            HSLConfig = new() // json like markup lang
            {
                Delimiters = ['{', '}', '[', ']', ',', ':'],
                SpecialParseTreatIdentiferAsLiteral = true, // treats "identifier: identifier," as "identifier: literal,"
                EnableCodeblock = true, // <! code... !>
            };

            HLSLConfig = new() // this is not pure HLSL but a modified version.
            {
                Delimiters = ['{', '}', '[', ']', '(', ')', ',', ':', '.', ';', '#', '@'],
                SpecialParseTreatIdentiferAsLiteral = false,
                EnableCodeblock = false,
            };

            Type keywordType = typeof(HXSLKeyword);
            foreach (var item in Enum.GetValues<HXSLKeyword>())
            {
                var fieldInfo = keywordType.GetField(item.ToString());
                var originalName = fieldInfo?.GetCustomAttribute<DescriptionAttribute>()?.Description;
                if (originalName == null) continue;
                HLSLConfig.Keywords.Insert(originalName, (int)item);
            }

            Type operatorType = typeof(HXSLOperator);
            foreach (var item in Enum.GetValues<HXSLOperator>())
            {
                var fieldInfo = operatorType.GetField(item.ToString());
                var originalName = fieldInfo?.GetCustomAttribute<DescriptionAttribute>()?.Description;
                if (originalName == null) continue;
                HLSLConfig.Operators.Insert(originalName, (int)item);
            }
        }

        public static LexerConfig HSLConfig { get; }

        public static LexerConfig HLSLConfig { get; }
    }
}