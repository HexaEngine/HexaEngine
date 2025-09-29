using HexaEngine.ShadingLang.Text;

namespace HexaEngine.ShadingLang
{
    public struct VariableReference
    {
        public TextSpan Span;
        public TextSpan PropertyName;
        public TextSpan ShaderPropertyName;
    }
}