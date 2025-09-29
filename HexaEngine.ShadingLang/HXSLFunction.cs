using Hexa.NET.Utilities;
using HexaEngine.ShadingLang.Text;

namespace HexaEngine.ShadingLang
{
    public unsafe struct HXSLFunction : IFreeable
    {
        public TextSpan Span;
        public TextSpan Name;
        public HXSLType* ReturnType;
        public UnsafeList<Pointer<HXSLParameter>> Parameters;
        public HXSLFunctionFlags Flags;
        public TextSpan Semantic;
        public HXSLExpression ExpressionBody;

        public void Release()
        {
            Free(ReturnType);
            foreach (HXSLParameter* parameter in Parameters)
            {
                Free(parameter);
            }
            Parameters.Release();
            ExpressionBody.Release();
        }
    }
}