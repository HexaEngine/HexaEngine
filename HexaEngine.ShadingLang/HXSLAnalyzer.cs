using System.Text;

namespace HexaEngine.ShadingLang
{
    public abstract unsafe class HXSLAnalyzer
    {
        public abstract bool TryParse(HXSLModule module, ref HXSLParser parser, ref TokenStream stream, HXSLCompilation* compilation);

        //public abstract void Compile(HSLModule module, StringBuilder sb, ref int offset, HSLCompilation* compilation);
    }
}