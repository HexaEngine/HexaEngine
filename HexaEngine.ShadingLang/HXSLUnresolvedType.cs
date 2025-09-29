using Hexa.NET.Utilities;
using HexaEngine.Core.Graphics.Reflection;
using HexaEngine.ShadingLang.LexicalAnalysis;
using HexaEngine.ShadingLang.Text;

namespace HexaEngine.ShadingLang
{
    public unsafe struct HXSLUnresolvedType : IFreeable
    {
        public HXSLType Base;
        public Token Token;
        public fixed long Dummy[64];

        public void Release()
        {
            Base.ReleaseCore();
        }
    }
}