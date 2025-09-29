using Hexa.NET.Utilities;
using HexaEngine.ShadingLang.Text;

namespace HexaEngine.ShadingLang
{
    public unsafe struct HXSLField : IFreeable
    {
        public TextSpan Span;
        public HXSLType* Type;
        public TextSpan Name;
        public HXSLFieldFlags Flags;
        public TextSpan Semantic;

        public void Release()
        {
            Type = null;
        }
    }
}