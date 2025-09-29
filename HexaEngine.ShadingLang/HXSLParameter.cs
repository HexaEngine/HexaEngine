using Hexa.NET.Utilities;
using HexaEngine.ShadingLang.Text;

namespace HexaEngine.ShadingLang
{
    public unsafe struct HXSLParameter : IFreeable
    {
        public TextSpan Span;
        public HXSLParameterFlags Flags;
        public HXSLType* Type;
        public TextSpan Name;
        public TextSpan Semantic;

        public void Release()
        {
            if (Type != null)
            {
                Free(Type);
                Type = null;
            }
        }
    }
}