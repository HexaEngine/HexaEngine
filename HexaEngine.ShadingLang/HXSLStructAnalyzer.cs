using HexaEngine.ShadingLang.LexicalAnalysis.HXSL;

namespace HexaEngine.ShadingLang
{
    public enum HXSLAccessModifier
    {
        Private,
        Internal,
        Public,
    }

    public class HXSLStructAnalyzer : HXSLAnalyzer
    {
        public override unsafe bool TryParse(HXSLModule module, ref HXSLParser parser, ref TokenStream stream, HXSLCompilation* compilation)
        {
            stream.PushState();
            var access = parser.ParseAccessModifier();

            if (!stream.TryGetKeyword(HXSLKeyword.Struct))
            {
                stream.PopState();
                return false;
            }

            var name = stream.ExpectIdentifier();

            HXSLStructType structType = new();
            structType.Base.Name = name.ToStdWString();
            structType.AccessModifier = access;

            parser.EnterScope(name, ScopeType.Struct, &structType);
            while (parser.IterateScope())
            {
                while (HXSLAnalyzerRegistry.TryParse(module, ref parser, ref stream, compilation)) ;
            }

            parser.CurrentNamespace->Structs.Add(AllocT(structType));

            return true;
        }
    }
}