using Hexa.NET.Utilities;
using HexaEngine.ShadingLang.Text;

namespace HexaEngine.ShadingLang
{
    public struct HXSLExpression : IFreeable
    {
        public TextSpan Span;

        public void Release()
        {
        }
    }
}