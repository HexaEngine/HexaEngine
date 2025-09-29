namespace HexaEngine.ShadingLang
{
    public static unsafe class HXSLAnalyzerRegistry
    {
        private static readonly List<HXSLAnalyzer> analyzers = [];

        static HXSLAnalyzerRegistry()
        {
            Register<HXSLPropertyAnalyzer>();
            Register<HXSLStructAnalyzer>();
            Register<HXSLDeclarationAnalyzer>();
        }

        public static bool TryParse(HXSLModule module, ref HXSLParser parser, ref TokenStream stream, HXSLCompilation* compilation)
        {
            foreach (var analyzer in analyzers)
            {
                if (analyzer.TryParse(module, ref parser, ref stream, compilation))
                {
                    return true;
                }
            }

            return false;
        }

        public static void Register<T>() where T : HXSLAnalyzer, new()
        {
            Register(new T());
        }

        public static void Register(HXSLAnalyzer analyzer)
        {
            analyzers.Add(analyzer);
        }
    }
}