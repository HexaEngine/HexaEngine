using Hexa.NET.Utilities;
using HexaEngine.ShadingLang.Text;

namespace HexaEngine.ShadingLang
{
    internal static unsafe class TextSpanExtensions
    {
        public static TextSpan AsTextSpan(this StdWString str)
        {
            return new(str.Data, 0, str.Size);
        }
    }
}